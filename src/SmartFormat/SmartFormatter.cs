﻿//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.ZString;

namespace SmartFormat;

/// <summary>
/// This class contains the Format method that constructs
/// the composite string by invoking each extension.
/// </summary>
public class SmartFormatter
{
    private readonly List<ISource> _sourceExtensions = new();
    private readonly List<IFormatter> _formatterExtensions = new();

    #region: Constructor :

    /// <summary>
    /// Creates a new instance of a <see cref="SmartFormatter"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="SmartSettings"/> to use, or <see langword="null"/> for default settings.
    /// Any changes after passing settings as a parameter may not have effect.
    /// </param>
    public SmartFormatter(SmartSettings? settings = null)
    {
        Settings = settings ?? new SmartSettings();
        Parser = new Parser(Settings);
    }

    #endregion

    #region : EventHandlers :

    /// <summary>
    /// Event raising, if an error occurs during formatting.
    /// </summary>
    public event EventHandler<FormattingErrorEventArgs>? OnFormattingFailure;

    #endregion

    #region: Extension Registry :

    /// <summary>
    /// Gets the list of <see cref="ISource" /> source extensions.
    /// </summary>
    internal List<ISource> SourceExtensions => _sourceExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="ISource" /> source extensions.
    /// </summary>
    public IReadOnlyList<ISource> GetSourceExtensions() => _sourceExtensions.AsReadOnly();

    /// <summary>
    /// Gets the list of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    internal List<IFormatter> FormatterExtensions => _formatterExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    public IReadOnlyList<IFormatter> GetFormatterExtensions() => _formatterExtensions.AsReadOnly();

    /// <summary>
    /// Adds <see cref="ISource"/> extensions to the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before. <see cref="WellKnownExtensionTypes.Sources"/> are inserted
    /// at the recommended position, all others are added at the end of the list.
    /// <para>
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </para>
    /// <para>
    /// Extensions implementing <see cref="ISource"/> <b>and</b> <see cref="IFormatter"/>
    /// will be auto-registered for both.
    /// </para>
    /// </summary>
    /// <param name="sourceExtensions"><see cref="ISource"/> extensions in an arbitrary order.</param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    public SmartFormatter AddExtensions(params ISource[] sourceExtensions)
    {
        foreach (var source in sourceExtensions)
        {
            var index = WellKnownExtensionTypes.GetIndexToInsert(SourceExtensions, source);
            _ = InsertExtension(index, source);

            // Also add the class as a formatter, if possible
            if (source is IFormatter formatter && FormatterExtensions.TrueForAll(fx => fx.GetType() != formatter.GetType())) AddExtensions(formatter);
        }

        return this;
    }

