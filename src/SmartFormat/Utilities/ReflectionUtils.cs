//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat.Utilities;
internal static class ReflectionUtils
{
    #region *** Dictionary ***

    public static readonly List<Type> DictionaryInterfaces = new() {
        typeof(IDictionary<,>), // 1
        typeof(IDictionary), // 2
        typeof(IReadOnlyDictionary<,>) // 3
    };

    public static bool IsDictionary(Type type)
    {
        return DictionaryInterfaces
            .Exists(dictInterface =>
                dictInterface == type || // 1
                (type.IsGenericType && dictInterface == type.GetGenericTypeDefinition()) || // 2
                type.GetInterfaces().ToList().Exists(typeInterface => // 3
                    typeInterface == dictInterface ||
                    (typeInterface.IsGenericType && dictInterface == typeInterface.GetGenericTypeDefinition())));
    }

    public static bool TryGetDictionaryValue(Type type, object obj, string theKey, StringComparison comparison, out object? value)
    {
        value = null;
        if (!IsDictionary(type)) return false;

        var keys = (IEnumerable) type.GetProperty(nameof(IDictionary.Keys))!.GetValue(obj);

        foreach (var key in keys)
        {
            if (!key.ToString().Equals(theKey, comparison))
                continue;

            value = type.GetProperty("Item")?.GetValue(obj, new [] { key });
            return true;
        }

        return false;
    }

    #endregion
}
