//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public DictionarySource(SmartFormatter formatter) : base(formatter)
        {
        }

        /// <inheritdoc />>
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var current = selectorInfo.CurrentValue;
            if (current is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }
            
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
            if (current is IDictionary<string, object> dict)
            {
                var val = dict.FirstOrDefault(x =>
                    x.Key.Equals(selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison())).Value;
                if (val != null)
                {
                    selectorInfo.Result = val;
                    return true;
                }
            }

            return false;
        }
    }
}