    /// <summary>
    /// Adds <see cref="IFormatter"/> extensions to the <see cref="GetFormatterExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before. <see cref="WellKnownExtensionTypes.Formatters"/> are inserted
    /// at the recommended position, all others are added at the end of the list.
    /// <para>
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </para>
    /// <para>
    /// Extensions implementing <see cref="ISource"/> <b>and</b> <see cref="IFormatter"/>
    /// will be auto-registered for both.
    /// </para>
    /// </summary>
    /// <param name="formatterExtensions"><see cref="ISource"/> extensions in an arbitrary order.</param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    public SmartFormatter AddExtensions(params IFormatter[] formatterExtensions)
    {
        foreach (var formatter in formatterExtensions)
        {
            var index = WellKnownExtensionTypes.GetIndexToInsert(FormatterExtensions, formatter);
            _ = InsertExtension(index, formatter);

            // Also add the class as a source, if possible
            if (formatter is ISource source && SourceExtensions.TrueForAll(sx => sx.GetType() != source.GetType())) AddExtensions(source);
        }

        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="sourceExtension"></param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public SmartFormatter InsertExtension(int position, ISource sourceExtension)
    {
        if (_sourceExtensions.Exists(sx => sx.GetType() == sourceExtension.GetType())) return this;

        if (sourceExtension is IInitializer sourceToInitialize)
            sourceToInitialize.Initialize(this);

        _sourceExtensions.Insert(position, sourceExtension);

        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="formatterExtension"></param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public SmartFormatter InsertExtension(int position, IFormatter formatterExtension)
    {
        if (_formatterExtensions.Exists(sx => sx.GetType() == formatterExtension.GetType())) return this;

        // Extension name is in use by a different type
        if (_formatterExtensions.Exists(fx => fx.Name.Equals(formatterExtension.Name)))
            throw new ArgumentException($"Formatter '{formatterExtension.GetType().Name}' uses existing name.", nameof(formatterExtension));

        if (formatterExtension is IInitializer formatterToInitialize)
            formatterToInitialize.Initialize(this);

        _formatterExtensions.Insert(position, formatterExtension);

        return this;
    }

    /// <summary>
    /// Searches for a Source Extension of the given type, and returns it.
    /// Returns <see langword="null"/> if the type cannot be found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The class implementing <see cref="ISource"/> if found, else <see langword="null"/>.</returns>
    public T? GetSourceExtension<T>() where T : class, ISource
    {
        return _sourceExtensions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Searches for a Formatter Extension of the given type, and returns it.
    /// Returns <see langword="null"/> if the type cannot be found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The class implementing <see cref="IFormatter"/> if found, else <see langword="null"/>.</returns>
    public T? GetFormatterExtension<T>() where T : class, IFormatter
    {
        return _formatterExtensions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Removes Source Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveSourceExtension<T>() where T : class, ISource
    {
        var source = _sourceExtensions.OfType<T>().FirstOrDefault();
        return source is not null && _sourceExtensions.Remove(source);
    }

    /// <summary>
    /// Removes the Formatter Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveFormatterExtension<T>() where T : class, IFormatter
    {
        var format = _formatterExtensions.OfType<T>().FirstOrDefault();
        return format is not null && _formatterExtensions.Remove(format);
    }

    #endregion

    #region: Properties :

    /// <summary>
    /// Gets or set the instance of the <see cref="Core.Parsing.Parser" />
    /// </summary>
    public Parser Parser { get; }

    /// <summary>
    /// Get the <see cref="SmartSettings" /> for Smart.Format
    /// </summary>
    public SmartSettings Settings { get; }

    #endregion

    #region: Format Overloads :

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(string format, params object?[] args)
    {
        return Format(null, format, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(string format, IList<object?> args)
    {
        return Format(null, format, args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, string format, params object?[] args)
    {
        return Format(provider, format, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, string format, IList<object?> args)
    {
        var formatParsed = Parser.ParseFormat(format);
        try
        {
            var result = Format(provider, formatParsed, args);
            return result;
        }
        finally
        {
            FormatPool.Instance.Return(formatParsed); // The parser gets the Format from the pool 
        }
    }

    #region ** Format overloads with cached Format **

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(Format formatParsed, params object?[] args)
    {
        return Format(null, formatParsed, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(Format formatParsed, IList<object?> args)
    {
        return Format(null, formatParsed, args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, Format formatParsed, params object?[] args)
    {
        return Format(provider, formatParsed, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, Format formatParsed, IList<object?> args)
    {
        // Note: Making ZStringOutput a class instance variable has no advantage for speed,
        // but brings 10% less Gen 0 GC. Then, SmartFormatter would have to be IDisposable (to dispose ZStringOutput)
        using var zsOutput = new ZStringOutput(ZStringBuilderUtilities.CalcCapacity(formatParsed));
        FormatInto(zsOutput, provider, formatParsed, args);
        return zsOutput.ToString();
    }

    #endregion

    /// <summary>
    /// Format the <see cref="FormattingInfo" /> argument.
    /// </summary>
    /// <param name="formattingInfo"></param>
    public void Format(FormattingInfo formattingInfo)
    {
        // Before we start, make sure we have at least one source extension and one formatter extension:
        CheckForExtensions();
        if (formattingInfo.Format is null) return;

        foreach (var item in formattingInfo.Format.Items)
        {
            if (item is LiteralText literalItem)
            {
                formattingInfo.Write(literalItem.AsSpan());
                continue;
            }

            // Otherwise, the item must be a placeholder.
            var placeholder = (Placeholder) item;
            var childFormattingInfo = formattingInfo.CreateChild(placeholder);
            try
            {
                EvaluateSelectors(childFormattingInfo);
            }
            catch (Exception ex)
            {
                // An error occurred while evaluation selectors
                var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors[placeholder.Selectors.Count - 1].EndIndex;
                FormatError(item, ex, errorIndex, childFormattingInfo);
                continue;
            }

            try
            {
                EvaluateFormatters(childFormattingInfo);
            }
            catch (Exception ex)
            {
                // An error occurred while evaluating formatters
                var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors[placeholder.Selectors.Count - 1].EndIndex;
                FormatError(item, ex, errorIndex, childFormattingInfo);
            }
        }
    }

    #endregion

    #region: FormatInto Overloads :

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, string format, params object?[] args)
    {
        FormatInto(output, format, (IList<object?>) args);
    }

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, string format, IList<object?> args)
    {
        FormatInto(output, null, format, args);
    }

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, string format, IList<object?> args)
    {
        var formatParsed = Parser.ParseFormat(format);
        try
        {
            FormatInto(output, provider, formatParsed, args);
        }
        finally
        {
            FormatPool.Instance.Return(formatParsed); // The parser gets the Format from the pool    
        }
    }

    #region: FormatInto Overloads with cached Format :

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="format">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, Format format, params object?[] args)
    {
        FormatInto(output, provider, format, (IList<object?>) args);
    }

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, Format formatParsed, IList<object?> args)
    {
        PerformActionWithFormattingInfo(this, provider, formatParsed, args, output, Format);
    }

    #endregion

    #endregion

    #region: Private methods :

    internal void FormatError(FormatItem errorItem, Exception innerException, int startIndex,
        IFormattingInfo formattingInfo)
    {
        OnFormattingFailure?.Invoke(this,
            new FormattingErrorEventArgs(errorItem.RawText, startIndex,
                Settings.Formatter.ErrorAction != FormatErrorAction.ThrowError));
        switch (Settings.Formatter.ErrorAction)
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

    private void CheckForExtensions()
    {
        if (_sourceExtensions.Count == 0)
            throw new InvalidOperationException(
                "No source extensions are available. Please add at least one source extension, such as the DefaultSource.");
        if (_formatterExtensions.Count == 0)
            throw new InvalidOperationException(
                "No formatter extensions are available. Please add at least one formatter extension, such as the DefaultFormatter.");
    }

    /// <summary>
    /// Evaluates all <see cref="Selector"/>s of a <see cref="Placeholder"/>.
    /// </summary>
    /// <remarks>
    /// Note: If there is no selector (like {:0.00}), <see cref="FormattingInfo.CurrentValue"/> is left unchanged.
    /// <br/>
    /// Child formats <b>inside <see cref="Placeholder"/>s</b> are evaluated in <see cref="DefaultFormatter"/>.
    /// Example: "{ChildOne.ChildTwo.ChildThree:{}{Four}}" where "{} and {Four}" are child placeholders.
    /// </remarks>
    /// <param name="formattingInfo"></param>
    /// <exception cref="FormattingException"></exception>
    private void EvaluateSelectors(FormattingInfo formattingInfo)
    {
        if (formattingInfo.Placeholder is null) return;

        var firstSelector = true;
        foreach (var selector in formattingInfo.Placeholder.Selectors)
        {
            // Don't evaluate empty selectors
            // (used e.g. for Settings.Parser.NullableOperator and Settings.Parser.ListIndexEndChar final operators)
            if (selector.Length == 0) continue;

            // Ensure ISelectorInfo implementation returns the current selector values
            formattingInfo.Selector = selector;
            // Do not evaluate alignment-only selectors
            if (formattingInfo.SelectorOperator.Length > 0 &&
                formattingInfo.SelectorOperator[0] == Settings.Parser.AlignmentOperator) continue;

            // The result for the selector will be set by a source extension.
            formattingInfo.Result = null;

            var handled = InvokeSourceExtensions(formattingInfo);
            // Set the value that will be used for formatting
            if (handled) formattingInfo.CurrentValue = formattingInfo.Result;

            if (firstSelector)
            {
                firstSelector = false;
                // Handle "nested scopes" like "{ChildOne.ChildTwo.ChildThree:{}{:Four}}" where "{} and {:Four}" are child placeholders.
                // by traversing the stack:
                var parentFormattingInfo = formattingInfo;
                while (!handled && parentFormattingInfo.Parent != null)
                {
                    parentFormattingInfo = parentFormattingInfo.Parent;
                    // Ensure ISelectorInfo implementation returns the current selector values
                    parentFormattingInfo.Selector = selector;
                    // The result for the selector will be set by a source extension.
                    parentFormattingInfo.Result = null;
                    handled = InvokeSourceExtensions(parentFormattingInfo);
                    // Set the value that will be used for formatting
                    if (handled) formattingInfo.CurrentValue = parentFormattingInfo.Result;
                }
            }

            if (!handled)
                throw formattingInfo.FormattingException($"No source extension could handle the selector named \"{selector.RawText}\"",
                    selector);
        }
    }

    private bool InvokeSourceExtensions(ISelectorInfo selectorInfo)
    {
        // less GC than using Linq
        foreach (var sourceExtension in _sourceExtensions)
        {
            var handled = sourceExtension.TryEvaluateSelector(selectorInfo);
            if (handled) return true;
        }

        return false;
    }

    /// <summary>
    /// Try to get a suitable formatter.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <exception cref="FormattingException"></exception>
    private void EvaluateFormatters(FormattingInfo formattingInfo)
    {
        var handled = InvokeFormatterExtensions(formattingInfo);
        if (!handled)
            throw formattingInfo.FormattingException("No suitable Formatter could be found", formattingInfo.Format);
    }

    /// <summary>
    /// First check whether the named formatter name exist in of the <see cref="FormatterExtensions" />,
    /// next check whether the named formatter is able to process the format.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <returns>True if an FormatterExtension was found, else False.</returns>
    private bool InvokeFormatterExtensions(FormattingInfo formattingInfo)
    {
        if (formattingInfo.Placeholder is null)
        {
            throw new ArgumentException($"The property {nameof(formattingInfo)}.{nameof(formattingInfo.Placeholder)} must not be null.", nameof(formattingInfo));
        }

        var formatterName = formattingInfo.Placeholder.FormatterName;
        var comparison = Settings.GetCaseSensitivityComparison();

        // Compatibility mode does not support formatter extensions except this one:
        if (Settings.StringFormatCompatibility)
        {
            return
                _formatterExtensions.First(fe => fe is DefaultFormatter)
                    .TryEvaluateFormat(formattingInfo);
        }

        // Try to evaluate using the not empty formatter name from the format string
        if (formatterName != string.Empty)
        {
            IFormatter? formatterExtension = null;
            // less GC than using Linq
            foreach (var fe in _formatterExtensions)
            {
                if (!fe.Name.Equals(formatterName, comparison)) continue;

                formatterExtension = fe;
                break;
            }

            if (formatterExtension is null)
                throw formattingInfo.FormattingException($"No formatter with name '{formatterName}' found",
                    formattingInfo.Format, formattingInfo.Selector?.SelectorIndex ?? -1);

            return formatterExtension.TryEvaluateFormat(formattingInfo);
        }

        // Go through all (implicit) formatters which contain an empty name
        // much higher performance and less GC than using Linq
        foreach (var fe in _formatterExtensions)
        {
            if (!fe.CanAutoDetect) continue;
            if (fe.TryEvaluateFormat(formattingInfo)) return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new instance of <see cref="FormattingInfo"/>
    /// and performs the <see paramref="work"/> action
    /// using the <see cref="FormattingInfo"/>.
    /// </summary>
    /// <param name="formatter"></param>
    /// <param name="provider"></param>
    /// <param name="formatParsed"></param>
    /// <param name="args">The data argument.</param>
    /// <param name="output"></param>
    /// <param name="doWork">The <see cref="Action{T}"/>to invoke.</param>
    /// <remarks>
    /// The method uses object pooling to reduce GC pressure,
    /// and assures that objects are returned to the pool after
    /// <see paramref="work"/> is done (or an exception is thrown).
    /// </remarks>
    private static void PerformActionWithFormattingInfo(SmartFormatter formatter, IFormatProvider? provider, Format formatParsed, IList<object?> args, IOutput? output, Action<FormattingInfo> doWork)
    {
        var current = args.Count > 0 ? args[0] : args; // The first item is the default.
        using var fdo = FormatDetailsPool.Instance.Pool.Get(out var formatDetails);
        formatDetails.Initialize(formatter, formatParsed, args, provider, output ?? new ZStringOutput(ZStringBuilderUtilities.CalcCapacity(formatParsed)));
        using var fio = FormattingInfoPool.Instance.Pool.Get(out var formattingInfo);
        formattingInfo.Initialize(formatDetails, formatParsed, current);
        doWork(formattingInfo);
    }

    #endregion
}
