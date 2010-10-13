using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StringFormatEx.Core;
using StringFormatEx.Core.Output;
using StringFormatEx.Core.Parsing;
using StringFormatEx.Core.Plugins;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx.Plugins
{
    public class ConditionalPlugin : IStringFormatterPlugin, IFormatterPlugin
    {
        public void EvaluateFormat(SmartFormat formatter, object[] args, object current, Format format, ref bool handled, IOutput output)
        {

        }

        public IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions()
        {
            return new EventHandler<ExtendSourceEventArgs>[] {};
        }

        public IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions()
        {
            return new EventHandler<ExtendFormatEventArgs>[] 
                { ConditionalPlugin.FormatConditional };
        }



        private static Regex static_TryEvaluateCondition_conditionFormat 
            = new Regex("^(?:   ([&/]?)   ([<>=!]=?)   ([0-9.-]+)   )+   \\?", 
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        [CustomFormatPriority(CustomFormatPriorities.High)]
        private static void FormatConditional(object sender, ExtendFormatEventArgs e)
        {
            CustomFormatInfo info = e.FormatInfo;

            // See if the format string contains un-nested "|":
            string[] parameters = Core.ParsingServices.SplitNested(info.Format, '|');
            if (parameters.Length == 1) {
                return; // There are no splits.
            }


            int paramCount = parameters.Length;
            int paramIndex = 0; // Determines which parameter to use in the result


            // See if there are any (optional) conditions:
            bool conditionResult = false;
            if (info.CurrentIsNumber && TryEvaluateCondition(ref parameters[0], info.Current, ref conditionResult)) {
                // parameters(0) contained a "conditional statement"
                // If the conditional statement was False, then
                // we will move on to the next parameters
                while (!conditionResult) {
                    if (paramIndex == parameters.Length - 1) {
                        break;
                    }

                    paramIndex += 1;

                    if (!TryEvaluateCondition(ref parameters[paramIndex], info.Current, ref conditionResult)) {
                        // (couldn't evaluate the conditional statement, which means it's an "else" statement
                        break;
                    }
                }
            } 
            else {
                // Determine the Current item's Type:
                if (info.CurrentIsNumber) {
                    // Number: Neg|Zero|One|Many  or  Zero|One|Many/Neg  or  One|Many/Neg/Zero
                    var arg = Convert.ToDecimal(info.Current);
                    if (arg < 0m) {
                        paramIndex = -4;
                    } 
                    else if (arg == 0m) {
                        paramIndex = -3;
                    } 
                    else if (0m < arg && arg <= 1m) {
                        paramIndex = -2;
                    } 
                    else {
                        paramIndex = -1;
                    }

                    paramIndex = paramIndex + paramCount;

                    if (paramIndex < 0) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (info.CurrentIsBoolean) {
                    // Bool: True|False
                    bool arg = (bool)info.Current;
                    if (!arg) {
                        paramIndex = 1;
                    }
                } 
                else if (info.CurrentIsDate) {
                    // Date: Past|Present|Future   or   Past/Present|Future
                    System.DateTime arg = (DateTime)info.Current;
                    if (paramCount == 3 && arg.Date == DateTime.Today) {
                        paramIndex = 1;
                    } 
                    else if (arg > DateTime.Now) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (info.CurrentIsTimeSpan) {
                    // TimeSpan: Negative|Zero|Positive  or  Negative/Zero|Positive
                    TimeSpan arg = (TimeSpan)info.Current;
                    if (paramCount == 3 && arg == TimeSpan.Zero) {
                        paramIndex = 1;
                    } 
                    else if (arg.CompareTo(TimeSpan.Zero) == 1) {
                        paramIndex = paramCount - 1;
                    }
                } 
                else if (info.CurrentIsString) {
                    // String: Value|NullOrEmpty
                    var arg = (string)info.Current;
                    if (string.IsNullOrEmpty(arg)) {
                        paramIndex = 1;
                    }
                } else {
                    // Object: Something|Nothing
                    object arg = info.Current;
                    if (arg == null) {
                        paramIndex = 1;
                    }
                }

            }


            // Now, output the selected parameter:
            if (parameters[paramIndex].Contains("{")) {
                // The format has nested items, so let's evaluate those now:
                info.SetFormat(parameters[paramIndex], true);
                info.CustomFormatNested();
            } 
            else {
                // The format doesn't have nested items so let's just write the selected parameter:
                info.Write(parameters[paramIndex]);
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
        private static bool TryEvaluateCondition(ref string conditions, System.Object value, ref bool conditionResult)
        {
            //                                           and/or   comparator     value
            // Let's evaluate the conditions into a boolean value:
            Match m = static_TryEvaluateCondition_conditionFormat.Match(conditions);
            if (!m.Success) {
                return false; // Unsuccessful
            }


            CaptureCollection andOrs = m.Groups[1].Captures;
            CaptureCollection comps = m.Groups[2].Captures;
            CaptureCollection values = m.Groups[3].Captures;

            var decimalValue = Convert.ToDecimal(value);
            for (int i = 0; i < andOrs.Count; i++) {
                decimal v = decimal.Parse(values[i].Value);
                bool exp = false;
                switch (comps[i].Value) {
                    case ">":
                        exp = decimalValue > v;
                        break;
                    case "<":
                        exp = decimalValue < v;
                        break;
                    case "=":
                    case "==":
                        exp = decimalValue == v;
                        break;
                    case "<=":
                        exp = decimalValue <= v;
                        break;
                    case ">=":
                        exp = decimalValue >= v;
                        break;
                    case "!":
                    case "!=":
                        exp = decimalValue != v;
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