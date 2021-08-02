//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Formatter with evaluation of regular expressions.
    /// </summary>
    /// <remarks>
    /// Syntax:
    /// {value:ismatch(regex): format | default}
    /// Or in context of a list:
    /// {myList:list:{:ismatch(^regex$):{:format}|'no match'}|, | and }
    /// </remarks>
    public class IsMatchFormatter : IFormatter
    {
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"ismatch"};

        ///<inheritdoc/>
        public string Name { get; set; } = "ismatch";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = false;

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var expression = formattingInfo.FormatterOptions ?? string.Empty;
            var formats = formattingInfo.Format?.Split('|');

            // Check whether arguments can be handled by this formatter
            if (formats is null || formats.Count != 2)
            {
                if (formats?.Count == 0)
                    return true;

                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format options.");
            }

            var regEx = new Regex(expression, RegexOptions);

            if (formattingInfo.CurrentValue != null && regEx.IsMatch(formattingInfo.CurrentValue.ToString()!))
                formattingInfo.FormatAsChild(formats[0], formattingInfo.CurrentValue);
            else if (formats.Count == 2)
                formattingInfo.FormatAsChild(formats[1], formattingInfo.CurrentValue);

            return true;
        }

        /// <summary>
        /// Gets or sets the <see cref="RegexOptions"/> for the <see cref="Regex"/> expression.
        /// </summary>
        public RegexOptions RegexOptions { get; set; }
    }
}