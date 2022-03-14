// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate sources of types <see cref="IDictionary"/>,
    /// generic <see cref="IDictionary{TKey,TValue}"/> and dynamic <see cref="System.Dynamic.ExpandoObject"/>.
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

                    if (key.Equals(selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()))
                    {
                        selectorInfo.Result = entry.Value;
                        return true;
                    }
                }

            // this check is for dynamics and generic dictionaries
            if (current is not IDictionary<string, object?> dict) return false;

            // We're using the CaseSensitivityType of the dictionary,
            // not the one from Settings.GetCaseSensitivityComparison().
            // This is faster and has less GC than Key.Equals(...)
            if (!dict.TryGetValue(selector, out var val)) return false;

            selectorInfo.Result = val;
            return true;
        }
    }
}
