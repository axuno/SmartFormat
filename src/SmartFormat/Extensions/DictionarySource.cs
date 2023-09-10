// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Utilities;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions;

/// <summary>
/// Class to evaluate sources of types <see cref="IDictionary"/>,
/// generic <see cref="IDictionary{TKey,TValue}"/>, dynamic <see cref="System.Dynamic.ExpandoObject"/>,
/// and <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
/// Include this source, if any of these types shall be used.
/// </summary>
public class DictionarySource : Source
{
    /// <inheritdoc />>
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        var current = selectorInfo.CurrentValue;
        if (TrySetResultForNullableOperator(selectorInfo)) return true;
            
        if (current is null) return false;

        var selector = selectorInfo.SelectorText;

        // See if current is an IDictionary and contains the selector:
        if (current is IDictionary rawDict)
            foreach (DictionaryEntry entry in rawDict)
            {
                var key = entry.Key as string ?? entry.Key.ToString()!;

                if (!key.Equals(selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()))
                    continue;

                selectorInfo.Result = entry.Value;
                return true;
            }

        // This check is for dynamics and generic dictionaries
        if (current is IDictionary<string, object?> dict)
        {
            // We're using the CaseSensitivityType of the dictionary,
            // not the one from Settings.GetCaseSensitivityComparison().
            // This is faster and has less GC than Key.Equals(...)
            if (!dict.TryGetValue(selector, out var val)) return false;

            selectorInfo.Result = val;
            return true;
        }

        // This is for IReadOnlyDictionary<,> using Reflection
        if (ReflectionUtils.TryGetDictionaryValue(current.GetType(), current, selector,
                selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison(), out var value))
        {
            selectorInfo.Result = value;
            return true;
        }
        
        return false;
    }
}
