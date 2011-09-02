using System;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using FormatException = SmartFormat.Core.FormatException;

namespace SmartFormat.Extensions
{
    [ExtensionPriority(ExtensionPriority.High)]
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
            // Let's retirm the correct Plural Rule to use:
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
                return CommonLanguageRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
            }
            return CommonLanguageRules.GetPluralRule(defaultTwoLetterISOLanguageName);
        }

        public static class CommonLanguageRules
        {
            // Much of this language information came from the following url:
            // http://www.gnu.org/s/hello/manual/gettext/Plural-forms.html
            // The following language codes came from:
            // http://www.loc.gov/standards/iso639-2/php/code_list.php
        
            public static PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
            {
                switch (twoLetterISOLanguageName)
                {
                    // Germanic family
                    //     English, German, Dutch, Swedish, Danish, Norwegian, Faroese
                    // Romanic family
                    //     Spanish, Portuguese, Italian, Bulgarian
                    // Latin/Greek family
                    //     Greek
                    // Finno-Ugric family
                    //     Finnish, Estonian
                    // Semitic family
                    //     Hebrew
                    // Artificial
                    //     Esperanto 
                    // Finno-Ugric family
                    //     Hungarian
                    // Turkic/Altaic family
                    //     Turkish 
                    case "en": case "de": case "nl": case "sv": case "da": case "no": case "nn": case "nb": case "fo": 
                    case "es": case "pt": case "it": case "bg": 
                    case "el": 
                    case "fi": case "et":
                    case "he":
                    case "eo":
                    case "hu":
                    case "tr":
                        return CommonLanguageRules.English_Special;

                    // Romanic family
                    //     Brazilian Portuguese, French 
                    case "fr":
                        return CommonLanguageRules.French;

                    // Baltic family
                    //     Latvian 
                    case "lv":
                        return CommonLanguageRules.Latvian;

                    // Celtic
                    //     Gaeilge (Irish) 
                    case "ga":
                        return CommonLanguageRules.Irish;

                    // Romanic family
                    //     Romanian 
                    case "ro":
                        return CommonLanguageRules.Romanian;

                    //Baltic family
                    //    Lithuanian 
                    case "lt":
                        return CommonLanguageRules.Lithuanian;

                    // Slavic family
                    //     Russian, Ukrainian, Serbian, Croatian 
                    case "ru": case "uk": case "sr": case "hr":
                        return CommonLanguageRules.Russian;

                    // Slavic family
                    //     Czech, Slovak 
                    case "cs": case "sk":
                        return CommonLanguageRules.Czech;

                    // Slavic family
                    //     Polish 
                    case "pl":
                        return CommonLanguageRules.Polish;

                    // Slavic family
                    //     Slovenian 
                    case "sl":
                        return CommonLanguageRules.Slovenian;


                    default:
                        return null;
                }
            }

            public static int English_Special(decimal value, int pluralCount)
            {
                // Two forms, singular used for one only
                if (pluralCount == 2)
                {
                    return (value == 1m ? 0 : 1);
                }
                // Three forms (zero, one, plural)
                if (pluralCount == 3)
                {
                    return (value == 0m) ? 0 : (value == 1m) ? 1 : 2;
                }
                // Four forms (negative, zero, one, plural)
                if (pluralCount == 4)
                {
                    return (value < 0m) ? 0 : (value == 0m) ? 1 : (value == 1m) ? 2 : 3;
                }

                return -1; // Too many parameters!
            }
            public static int English(decimal value, int pluralCount)
            {
                // Two forms, singular used for one only
                if (pluralCount == 2)
                {
                    return (value == 1m ? 0 : 1);
                }

                return -1; // Too many parameters!
            }
            public static int French(decimal value, int pluralCount)
            {
                // Two forms, singular used for zero and one
                if (pluralCount == 2)
                {
                    return (value == 0m || value == 1m) ? 0 : 1;
                }
                return -1;
            }
            public static int Latvian(decimal value, int pluralCount)
            {
                // Three forms, special case for zero
                if (pluralCount == 3)
                {
                    return (value % 10 == 1 && value % 100 != 11) ? 0 : (value != 0) ? 1 : 2;
                }
                return -1;
            }
            public static int Irish(decimal value, int pluralCount)
            {
                // Three forms, special cases for one and two
                if (pluralCount == 3)
                {
                    return (value == 1) ? 0 : (value == 2) ? 1 : 2;

                }
                return -1;
            }
            public static int Romanian(decimal value, int pluralCount)
            {
                // Three forms, special case for numbers ending in 00 or [2-9][0-9]
                if (pluralCount == 3)
                {
                    return (value == 1m) ? 0 : (value == 0m || (value % 100 > 0 && value % 100 < 20)) ? 1 : 2;
                }
                return -1;
            }
            public static int Lithuanian(decimal value, int pluralCount)
            {
                // Three forms, special case for numbers ending in 1[2-9]
                if (pluralCount == 3)
                {
                    return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
                }
                return -1;
            }
            public static int Russian(decimal value, int pluralCount)
            {
                // Three forms, special cases for numbers ending in 1 and 2, 3, 4, except those ending in 1[1-4]
                if (pluralCount == 3)
                {
                    return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
                }
                return -1;
            }
            public static int Czech(decimal value, int pluralCount)
            {
                // Three forms, special cases for 1 and 2, 3, 4
                if (pluralCount == 3)
                {
                    return (value == 1) ? 0 : (value >= 2 && value <= 4) ? 1 : 2;
                }
                return -1;
            }
            public static int Polish(decimal value, int pluralCount)
            {
                // Three forms, special case for one and some numbers ending in 2, 3, or 4
                if (pluralCount == 3)
                {
                    return (value == 1) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
                }
                return -1;
            }
            public static int Slovenian(decimal value, int pluralCount)
            {
                // Four forms, special case for one and all numbers ending in 02, 03, or 04
                if (pluralCount == 4)
                {
                    return (value % 100 == 1) ? 0 : (value % 100 == 2) ? 1 : (value % 100 == 3 || value % 100 == 4) ? 2 : 3;
                }
                return -1;
            }
        }

        #endregion

    }



}
