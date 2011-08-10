using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;
using FormatException = SmartFormat.Core.FormatException;


namespace SmartFormat.Plugins
{
    [PluginPriority(PluginPriority.High)]
    public class LocalizationForPluralsPlugin : IFormatter
    {
        #region: Delegate for Custom Rules:

        /// <summary>
        /// This delegate determines which singular or plural word 
        /// should be chosen for the given quantity.
        /// 
        /// This allows each language to define its own behavior 
        /// for singular or plural words.
        /// 
        /// It should return the index of the correct parameter.
        /// </summary>
        /// <param name="quantity">The value that is being referenced by the singular or plural words</param>
        /// <returns></returns>
        public delegate int GetParameterIndexDelegate(decimal quantity);
        
        #endregion

        #region: Custom Culture Rules :

        private static Dictionary<int, GetParameterIndexDelegate> customRules;
        public const int DEFAULT_LCID = 0;

        static LocalizationForPluralsPlugin()
        {
            customRules = new Dictionary<int, GetParameterIndexDelegate>();

            // Create our own built-in rules:
            GetParameterIndexDelegate OneSingleElsePluralRule = quantity => (quantity == 1m) ? 0 : 1;
            // Add the default:
            AddCustomCultureRule(DEFAULT_LCID, OneSingleElsePluralRule);

            // en-US Rule:
            AddCustomCultureRule(1033, OneSingleElsePluralRule);
        }

        public static void AddCustomCultureRule(int LCID, GetParameterIndexDelegate customRule)
        {
            if (customRule == null) throw new ArgumentNullException("customRule");

            // Store the custom rule:
            customRules[LCID] = customRule;
        }

        protected static int GetParameterIndex(int LCID, decimal quantity)
        {
            // See if there is a custom rule for this LCID:
            GetParameterIndexDelegate customRule;
            if (customRules.TryGetValue(LCID, out customRule))
            {
                return customRule(quantity);
            }
            // Let's fallback to the default rule:
            return customRules[DEFAULT_LCID](quantity);
        }

        #endregion
        
        #region: IFormatter Implementation :

        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            if (format == null) return;

            // See if the format string contains un-nested ":":
            var parameters = format.Split(":");
            if (parameters.Count == 1) return; // There are no parameters found for this extension to use.

            // See if the value is a number:
            var currentIsNumber =
                current is byte || current is short || current is int || current is long
                || current is float || current is double || current is decimal;
            if (!currentIsNumber) return; // This plugin only formats numbers.

            // Normalize the number to decimal:
            var currentNumber = Convert.ToDecimal(current);

            // Determine the correct plural form, 
            // either based on the locale (if set),
            // or the default locale.
            var currentCulture = formatDetails.Formatter.Provider as CultureInfo;
            var LCID = (currentCulture != null) ? currentCulture.LCID : DEFAULT_LCID; // Default rule is stored as LCID=0.
            var selectedParameterIndex = GetParameterIndex(LCID, currentNumber);

            // Validate the selectedParameterIndex:
            if (selectedParameterIndex < 0 || parameters.Count <= selectedParameterIndex)
            {
                // Parameter index is out-of-bounds.
                throw new FormatException(format, "LocalizationForPluralsPlugin cannot determine the correct plural form for this item", parameters.Last().endIndex);
            }

            // Now, output the selected parameter:
            var selectedParameter = parameters[selectedParameterIndex];

            // Output the selectedParameter (allowing for nested parameters):
            formatDetails.Formatter.Format(output, selectedParameter, current, formatDetails);
            handled = true;
        }
        
        #endregion
        




    }
}