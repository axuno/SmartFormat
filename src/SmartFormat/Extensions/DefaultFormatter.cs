// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions;

#if NET6_0_OR_GREATER
/// <summary>
/// Does the default formatting.
/// This formatter in always required, unless you implement your own.
/// <pre/>
/// It supports <see cref="ISpanFormattable"/>, <see cref="IFormattable"/>, and <see cref="ICustomFormatter"/>.
/// </summary>
#else
/// <summary>
/// Does the default formatting.
/// This formatter in always required, unless you implement your own.
/// <pre/>
/// It supports <see cref="IFormattable"/> and <see cref="ICustomFormatter"/>.
/// </summary>
#endif
public class DefaultFormatter : IFormatter
{

#if NET6_0_OR_GREATER
    /// <summary>
    /// The maximum size of the stack-allocated buffer
    /// for formatting <see cref="System.ISpanFormattable"/> objects.
    /// </summary>
    internal const int StackAllocCharBufferSize = 512;
#endif
    
    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    public string[] Names { get; set; } = { "default", "d", string.Empty };

    ///<inheritdoc/>
    public string Name { get; set; } = "d";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = true;
        
    /// <summary>
    /// Checks, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/>.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <returns>Returns true, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/></returns>
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;
        var current = formattingInfo.CurrentValue;
            
        // If the format has nested placeholders, we process those first
        // instead of formatting the item. Like with "{2:list:{:{FirstName}}|, }"
        if (format is {HasNested: true})
        {
            formattingInfo.FormatAsChild(format, current);
            return true;
        }

        /* 
         * The order of precedence is:
         * 1. ICustomFormatter from the IFormatProvider
         * 2. ISpanFormattable (for .NET 6.0 or later)
         * 3. IFormattable
         * 4. ToString
         */

        var provider = formattingInfo.FormatDetails.Provider;
        if (provider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter cFormatter)
        {
            var formatText = format?.GetLiteralText();
            formattingInfo.Write(cFormatter.Format(formatText, current, provider).AsSpan());
            return true;
        }

#if NET6_0_OR_GREATER
        if (current is ISpanFormattable spanFormattable)
        {
            // ISpanFormattable has the same speed as IFormattable,
            // but brings less GC pressure (e.g. 25% less for processing 1234567.890123f).

            var fmtTextSpan = format != null ? format.AsSpan() : Span<char>.Empty;

            // Try to use the stack buffer first
            Span<char> buffer = stackalloc char[StackAllocCharBufferSize];

            if (spanFormattable.TryFormat(buffer, out var written, fmtTextSpan, provider))
            {
                formattingInfo.Write(buffer.Slice(0, written));
                return true;
            }

            // If the stack buffer is too small, use a heap buffer
            using var arrayBuffer = new ZString.ZCharArray(2_000_000);
            arrayBuffer.Write(spanFormattable, fmtTextSpan, provider);
            formattingInfo.Write(arrayBuffer.GetSpan());
            return true;
        }
#endif

        if (current is IFormattable formattable)
        {
            var fmtTextString = format?.ToString();
            formattingInfo.Write(formattable.ToString(fmtTextString, provider).AsSpan());
            return true;
        }

        // Fallback to ToString (string.ToString() returns 'this')
        var result = current != null ? current.ToString().AsSpan() : Span<char>.Empty;
        formattingInfo.Write(result);
        return true;
    }
}
