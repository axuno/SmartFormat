//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat;

/// <summary>
/// The <see cref="Evaluator"/> class evaluates the <see cref="Format"/>s and <see cref="Placeholder"/>s.
/// using <see cref="ISource"/> extensions, and formats these values using <see cref="IFormatter"/> extensions.
/// Additionally, it handles errors that occur during evaluation, and provides an event for error handling.
/// </summary>
internal partial class Evaluator
{
    private readonly SmartFormatter _formatter;
    private readonly SmartSettings _settings;
    private readonly Registry _registry;

    /// <summary>
    /// Creates a new instance of the <see cref="Evaluator"/> class.
    /// </summary>
    /// <param name="formatter"></param>
    public Evaluator(SmartFormatter formatter)
    {
        _formatter = formatter;
        _settings = _formatter.Settings;
        _registry = _formatter.Registry;
    }

    /// <summary>
    /// Writes all items of the <see cref="FormattingInfo.Format"/> property of the <paramref name="formattingInfo"/>
    /// to the <paramref name="formattingInfo"/>.<see cref="FormatDetails.Output"/>.
    /// </summary>
    /// <param name="formattingInfo">The <see cref="FormattingInfo.Format"/> must not be null.</param>
    public void WriteFormat(FormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format!;

        OnFormat?.Invoke(this, new FormatEventArgs(format));
        foreach (var item in format.Items)
        {
            if (item is LiteralText literalItem)
            {
                OnLiteral?.Invoke(this, new LiteralEventArgs(literalItem.ToString()));

                // Note: Literals must also respect FormattingInfo.Alignment,
                // so we must use FormattingInfo.Write here as well.
                formattingInfo.Write(literalItem.ToString().AsSpan());
                continue;
            }

            // Otherwise, the item must be a placeholder.
            var placeholder = (Placeholder) item;
            var childFormattingInfo = formattingInfo.CreateChild(placeholder);
            OnPlaceholder?.Invoke(this, new PlaceholderEventArgs(placeholder));

            // Try to get a value for the placeholder...
            if (!EvaluatePlaceholder(childFormattingInfo)) continue;
            // ... and format it.
            InvokeFormatters(childFormattingInfo);
        }
    }

