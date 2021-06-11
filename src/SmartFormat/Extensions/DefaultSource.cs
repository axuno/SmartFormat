﻿//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate an index-based <see cref="Selector"/>.
    /// </summary>
    public class DefaultSource : Source
    {
        private readonly SmartSettings _settings;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public DefaultSource(SmartFormatter formatter) : base(formatter)
        {
            _settings = formatter.Settings;
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var selector = selectorInfo.SelectorText;
            var formatDetails = selectorInfo.FormatDetails;

            if (int.TryParse(selector, out var selectorValue))
            {
                // Argument Index:
                // Just like string.Format, the arg index must be in-range,
                // must be the first item, and shouldn't have any operator
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