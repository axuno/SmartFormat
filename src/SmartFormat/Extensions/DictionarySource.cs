// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions;

/// <summary>
/// Class to evaluate sources of types <see cref="IDictionary"/>,
/// generic <see cref="IDictionary{TKey,TValue}"/>, dynamic <see cref="System.Dynamic.ExpandoObject"/>,
/// and <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
/// Include this source, if any of these types shall be used.
/// <para/>
/// For support of <see cref="IReadOnlyDictionary{TKey,TValue}"/> <see cref="IsIReadOnlyDictionaryEnabled"/> must be set to <see langword="true"/>.
/// This uses Reflection and is slower than the other types despite caching.
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

        // See if current is an IDictionary (including generic dictionaries) and contains the selector:
        if (current is IDictionary rawDict)
            foreach (DictionaryEntry entry in rawDict)
            {
                var key = entry.Key as string ?? entry.Key.ToString()!;

                if (!key.Equals(selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()))
                    continue;

                selectorInfo.Result = entry.Value;
                return true;
            }

        // This check is for dynamics (ExpandoObject):
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
        if (IsIReadOnlyDictionaryEnabled && TryGetDictionaryValue(current, selector,
                selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison(), out var value))
        {
            selectorInfo.Result = value;
            return true;
        }
        
        return false;
    }

    #region *** IReadOnlyDictionary<,> ***

    /// <summary>
    /// Gets the type cache <see cref="IDictionary{TKey,TValue}"/> for <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
    /// It could e.g. be pre-filled or cleared in a derived class.
    /// </summary>
    /// <remarks>
    /// Note: For reading, <see cref="Dictionary{TKey, TValue}"/> and <see cref="ConcurrentDictionary{TKey,TValue}"/> perform equally.
    /// For writing, <see cref="ConcurrentDictionary{TKey, TValue}"/> is slower with more garbage (tested under net5.0).
    /// </remarks>
    protected internal readonly IDictionary<Type, (PropertyInfo, PropertyInfo)?> RoDictionaryTypeCache =
        SmartSettings.IsThreadSafeMode
            ? new ConcurrentDictionary<Type, (PropertyInfo, PropertyInfo)?>()
            : new Dictionary<Type, (PropertyInfo, PropertyInfo)?>();

    /// <summary>
    /// Gets or sets, whether the <see cref="IReadOnlyDictionary{TKey,TValue}"/> interface should be supported.
    /// Although caching is used, this is still slower than the other types.
    /// Default is <see langword="false"/>.
    /// </summary>
    public bool IsIReadOnlyDictionaryEnabled { get; set; } = false;
    
    private bool TryGetDictionaryValue(object obj, string key, StringComparison comparison, out object? value)
    {
        value = null;

        if (!TryGetDictionaryProperties(obj.GetType(), out var propertyTuple)) return false;

        var keys = (IEnumerable) propertyTuple!.Value.KeyProperty.GetValue(obj);

        foreach (var k in keys)
        {
            if (!k.ToString().Equals(key, comparison))
                continue;

            value = propertyTuple.Value.ItemProperty.GetValue(obj, new [] { k });
            return true;
        }

        return false;
    }

    private bool TryGetDictionaryProperties(Type type, out (PropertyInfo KeyProperty, PropertyInfo ItemProperty)? propertyTuple)
    {
        // try to get the properties from the cache
        if (RoDictionaryTypeCache.TryGetValue(type, out propertyTuple))
            return propertyTuple != null;

        if (!IsIReadOnlyDictionary(type))
        {
            // don't check the type again, although it's not a IReadOnlyDictionary
            RoDictionaryTypeCache[type] = null;
            return false;
        }

        // get Key and Item properties of the dictionary
        propertyTuple = (type.GetProperty(nameof(IDictionary.Keys)), type.GetProperty("Item"));

        System.Diagnostics.Debug.Assert(propertyTuple.Value.KeyProperty != null && propertyTuple.Value.ItemProperty != null, "Key and Item properties must not be null");

        RoDictionaryTypeCache[type] = propertyTuple;
        return true;
    }

    private static bool IsIReadOnlyDictionary(Type type)
    {
        // No Linq for less garbage
        foreach (var typeInterface in type.GetInterfaces())
        {
            if (typeInterface == typeof(IReadOnlyDictionary<,>) ||
                (typeInterface.IsGenericType
                 && typeInterface.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
                return true;
        }

        return false;
    }

    #endregion
}
