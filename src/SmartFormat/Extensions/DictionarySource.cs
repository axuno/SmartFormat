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
    public class DictionarySource : ISource
    {
        public DictionarySource(SmartFormatter formatter)
        {
            // Add some special info to the parser:
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            formatter.Parser.AddOperators(".");
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var current = selectorInfo.CurrentValue;
            var selector = selectorInfo.SelectorText;

            // See if current is a IDictionary and contains the selector:
            var rawDict = current as IDictionary;
            if (rawDict != null)
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