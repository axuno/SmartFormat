// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Formatting;

/// <summary>
/// The class contains the fields and methods which are necessary for formatting.
/// </summary>
public class FormattingInfo : IFormattingInfo, ISelectorInfo
{
    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public FormattingInfo()
    {
        FormatDetails = InitializationObject.FormatDetails;
    }

    /// <summary>
    /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
    /// </summary>
    /// <param name="formatDetails"></param>
    /// <param name="format"></param>
    /// <param name="currentValue"></param>
    public FormattingInfo Initialize(FormatDetails formatDetails, Format format, object? currentValue)
    {
        return Initialize(null, formatDetails, format, currentValue);
    }

    /// <summary>
    /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="formatDetails"></param>
    /// <param name="format"></param>
    /// <param name="currentValue"></param>
    public FormattingInfo Initialize (FormattingInfo? parent, FormatDetails formatDetails, Format format, object? currentValue)
    {
        Parent = parent;
        CurrentValue = currentValue;
        FormatDetails = formatDetails;
        Format = format;
        // inherit alignment
        if (parent != null) Alignment = parent.Alignment;
        else if (format.ParentPlaceholder != null) Alignment = format.ParentPlaceholder.Alignment;

        return this;
    }

    /// <summary>
    /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="formatDetails"></param>
    /// <param name="placeholder"></param>
    /// <param name="currentValue"></param>
    public FormattingInfo Initialize (FormattingInfo? parent, FormatDetails formatDetails, Placeholder placeholder,
        object? currentValue)
    {
        Parent = parent;
        FormatDetails = formatDetails;
        Placeholder = placeholder;
        Format = placeholder.Format;
        CurrentValue = currentValue;
        // inherit alignment
        Alignment = placeholder.Alignment;

        return this;
    }

    /// <summary>
    /// Returns this instance and its <see cref="FormattingInfo"/> children to the object pool.
    /// This method gets called by <see cref="FormattingInfoPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.
    /// </summary>
    public void ReturnToPool()
    {
        Parent = null;
        // Assign new value, instance is returned to the pool elsewhere
        FormatDetails = InitializationObject.FormatDetails;
        Placeholder = null;
        Selector = null;
        Alignment = 0;
            
        Format = null;
        CurrentValue = null;

        // Children can safely be returned
        foreach (var c in Children)
        {
            FormattingInfoPool.Instance.Return(c);
        }

        Children.Clear();
    }

