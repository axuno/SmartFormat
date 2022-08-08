//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Utilities;

internal static class FormattingInfoExtensions
{
    public static void FormatPlaceholderAsChild(this FormattingInfo formattingInfo, Placeholder placeholder, object? value)
    {
        using var fmtObject = FormatPool.Instance.Get(out var format);

        format.Initialize(formattingInfo.FormatDetails.Settings, placeholder.BaseString);
        format.Items.Add(placeholder);

        formattingInfo.FormatAsChild(format, value);
    }

    public static object? GetValue(this FormattingInfo formattingInfo, Placeholder placeholder)
    {
        using var fiObject = FormattingInfoPool.Instance.Get(out var fi);
        using var fmtObject = FormatPool.Instance.Get(out var format);
        using var fdObject = FormatDetailsPool.Instance.Get(out var fd);

        if (placeholder.Selectors.Count == 0) return formattingInfo.CurrentValue;

        format.Initialize(formattingInfo.FormatDetails.Settings, placeholder.BaseString);
        format.Items.Add(placeholder);

        fd.Initialize(formattingInfo.FormatDetails.Formatter, formattingInfo.Format!,
            formattingInfo.FormatDetails.OriginalArgs, CultureInfo.InvariantCulture, new NullOutput());

        fi.Initialize(formattingInfo, fd, placeholder, formattingInfo.FormatDetails.OriginalArgs);
        fi.FormatDetails.Formatter.EvaluateSelectors(fi);

        return fi.CurrentValue;
    }

    public static ReadOnlySpan<char> FormatPlaceholderToSpan(this FormattingInfo formattingInfo, Placeholder placeholder,
        IFormatProvider? provider, object? value)
    {
        using var fmtObject = FormatPool.Instance.Get(out var format);

        format.Initialize(formattingInfo.FormatDetails.Settings, placeholder.BaseString);
        format.Items.Add(placeholder);

        return FormatToSpan(formattingInfo, format, provider, value);
    }

    /// <summary>
    /// Creates a new child <see cref="IFormattingInfo"/> from the current <see cref="Format"/> instance
    /// and invokes formatting with <see cref="SmartFormatter"/> and with the child as parameter.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <param name="format">The <see cref="Format"/> to use.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/>, e.g. <see cref="CultureInfo"/>.</param>
    /// <param name="value">The value for the item in the format.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> filled with the result.</returns>
    public static ReadOnlySpan<char> FormatToSpan(this FormattingInfo formattingInfo, Format format,
        IFormatProvider? provider, object? value)
    {
        using var zsOutput = new ZStringOutput(ZStringBuilderExtensions.CalcCapacity(format));
        using var fdObject = FormatDetailsPool.Instance.Get(out var formatDetails);
        // We don't add the nestedFormattingInfo as child to the parent, so we have to dispose it
        using var fiObject = FormattingInfoPool.Instance.Get(out var nestedFormattingInfo);

        formatDetails.Initialize(formattingInfo.FormatDetails.Formatter, format,
            formattingInfo.FormatDetails.OriginalArgs,
            provider, zsOutput);
        nestedFormattingInfo.Initialize(null, formatDetails, format, value);

        // recursive method call
        formatDetails.Formatter.Format(nestedFormattingInfo);

        return zsOutput.Output.AsSpan();
    }

    public static void GetAllValues(this FormattingInfo rootFormattingInfo, Format? format,
        Dictionary<string, (Exception?, object?)> result)
    {
        if (rootFormattingInfo.Format is null || format is null) return;

        foreach (var item in rootFormattingInfo.Format.Items)
        {
            if (item is LiteralText) continue;
            
            // Otherwise, the item must be a placeholder.
            var placeholder = (Placeholder) item;

            var childFormattingInfo = rootFormattingInfo.CreateChild(placeholder);

            try
            {
                // Note: If there is no selector (like {:0.00}),
                // FormattingInfo.CurrentValue is left unchanged
                ValueTuple<Exception?, object?> evalResult;
                try
                {
                    rootFormattingInfo.FormatDetails.Formatter.EvaluateSelectors(childFormattingInfo);
                    evalResult = (null, childFormattingInfo.Result);

                    using var fdObject = FormatDetailsPool.Instance.Get(out var formatDetails);
                    // We don't add the nestedFormattingInfo as child to the parent, so we have dispose it
                    using var fiObject = FormattingInfoPool.Instance.Get(out var nestedFormattingInfo);
                    formatDetails.Initialize(rootFormattingInfo.FormatDetails.Formatter, format,
                        rootFormattingInfo.FormatDetails.OriginalArgs,
                        CultureInfo.InvariantCulture, new NullOutput());
                    nestedFormattingInfo.Initialize(null, formatDetails, format, rootFormattingInfo.CurrentValue);

                    rootFormattingInfo.FormatDetails.Formatter.EvaluateFormatters(childFormattingInfo);
                }
                catch (Exception e)
                {
                    evalResult = (e, null);
                }

                var ph = GetSelectorsAsString(childFormattingInfo);
                if (!result.ContainsKey(ph)) result.Add(ph, evalResult);

                // If the format has nested placeholders, we process those, too.
                // E.g.: "{2:list:{:{FirstName}}|, }"
                if (placeholder.Format is { HasNested: true })
                {
                    GetAllValues(childFormattingInfo, placeholder.Format, result);
                }
            }
            catch (Exception ex)
            {
                // An error occurred while evaluation selectors
                var errorIndex = placeholder.Format?.StartIndex ??
                                 placeholder.Selectors[placeholder.Selectors.Count - 1].EndIndex;
                rootFormattingInfo.FormatDetails.Formatter.FormatError(item, ex, errorIndex, childFormattingInfo);
            }
        }
    }

    private static string GetSelectorsAsString(FormattingInfo formattingInfo)
    {
        var parentFormattingInfo = formattingInfo;
        var hasParent = false;
        while (parentFormattingInfo.Parent != null && parentFormattingInfo.Parent.Placeholder != null &&
               parentFormattingInfo.Parent.Placeholder.Selectors.Count != 0)
        {
            parentFormattingInfo = parentFormattingInfo.Parent;
            hasParent = true;
        }

        // Selector coming from list formatter:
        // if value is part of the parent value, we can determine the index here

        var selectors = new List<Selector>();
        if (hasParent)
            selectors.AddRange(
                parentFormattingInfo.Placeholder!.Selectors.Where(s => s.Length > 0 && s.Operator != ","));
        selectors.AddRange(formattingInfo.Placeholder!.Selectors.Where(s => s.Length > 0 && s.Operator != ","));
        return string.Join(formattingInfo.FormatDetails.Settings.Parser.SelectorOperator.ToString(), selectors);

    }
}
