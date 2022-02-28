// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// A class to format primitive types with condition patterns.
    /// </summary>
    public class ConditionalFormatter : IFormatter
    {
        private static readonly Regex _complexConditionPattern
            = new(@"^  (?:   ([&/]?)   ([<>=!]=?)   ([0-9.-]+)   )+   \?",
                //   Description:      and/or    comparator     value
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"conditional", "cond", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "cond";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used to split the option text literals.
        /// </summary>
        public char SplitChar { get; set; } = '|';
        
        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;
            
            // See if the format string contains un-nested "|":
            var parameters = format is not null ? format.Split(SplitChar) : new List<Format>(0);

            // Check whether arguments can be handled by this formatter
            if (format is null || parameters.Count == 1)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format parameters.");
            }

            // See if the value is a number or an Enum:
            var currentIsNumber =
                current is byte or short or ushort or int or uint or long or ulong or float or double or decimal or Enum;

            var currentNumber = currentIsNumber ? Convert.ToDecimal(current) : 0;

            int paramIndex; // Determines which parameter to use for output

            // First, we'll see if we are using "complex conditions":
            if (currentIsNumber)
            {
                paramIndex = -1;
                while (paramIndex++ < parameters.Count)
                {
                    if (!TryEvaluateCondition(parameters[paramIndex], currentNumber, out var conditionWasTrue,
                        out var outputItem))
                    {
                        // This parameter doesn't have a
                        // complex condition (making it a "else" condition)

                        // Only do "complex conditions" if the first item IS a "complex condition".
                        if (paramIndex == 0) break;
                        // Otherwise, output the "else" section:
                        conditionWasTrue = true;
                    }

                    // If the conditional statement was true, then we can break.
                    if (conditionWasTrue)
                    {
                        formattingInfo.FormatAsChild(outputItem, current);
                        return true;
                    }
                }

                // We don't have any "complex conditions",
                // so let's do the normal conditional formatting:
            }
            
            var paramCount = parameters.Count;

            // Determine the Current item's Type:
            if (currentIsNumber)
            {
                if (currentNumber < 0)
                    paramIndex = paramCount - 1;
                else
                    paramIndex = Math.Min((int) Math.Floor(currentNumber), paramCount - 1);
            }
            else switch (current)
            {
                case bool boolArg:
                    // Bool: True|False
                    paramIndex = boolArg ? 0 : 1;
                    break;
                // Date: Past|Present|Future   or   Past/Present|Future
                case DateTime dateTimeArg when paramCount == 3 && dateTimeArg.ToUniversalTime().Date == SystemTime.Now().ToUniversalTime().Date:
                    paramIndex = 1;
                    break;
                case DateTime dateTimeArg when dateTimeArg.ToUniversalTime() <= SystemTime.Now().ToUniversalTime():
                    paramIndex = 0;
                    break;
                case DateTime dateTimeArg:
                    paramIndex = paramCount - 1;
                    break;
                // Date: Past|Present|Future   or   Past/Present|Future
                case DateTimeOffset dateTimeOffsetArg when paramCount == 3 && dateTimeOffsetArg.UtcDateTime.Date == SystemTime.OffsetNow().UtcDateTime.Date:
                    paramIndex = 1;
                    break;
                case DateTimeOffset dateTimeOffsetArg when dateTimeOffsetArg.UtcDateTime <= SystemTime.OffsetNow().UtcDateTime:
                    paramIndex = 0;
                    break;
                case DateTimeOffset:
                    paramIndex = paramCount - 1;
                    break;
                // TimeSpan: Negative|Zero|Positive  or  Negative/Zero|Positive
                case TimeSpan timeSpanArg when paramCount == 3 && timeSpanArg == TimeSpan.Zero:
                    paramIndex = 1;
                    break;
                case TimeSpan timeSpanArg when timeSpanArg.CompareTo(TimeSpan.Zero) <= 0:
                    paramIndex = 0;
                    break;
                case TimeSpan:
                    paramIndex = paramCount - 1;
                    break;
                case string stringArg:
                    // String: Value|NullOrEmpty
                    paramIndex = !string.IsNullOrEmpty(stringArg) ? 0 : 1;
                    break;
                default:
                {
                    // Object: Something|Nothing
                    var arg = current;
                    paramIndex = arg != null ? 0 : 1;
                    break;
                }
            }

            // Now, output the selected parameter:
            var selectedParameter = parameters[paramIndex];

            // Output the selectedParameter:
            formattingInfo.FormatAsChild(selectedParameter, current);
            return true;
        }

        /// <summary>
        /// Evaluates a conditional format.
        /// Each condition must start with a comparer: "&gt;/&gt;=", "&lt;/&lt;=", "=", "!=".
        /// Conditions must be separated by either "&amp;" (AND) or "/" (OR).
        /// The conditional statement must end with a "?".
        /// Examples:
        /// &gt;=21&amp;&lt;30&amp;!=25/=40?
        /// </summary>
        private static bool TryEvaluateCondition(Format parameter, decimal value, out bool conditionResult,
            out Format outputItem)
        {
            conditionResult = false;
            // Let's evaluate the conditions into a boolean value:
            var m = _complexConditionPattern.Match(parameter.BaseString, parameter.StartIndex,
                parameter.EndIndex - parameter.StartIndex);
            if (!m.Success)
            {
                // Could not parse the "complex condition"
                outputItem = parameter;
                return false;
            }


            var andOrs = m.Groups[1].Captures;
            var comps = m.Groups[2].Captures;
            var values = m.Groups[3].Captures;

            for (var i = 0; i < andOrs.Count; i++)
            {
                var v = decimal.Parse(values[i].Value);
                var exp = false;
                switch (comps[i].Value)
                {
                    case ">":
                        exp = value > v;
                        break;
                    case "<":
                        exp = value < v;
                        break;
                    case "=":
                    case "==":
                        exp = value == v;
                        break;
                    case "<=":
                        exp = value <= v;
                        break;
                    case ">=":
                        exp = value >= v;
                        break;
                    case "!":
                    case "!=":
                        exp = value != v;
                        break;
                }

                if (i == 0)
                    conditionResult = exp;
                else if (andOrs[i].Value == "/")
                    conditionResult |= exp;
                else
                    conditionResult &= exp;
            }

            // Successful
            // Output the substring that doesn't contain the "complex condition"
            var newStartIndex = m.Index + m.Length - parameter.StartIndex;
            outputItem = parameter.Substring(newStartIndex);
            return true;
        }
    }
}
