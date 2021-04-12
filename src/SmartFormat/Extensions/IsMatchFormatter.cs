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
        public string[] Names { get; set; } = { "ismatch" };

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var expression = formattingInfo.FormatterOptions ?? string.Empty;
            var formats = formattingInfo.Format?.Split('|');

            if (formats is null) return false;

            if (formats.Count == 0)
                return true;

            if (formats.Count != 2)
                throw new FormatException("Exactly 2 format options are required.");

            var regEx = new Regex(expression, RegexOptions);

            if (formattingInfo.CurrentValue != null && regEx.IsMatch(formattingInfo.CurrentValue.ToString()!))
                formattingInfo.Write(formats[0], formattingInfo.CurrentValue);
            else if (formats.Count == 2)
                formattingInfo.Write(formats[1], formattingInfo.CurrentValue ?? string.Empty);

            return true;
        }

        public RegexOptions RegexOptions { get; set; }
    }
}