    /// <summary>
    /// Tries to get the value for a <see cref="Placeholder"/>s by evaluating its <see cref="Selector"/>s from <see cref="ISource"/> extensions.
    /// </summary>
    /// <param name="formattingInfo">The <see cref="FormattingInfo.Placeholder"/> must be set.</param>
    /// <returns><see langword="true"/>, if evaluating the placeholder was successful.</returns>
    /// <exception cref="FormattingException">Throws if no <see cref="ISource"/> could get the value.</exception>
    private bool EvaluatePlaceholder(FormattingInfo formattingInfo)
    {
        formattingInfo.Result = null;

        var placeholder = formattingInfo.Placeholder!;

        try
        {
            EvaluateSelectors(formattingInfo);
        }
        catch (Exception ex)
        {
            var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors[placeholder.Selectors.Count - 1].EndIndex;
            FormatError(placeholder, ex, errorIndex, formattingInfo);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the value for a <see cref="Placeholder"/>s by evaluating its <see cref="Selector"/>s from <see cref="ISource"/> extensions.
    /// </summary>
    /// <param name="parentFormattingInfo"></param>
    /// <param name="placeholder"></param>
    /// <param name="result">The value for the given <paramref name="placeholder"/> from the registered <see cref="Core.Extensions.ISource"/>s.</param>
    /// <returns><see langword="true"/>, if one of the <see cref="ISource"/> returned a value.</returns>
    public bool TryGetValue(FormattingInfo parentFormattingInfo, Placeholder placeholder, out object? result)
    {
        result = null;
        if (placeholder.Selectors.Count == 0)
        {
            result = parentFormattingInfo.CurrentValue;
            return true;
        }

        // We must create a new FormattingInfo for the placeholder
        var fi = FormattingInfoPool.Instance.Get().Initialize(parentFormattingInfo, parentFormattingInfo.FormatDetails,
            placeholder, parentFormattingInfo.CurrentValue);
        parentFormattingInfo.Children.Add(fi);

        // If evaluating selectors is successful, the result is stored in fi.CurrentValue
        try
        {
            EvaluateSelectors(fi);
            result = fi.CurrentValue;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Evaluates all <see cref="Selector"/>s of a <see cref="Placeholder"/>.
    /// </summary>
    /// <remarks>
    /// Note: If there is no selector (like {:0.00}), <see cref="FormattingInfo.CurrentValue"/> is left unchanged.
    /// <br/>
    /// Child formats <b>inside <see cref="Placeholder"/>s</b> are evaluated with <see cref="Extensions.DefaultFormatter"/>.
    /// Example: "{ChildOne.ChildTwo.ChildThree:{}{Four}}" where "{} and {Four}" are child placeholders.
    /// </remarks>
    /// <param name="formattingInfo">The <see cref="FormattingInfo.Placeholder"/> must be set.</param>
    /// <exception cref="FormattingException"></exception>
    private void EvaluateSelectors(FormattingInfo formattingInfo)
    {
        // The result for the selector will be set by a source extension.
        formattingInfo.Result = null;

        var firstSelector = true;
#pragma warning disable S3267 // Don't use LINQ in favor of less GC
        foreach (var selector in formattingInfo.Placeholder!.Selectors)
        {
            if (SkipThisSelector(selector)) continue;

            // Ensure ISelectorInfo implementation returns the current selector values
            formattingInfo.Selector = selector;

            var handled = _registry.InvokeSourceExtensions(formattingInfo);
            switch (handled.Success)
            {
                case true:
                    // Set the value that will be used for formatting
                    formattingInfo.CurrentValue = formattingInfo.Result;
                    OnSelectorValue?.Invoke(this, new SelectorValueEventArgs(selector, handled.Success, handled.SourceType, formattingInfo.CurrentValue));
                    break;
                case false when firstSelector:
                    firstSelector = false;
                    HandleNestedScope(formattingInfo, selector, ref handled);
                    OnSelectorValue?.Invoke(this,new SelectorValueEventArgs(selector, handled.Success, handled.SourceType, formattingInfo.CurrentValue));
                    break;
            }

            if (handled.Success) continue;

            OnSelectorFailure?.Invoke(this, new SelectorValueEventArgs(selector, handled.Success, null, null));
            throw formattingInfo.FormattingException($"No source extension could handle the selector named \"{selector.RawText}\"",
                selector);
        }
#pragma warning restore S3267 // Don't use LINQ in favor of less GC
    }

    /// <summary>
    /// Handles "nested scopes" like "{ChildOne.ChildTwo.ChildThree:{}{:Four}}" where "{} and {:Four}" are child placeholders.
    /// by traversing the stack.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <param name="selector"></param>
    /// <param name="handled"></param>
    private void HandleNestedScope(FormattingInfo formattingInfo, Selector selector, ref (bool Success, Type? SourceType) handled)
    {
        var parentFormattingInfo = formattingInfo;
        while (!handled.Success && parentFormattingInfo.Parent != null)
        {
            parentFormattingInfo = parentFormattingInfo.Parent;
            // Ensure ISelectorInfo implementation returns the current selector values
            parentFormattingInfo.Selector = selector;
            // The result for the selector will be set by a source extension.
            parentFormattingInfo.Result = null;
            // recursive call
            handled = _registry.InvokeSourceExtensions(parentFormattingInfo);
            // Set the value that will be used for formatting
            if (handled.Success) formattingInfo.CurrentValue = parentFormattingInfo.Result;
        }
    }

    /// <summary>
    /// Skip empty selectors and alignment-only selectors.
    /// </summary>
    private bool SkipThisSelector(Selector selector)
    {
        // Don't evaluate empty selectors
        // (used e.g. for Settings.Parser.NullableOperator and Settings.Parser.ListIndexEndChar final operators)
        if (selector.Length == 0) return true;

        // Do not evaluate alignment-only selectors
        if (selector.Operator.Length > 0 &&
            selector.Operator[0] == _settings.Parser.AlignmentOperator) return true;

        return false;
    }

    /// <summary>
    /// Try to get a suitable formatter and invoke it.
    /// </summary>
    /// <param name="formattingInfo">The <see cref="FormattingInfo.Placeholder"/> must be set.</param>
    /// <exception cref="FormattingException"></exception>
    private void InvokeFormatters(FormattingInfo formattingInfo)
    {
        var placeholder = formattingInfo.Placeholder!;
        
        try
        {
            OnFormattingStart?.Invoke(this, new FormattingEventArgs(formattingInfo.Selector!, formattingInfo.CurrentValue, true, null));
            var handled = _registry.InvokeFormatterExtensions(formattingInfo);
            OnFormattingEnd?.Invoke(this, new FormattingEventArgs(formattingInfo.Selector!, formattingInfo.CurrentValue, handled.Success, handled.FormatterType));

            if (!handled.Success)
                throw formattingInfo.FormattingException("No suitable Formatter could be found", formattingInfo.Format, formattingInfo.Selector?.SelectorIndex ?? -1);
        }
        catch (Exception ex)
        {
            // An error occurred while evaluating formatters
            var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors[placeholder.Selectors.Count - 1].EndIndex;
            FormatError(placeholder, ex, errorIndex, formattingInfo);
        }

        return;
    }

    private void FormatError(FormatItem errorItem, Exception innerException, int startIndex,
        IFormattingInfo formattingInfo)
    {
        var errorArgs = new FormattingErrorEventArgs(errorItem.RawText, startIndex,
            _settings.Formatter.ErrorAction != FormatErrorAction.ThrowError);
        
        OnFormattingFailure?.Invoke(this, errorArgs);
        // Invoke the formatters' error handler, too:
        _formatter.FormatError(errorArgs);

        switch (_settings.Formatter.ErrorAction)
        {
            case FormatErrorAction.Ignore:
                return;
            case FormatErrorAction.ThrowError:
                throw innerException as FormattingException ??
                      new FormattingException(errorItem, innerException, startIndex);
            case FormatErrorAction.OutputErrorInResult:
                formattingInfo.FormatDetails.FormattingException =
                    innerException as FormattingException ??
                    new FormattingException(errorItem, innerException, startIndex);
                formattingInfo.Write(innerException.Message);
                formattingInfo.FormatDetails.FormattingException = null;
                break;
            case FormatErrorAction.MaintainTokens:
                formattingInfo.Write(formattingInfo.Placeholder?.RawText ?? "'null'");
                break;
        }
    }
}



