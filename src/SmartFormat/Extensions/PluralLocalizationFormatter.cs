using System;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;
using FormatException = SmartFormat.Core.FormatException;

namespace SmartFormat.Extensions
{
    public class PluralLocalizationFormatter : IFormatter
    {
        private readonly PluralFormatInfo defaultPluralFormatInfo;
        /// <summary>
        /// Initializes the plugin with rules for many common languages.
        /// If no CultureInfo is supplied to the formatter, the
        /// default language rules will be used by default.
        /// </summary>
        public PluralLocalizationFormatter(string defaultTwoLetterISOLanguageName)
        {
            this.defaultPluralFormatInfo = new CommonLanguagesPluralFormatInfo(defaultTwoLetterISOLanguageName);
        }
        /// <summary>
        /// Initializes the plugin with a custom default rule provider.
        /// </summary>
        /// <param name="defaultPluralFormatInfo"></param>
        public PluralLocalizationFormatter(PluralFormatInfo defaultPluralFormatInfo)
        {
            this.defaultPluralFormatInfo = defaultPluralFormatInfo;
        }

        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            // Ignore formats that start with "?" (this can be used to bypass this extension)
            if (format == null || format.baseString[format.startIndex] == ':')
            {
                return;
            }

            // Extract the plural words from the format string:
            var pluralWords = format.Split("|");
            // This extension requires at least two plural words:
            if (pluralWords.Count == 1) return; 

            // See if the value is a number:
            var currentIsNumber =
                current is byte || current is short || current is int || current is long
                || current is float || current is double || current is decimal;
            // This extension only formats numbers:
            if (!currentIsNumber) return; 

            // Normalize the number to decimal:
            var value = Convert.ToDecimal(current);

            // Get the plural rule:
            var provider = formatDetails.Provider;
            var pluralRule = GetPluralRule(provider);

            if (pluralRule == null)
            {
                // Not a supported language.
                return;
            }

            var pluralCount = pluralWords.Count;
            var pluralIndex = pluralRule(value, pluralCount);

            if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
            {
                // The plural rule should always return a value in-range!
                throw new FormatException(format, "Invalid number of plural parameters", pluralWords.Last().endIndex);
            }

            // Output the selected word (allowing for nested formats):
            var pluralForm = pluralWords[pluralIndex];
            formatDetails.Formatter.Format(output, pluralForm, current, formatDetails);
            handled = true;
        }
                
        private PluralFormatInfo.PluralRuleDelegate GetPluralRule(IFormatProvider provider)
        {
            // Let's determine the correct Plural Rule to use:
            PluralFormatInfo pluralFormatInfo = null;

            // See if a custom PluralFormatInfo is available from the FormatProvider:
            if (provider != null)
            {
                pluralFormatInfo = (PluralFormatInfo)provider.GetFormat(typeof (PluralFormatInfo));
            }

            // If no custom PluralFormatInfo, let's use our default:
            if (pluralFormatInfo == null)
            {
                pluralFormatInfo = defaultPluralFormatInfo;
            }

            // Only continue if we have a PluralFormatInfo:
            if (pluralFormatInfo == null)
            {
                return null;
            }


            // Get the culture, only if it was provided:
            var cultureInfo = provider as CultureInfo;

            // Retrieve the plural rule from the PluralFormatInfo:
            return pluralFormatInfo.GetPluralRule(cultureInfo);
        }

    }

    public class PluralFormatInfo : IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(PluralFormatInfo)) ? this : null;
        }

        /// <summary>
        /// This delegate determines which singular or plural word 
        /// should be chosen for the given quantity.
        /// 
        /// This allows each language to define its own behavior 
        /// for singular or plural words.
        /// 
        /// It should return the index of the correct parameter.
        /// </summary>
        /// <param name="value">The value that is being referenced by the singular or plural words</param>
        /// <param name="pluralCount"></param>
        /// <returns></returns>
        public delegate int PluralRuleDelegate(decimal value, int pluralCount);

        private readonly PluralRuleDelegate pluralRule;
        public PluralFormatInfo(PluralRuleDelegate pluralRule)
        {
            this.pluralRule = pluralRule;
        }

        public virtual PluralRuleDelegate GetPluralRule(CultureInfo culture)
        {
            return pluralRule;
        }
    }

    public class CommonLanguagesPluralFormatInfo : PluralFormatInfo
    {
        private string defaultTwoLetterISOLanguageName;
        public CommonLanguagesPluralFormatInfo(string defaultTwoLetterIsoLanguageName) : base(null)
        {
            this.defaultTwoLetterISOLanguageName = defaultTwoLetterIsoLanguageName;
        }

        #region: Common Language Rules :

        public override PluralRuleDelegate GetPluralRule(CultureInfo cultureInfo)
        {
            if (cultureInfo != null)
            {
                return PluralRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
            }
            return PluralRules.GetPluralRule(defaultTwoLetterISOLanguageName);
        }

        #endregion

    }
}
