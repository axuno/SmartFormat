//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    public class DefaultSource : ISource
    {
        private readonly SmartSettings _settings;

        public DefaultSource(SmartFormatter formatter)
        {
            _settings = formatter.Settings;
        }

        /// <summary>
        /// Performs the default index-based selector, same as String.Format.
        /// </summary>
        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var selector = selectorInfo.SelectorText;
            var formatDetails = selectorInfo.FormatDetails;

            if (int.TryParse(selector, out var selectorValue))
            {
                // Argument Index:
                // Just like string.Format, the arg index must be in-range,
                // should be the first item, and shouldn't have any operator:
                if (selectorInfo.SelectorIndex == 0
                    && selectorValue < formatDetails.OriginalArgs.Count
                    && selectorInfo.SelectorOperator == string.Empty)
                {
                    // This selector is an argument index.
                    selectorInfo.Result = formatDetails.OriginalArgs[selectorValue];
                    return true;
                }
            }

            return false;
        }
    }
}