using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartFormat.Core;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;


namespace SmartFormat.Plugins
{
    public class ConditionalPlugin : IFormatterPlugin
    {

        private static Regex static_TryEvaluateCondition_conditionFormat 
            = new Regex("^(?:   ([&/]?)   ([<>=!]=?)   ([0-9.-]+)   )+   \\?", 
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        public void EvaluateFormat(SmartFormatter formatter, object[] args, object current, Format format, ref bool handled, IOutput output)
        {
            if (format == null) return;
//        }
//        private static void FormatConditional(object sender, ExtendFormatEventArgs e)
//        {
//            CustomFormatInfo info = e.FormatInfo;

            // See if the format string contains un-nested "|":
            Format[] parameters = format.Split("|", 4);
            if (parameters.Length == 1) {
                return; // There are no splits.
            }


            int paramCount = parameters.Length;
            int paramIndex = 0; // Determines which parameter to use in the result

            var currentIsNumber = 
                current is byte || current is short || current is int || current is long
                || current is float || current is double || current is decimal;
            var currentNumber = currentIsNumber ? (decimal)current : 0;

            // See if there are any (optional) conditions:
            bool conditionResult = false;
            if (currentIsNumber && TryEvaluateCondition(ref parameters[0], currentNumber, ref conditionResult)) {
                // parameters(0) contained a "conditional statement"
                // If the conditional statement was False, then
                // we will move on to the next parameters
                while (!conditionResult) {
                    if (paramIndex == parameters.Length - 1) {
                        break;
                    }

                    paramIndex += 1;

                    if (!TryEvaluateCondition(ref parameters[paramIndex], currentNumber, ref conditionResult)) {
                        // (couldn't evaluate the conditional statement, which means it's an "else" statement
                        break;
                    }
                }
            } 
            else {
                // Determine the Current item's Type:
                if (currentIsNumber) {
                    // Number: Neg|Zero|One|Many  or  Zero|One|Many/Neg  or  One|Many/Neg/Zero
                    var arg = currentNumber;
                    if (arg < 0m) {
                        paramIndex = 0;
                    } 
                    else if (arg == 0m) {
                        paramIndex = 1;
                    } 
                    else if (arg <= 1m) {
                        paramIndex = 2;
                    } 
                    else {
                        paramIndex = 3;
                    }
                    
                    paramIndex = paramCount - paramIndex;
                    if (paramIndex < 0) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (current is bool) {
                    // Bool: True|False
                    bool arg = (bool)current;
                    if (!arg) {
                        paramIndex = 1;
                    }
                } 
                else if (current is DateTime) {
                    // Date: Past|Present|Future   or   Past/Present|Future
                    System.DateTime arg = (DateTime)current;
                    if (paramCount == 3 && arg.Date == DateTime.Today) {
                        paramIndex = 1;
                    } 
                    else if (arg > DateTime.Now) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (current is TimeSpan) {
                    // TimeSpan: Negative|Zero|Positive  or  Negative/Zero|Positive
                    TimeSpan arg = (TimeSpan)current;
                    if (paramCount == 3 && arg == TimeSpan.Zero) {
                        paramIndex = 1;
                    } 
                    else if (arg.CompareTo(TimeSpan.Zero) == 1) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (current is string) {
                    // String: Value|NullOrEmpty
                    var arg = (string)current;
                    if (string.IsNullOrEmpty(arg)) {
                        paramIndex = 1;
                    }
                } else {
                    // Object: Something|Nothing
                    object arg = current;
                    if (arg == null) {
                        paramIndex = 1;
                    }
                }
            }


            // Now, output the selected parameter:
            var selectedParameter = parameters[paramIndex];
            if (format.HasNested)
            {
                // The format has nested items, so let's evaluate those now:
                formatter.Format(output, selectedParameter, args, current);
            } else {
                // The format doesn't have nested items so let's just write the selected parameter:
                // Since the format doesn't have nested items, it must be a LiteralText:
                var literal = parameters[paramIndex].Items[0] as LiteralText;
                output.Write(literal);
            }
        }


        /// <summary>
        /// Evaluates a conditional format.
        /// 
        /// Each condition must start with a comparor: "&gt;/&gt;=", "&lt;/&lt;=", "=", "!=".
        /// Conditions must be separated by either "&amp;" (AND) or "/" (OR).
        /// The conditional statement must end with a "?".
        /// 
        /// Examples:
        /// &gt;=21&amp;&lt;30&amp;!=25/=40?
        /// </summary>
        private static bool TryEvaluateCondition(ref Format conditions, decimal value, ref bool conditionResult)
        {
            //                                           and/or   comparator     value
            // Let's evaluate the conditions into a boolean value:
            Match m = static_TryEvaluateCondition_conditionFormat.Match(conditions.Text);
            if (!m.Success) {
                return false; // Unsuccessful
            }


            CaptureCollection andOrs = m.Groups[1].Captures;
            CaptureCollection comps = m.Groups[2].Captures;
            CaptureCollection values = m.Groups[3].Captures;

            for (int i = 0; i < andOrs.Count; i++) {
                decimal v = decimal.Parse(values[i].Value);
                bool exp = false;
                switch (comps[i].Value) {
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

                if (i == 0) {
                    conditionResult = exp;
                } 
                else if (andOrs[i].Value == "/") {
                    conditionResult = conditionResult | exp;
                }
                else {
                    conditionResult = conditionResult & exp;
                }
            }

            conditions = conditions.Substring(m.Index + m.Length);
            return true;
            // Successful
        }

    }
}