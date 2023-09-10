// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        // This is for IReadOnlyDictionary<,>
        var currentType = current.GetType();
        if (IsDictionary(currentType))
        {
            if (currentType.GetProperty("Keys")?.GetValue(current) is not IEnumerable keys)
                return false;

            foreach (var key in keys)
            {
                if (!key.ToString()
                        .Equals(selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()))
                    continue;

                selectorInfo.Result = currentType.GetProperty("Item")?.GetValue(current, new [] { key });
                return true;
            }
        }
        
        return false;
    }

    private static readonly List<Type> DictionaryInterfaces = new() {
        typeof(IDictionary<,>), // 1
        typeof(IDictionary), // 2
        typeof(IReadOnlyDictionary<,>) // 3
    };
    private static bool IsDictionary(Type type)
    {
        return DictionaryInterfaces
            .Exists(dictInterface =>
                dictInterface == type || // 1
                (type.IsGenericType && dictInterface == type.GetGenericTypeDefinition()) || // 2
                type.GetInterfaces().ToList().Exists(typeInterface => // 3
                    typeInterface == dictInterface ||
                    (typeInterface.IsGenericType && dictInterface == typeInterface.GetGenericTypeDefinition())));
    }
}