    /// <summary>
    /// Gets the parent <see cref="FormattingInfo"/>.
    /// </summary>
    public FormattingInfo? Parent { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="Parsing.Selector"/>.
    /// </summary>
    public Selector? Selector { get; internal set; }

    /// <summary>
    /// Gets the <see cref="FormatDetails"/>.
    /// </summary>
    public FormatDetails FormatDetails { get; private set; }

    /// <summary>
    /// Gets or sets the current value that is going to be formatted.
    /// </summary>
    public object? CurrentValue { get; set; }

    /// <summary>
    /// Gets the <see cref="Placeholder"/>.
    /// </summary>
    public Placeholder? Placeholder { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Alignment"/> of the current <see cref="Placeholder"/>,
    /// or - if this is <see langword="null"/> - the <see cref="Alignment"/>
    /// of any parent <see cref="IFormattingInfo"/> that is not zero.
    /// </summary>
    public int Alignment { get; set; }

    /// <summary>
    /// Gets the <see cref="FormatterOptions"/> of a <see cref="Placeholder"/>.
    /// </summary>
    public string FormatterOptions => Placeholder?.FormatterOptions ?? string.Empty;

    /// <summary>
    /// Gets the <see cref="Format"/>.
    /// </summary>
    public Format? Format { get; private set; }

    /// <summary>
    /// Gets the list of child <see cref="FormattingInfo"/>s created by this instance.
    /// </summary>
    internal List<FormattingInfo> Children { get; } = new();

    /// <summary>
    /// Writes the <see cref="string"/> parameter to the <see cref="Output.IOutput"/>
    /// and takes care of alignment.
    /// </summary>
    /// <param name="text">The string to write to the <see cref="Output.IOutput"/></param>
    public void Write(string text)
    {
        Write(text.AsSpan());
    }

    /// <summary>
    /// Writes the <see cref="ReadOnlySpan{T}"/> text parameter to the <see cref="Output.IOutput"/>
    /// and takes care of alignment.
    /// </summary>
    /// <param name="text">The string to write to the <see cref="Output.IOutput"/></param>
    public void Write(ReadOnlySpan<char> text)
    {
        if (Alignment == 0)
        {
            FormatDetails.Output.Write(text);
            FormatDetails.Formatter.Evaluator.OnOutputWritten?.Invoke(this,
                new Evaluator.OutputWrittenEventArgs(text.ToString()));
            return;
        }

        // Create a buffer to store the aligned text
        var totalLength = Math.Max(text.Length, Math.Abs(Alignment));
        var alignedTextBuffer = ArrayPool<char>.Shared.Rent(totalLength);

        try
        {
            // Fill with pre-alignment character
            var filler = Alignment - text.Length;
            if (filler > 0)
            {
                alignedTextBuffer.AsSpan(0, filler).Fill(FormatDetails.Settings.Formatter.AlignmentFillCharacter);
            }

            text.CopyTo(alignedTextBuffer.AsSpan(Math.Max(0, filler)));

            // Fill with post-alignment character
            filler = -Alignment - text.Length;
            if (filler > 0)
            {
                alignedTextBuffer.AsSpan(text.Length).Fill(FormatDetails.Settings.Formatter.AlignmentFillCharacter);
            }

            // Write the aligned text to the output
            FormatDetails.Output.Write(alignedTextBuffer.AsSpan(0, totalLength));

            FormatDetails.Formatter.Evaluator.OnOutputWritten?.Invoke(this,
                new Evaluator.OutputWrittenEventArgs(alignedTextBuffer.AsSpan(0, totalLength).ToString()));
        }
        finally
        {
            ArrayPool<char>.Shared.Return(alignedTextBuffer);
        }
    }

    /// <summary>
    /// Creates a child <see cref="FormattingInfo"/> from the current <see cref="FormattingInfo"/> instance
    /// and invokes the <see cref="Evaluator"/> to evaluate the <paramref name="format"/> items with the <paramref name="value"/>.
    /// </summary>
    /// <param name="format">The <see cref="Parsing.Format"/> to use.</param>
    /// <param name="value">The value to use for evaluation.</param>
    public void FormatAsChild(Format format, object? value)
    {
        // recursive call
        FormatDetails.Formatter.Evaluator.WriteFormat(CreateChild(format, value));
    }

    /// <summary>
    /// Uses the <paramref name="current"/> value to format the <paramref name="format"/>
    /// and returns the result as a <see cref="Span{T}"/>.
    /// <para/>
    /// This method aims to be used by <see cref="IFormatter"/> implementations.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider"/>, or null for using the default.</param>
    /// <param name="format">The format that will be formatted.</param>
    /// <param name="current">The data object used for formatting.</param>
    /// <returns>A <see cref="Span{T}"/> with the formatting result.</returns>
    public Span<char> FormatAsSpan(IFormatProvider? provider, Format format, object? current)
    {
        using var zsPo = ZStringOutputPool.Instance.Get(out var output);
        ExecuteFormattingAction(provider, format, current, output, FormatDetails.Formatter.Evaluator.WriteFormat);

        // Copy the result to an array buffer, because output gets disposed.
        return output.Output.AsSpan().ToArray().AsSpan();
    }

    /// <summary>
    /// Uses the <paramref name="current"/> value to format the <paramref name="placeholder"/>
    /// and returns the result as a <see cref="Span{T}"/>.
    /// <para/>
    /// This method aims to be used by <see cref="IFormatter"/> implementations.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider"/>, or null for using the default.</param>
    /// <param name="placeholder">The placeholder that will be formatted.</param>
    /// <param name="current">The data object used for formatting.</param>
    /// <returns>A <see cref="Span{T}"/> with the formatting result.</returns>
    public Span<char> FormatAsSpan(IFormatProvider? provider, Placeholder placeholder, object? current)
    {
        using var fmtObject = FormatPool.Instance.Get(out var format);
        format.Initialize(FormatDetails.Settings, placeholder.BaseString);
        format.Items.Add(placeholder);

        return FormatAsSpan(provider, format, current);
    }

    /// <summary>
    /// Tries to get the value for the given <paramref name="placeholder"/> from the registered <see cref="ISource"/>s.
    /// </summary>
    /// <param name="placeholder"></param>
    /// <param name="result"></param>
    /// <returns><see langword="true"/>, if one of the <see cref="ISource"/> returned a value.</returns>
    public bool TryGetValue(Placeholder placeholder, out object? result)
    {
        return FormatDetails.Formatter.Evaluator.TryGetValue(this, placeholder, out result);
    }

    /// <summary>
    /// Creates a new <see cref="FormattingException"/>.
    /// </summary>
    /// <param name="issue">The text which goes to the <see cref="Exception.Message"/>.</param>
    /// <param name="problemItem">The <see cref="FormatItem"/> which caused the problem.</param>
    /// <param name="startIndex">The start index in the input format string.</param>
    /// <returns></returns>
    public FormattingException FormattingException(string issue, FormatItem? problemItem = null, int startIndex = -1)
    {
        problemItem ??= Format;
        if (startIndex == -1) startIndex = problemItem?.StartIndex ?? -1;
        return new FormattingException(problemItem, issue, startIndex);
    }

    /// <summary>
    /// Gets the (raw) text of the <see cref="Parsing.Selector"/>.
    /// </summary>
    public string SelectorText => Selector?.RawText ?? string.Empty;
        
    /// <summary>
    /// Gets index of the <see cref="Parsing.Selector"/> in the selector list.
    /// </summary>
    public int SelectorIndex => Selector?.SelectorIndex ?? -1;
        
    /// <summary>
    /// Gets the operator string of the <see cref="Parsing.Selector"/> (e.g.: comma, dot).
    /// </summary>
    public string SelectorOperator => Selector?.Operator ?? string.Empty;

    /// <summary>
    /// <see cref="ISource"/>s store the result of their assignment in the <see cref="Result"/> property.
    /// In order to be used during formatting, the <see cref="Result"/> must transition
    /// to the <see cref="CurrentValue"/> of the <see cref="IFormattingInfo"/>.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Creates a child <see cref="FormattingInfo"/> from the current <see cref="FormattingInfo"/> instance for a <see cref="Parsing.Format"/>.
    /// </summary>
    /// <param name="format">The <see cref="Parsing.Format"/> used for creating a child <see cref="IFormattingInfo"/>.</param>
    /// <param name="currentValue">The value to use for the child.</param>
    /// <returns>The child <see cref="FormattingInfo"/>.</returns>
    private FormattingInfo CreateChild(Format format, object? currentValue)
    {
        var fi = FormattingInfoPool.Instance.Get().Initialize(this, FormatDetails, format, currentValue);
        Children.Add(fi);
        return fi;
    }

    /// <summary>
    /// Creates a child <see cref="FormattingInfo"/> from the current <see cref="FormattingInfo"/> instance for a <see cref="Parsing.Placeholder"/>.
    /// </summary>
    /// <param name="placeholder">The <see cref="Parsing.Placeholder"/> used for creating a child <see cref="IFormattingInfo"/>.</param>
    /// <returns>The child <see cref="FormattingInfo"/>.</returns>
    public FormattingInfo CreateChild(Placeholder placeholder)
    {
        var fi = FormattingInfoPool.Instance.Get().Initialize(this, FormatDetails, placeholder, CurrentValue);
        Children.Add(fi);
        return fi;
    }

    /// <summary>
    /// Creates a new instance of <see cref="FormattingInfo"/>
    /// and performs the <see paramref="doWork"/> action
    /// using the <see cref="FormattingInfo"/>.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider"/>, or null for using the default.</param>
    /// <param name="formatParsed">The format that will be formatted.</param>
    /// <param name="current">The data object used for formatting.</param>
    /// <param name="output">The <see cref="IOutput"/> to use, or null for using a <b>new instance</b> of the default <seealso cref="ZStringOutput"/>.</param>
    /// <param name="doWork">The <see cref="Action{T}"/>to invoke.</param>
    /// <remarks>
    /// The method uses object pooling to reduce GC pressure,
    /// and assures that objects are returned to the pool after
    /// <see paramref="doWork"/> is done (or an exception is thrown).
    /// </remarks>
    internal void ExecuteFormattingAction(IFormatProvider? provider, Format formatParsed, object? current, IOutput output, Action<FormattingInfo> doWork)
    {
        var formatter = FormatDetails.Formatter;

        using var fdo = FormatDetailsPool.Instance.Pool.Get(out var formatDetails);
        formatDetails.Initialize(formatter, formatParsed, formatDetails.OriginalArgs, provider, output);

        using var fio = FormattingInfoPool.Instance.Pool.Get(out var formattingInfo);
        formattingInfo.Initialize(formatDetails, formatParsed, current);

        doWork(formattingInfo);
    }
}
