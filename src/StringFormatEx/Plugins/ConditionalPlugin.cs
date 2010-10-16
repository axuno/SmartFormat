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
            var parameters = new List<int>();
            parameters.Add(format.startIndex - 1);
            parameters.AddRange(format.FindAll("|"));
            if (parameters.Count == 1) return; // There are no parameters found..


            int paramCount = parameters.Count;
            int paramIndex; // Determines which parameter to use for output
            Format selectedParameter;

            var currentIsNumber = 
                current is byte || current is short || current is int || current is long
                || current is float || current is double || current is decimal;
            var currentNumber = currentIsNumber ? Convert.ToDecimal(current) : 0;


            // First, we'll see if we are using "complex conditions":
            if (currentIsNumber) {

                // Only do "complex conditions" if the first item is a complex condition.
                var hasComplexConditions = false;
                bool conditionWasTrue;
                int start = parameters[0] + 1;
                int end = parameters[1];
                paramIndex = 0;
                while (TryEvaluateCondition(format, ref start, end, currentNumber, out conditionWasTrue))
                {
                    hasComplexConditions = true;
                    // If the conditional statement was true, then we can break.
                    if (conditionWasTrue) {
                        break;
                    } else if (paramIndex == parameters.Count) {
                        // We've run out of conditional statements.
                        break;
                    }
                    paramIndex += 1;
                    start = parameters[paramIndex] + 1;
                    end = (paramIndex < parameters.Count - 1) ? parameters[paramIndex + 1] : format.endIndex;
                }

                if (hasComplexConditions) {
                    // Let's output the result of the complex condition
                    handled = true;
                    if (paramIndex >= parameters.Count) return; // Nothing to output
                    selectedParameter = format.Substring(start, end);
                    formatter.Format(output, selectedParameter, args, current);
                }
            }


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
                if (arg)
                {
                    paramIndex = 0;
                }
                else
                {
                    paramIndex = 1;
                }
            } 
            else if (current is DateTime) {
                // Date: Past|Present|Future   or   Past/Present|Future
                System.DateTime arg = (DateTime)current;
                if (paramCount == 3 && arg.Date == DateTime.Today) 
                {
                    paramIndex = 1;
                }
                else if (arg < DateTime.Now)
                {
                    paramIndex = 0;
                }
                else
                {
                    paramIndex = paramCount - 1;
                }
            } 
            else if (current is TimeSpan) {
                // TimeSpan: Negative|Zero|Positive  or  Negative/Zero|Positive
                TimeSpan arg = (TimeSpan)current;
                if (paramCount == 3 && arg == TimeSpan.Zero)
                {
                    paramIndex = 1;
                } 
                else if (arg.CompareTo(TimeSpan.Zero) == -1) 
                {
                    paramIndex = 0;
                } 
                else 
                {
                    paramIndex = paramCount - 1;
                }
            } 
            else if (current is string) {
                // String: Value|NullOrEmpty
                var arg = (string)current;
                if (!string.IsNullOrEmpty(arg))
                {
                    paramIndex = 0;
                }
                else
                {
                    paramIndex = 1;
                }
            } else {
                // Object: Something|Nothing
                object arg = current;
                if (arg != null)
                {
                    paramIndex = 0;
                } 
                else 
                {
                    paramIndex = 1;
                }
            }

            // Now, output the selected parameter:
            int startIndex = (paramIndex < parameters.Count) ? parameters[paramIndex] + 1 : format.endIndex;
            int endIndex = (paramIndex < parameters.Count - 1) ? parameters[paramIndex + 1] : format.endIndex;
            selectedParameter = format.Substring(startIndex, endIndex);
            
            // Output the selectedParameter:
            formatter.Format(output, selectedParameter, args, current);
            handled = true;
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
        private static bool TryEvaluateCondition(Format format, ref int start, int end, decimal value, out bool conditionResult)
        {
        //private static bool TryEvaluateCondition(ref Format conditions, decimal value, ref bool conditionResult)
        //{
            conditionResult = false;
            //                                           and/or   comparator     value
            // Let's evaluate the conditions into a boolean value:
            Match m = static_TryEvaluateCondition_conditionFormat.Match(format.baseString, start, end - start);
            if (!m.Success) {
                // Could not parse the "complex condition"
                return false; 
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

            // Successful
            start = m.Index + m.Length;
            return true;
        }

    }
}