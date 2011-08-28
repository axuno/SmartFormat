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
    public class PluralLocalizationExtension : IFormatter
    {
        private readonly PluralFormatInfo defaultPluralFormatInfo;
        public PluralLocalizationExtension()
        {
            this.defaultPluralFormatInfo = new CommonLanguagesPluralFormatInfo();
        }
        public PluralLocalizationExtension(PluralFormatInfo defaultPluralFormatInfo)
        {
            this.defaultPluralFormatInfo = defaultPluralFormatInfo;
        }

        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            // See if the format string contains un-nested ":":
            var pluralWords = format.Split(":");
            if (pluralWords.Count == 1) return; // This extension requires a ":" in the format string.

            // See if the value is a number:
            var currentIsNumber =
                current is byte || current is short || current is int || current is long
                || current is float || current is double || current is decimal;
            if (!currentIsNumber) return; // This extension only formats numbers.

            // Normalize the number to decimal:
            var value = Convert.ToDecimal(current);

            // Determine which PluralFormatInfo to use (depending on CurrentCulture, etc):
            var provider = formatDetails.Formatter.Provider;
            var cultureInfo = provider as CultureInfo;
            var pluralFormatInfo = GetPluralFormatInfo(provider);

            if (pluralFormatInfo == null)
            {
                // Not a supported language.
                return;
            }

            var pluralCount = pluralWords.Count;
            var pluralIndex = pluralFormatInfo.GetPluralIndex(cultureInfo, value, pluralCount);

            if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
            {
                // Index is out of range.
                throw new FormatException(format, "Invalid plural index", pluralWords.Last().endIndex);
            }

            // Output the selected word (allowing for nested formats):
            var pluralForm = pluralWords[pluralIndex];
            formatDetails.Formatter.Format(output, pluralForm, current, formatDetails);
            handled = true;
        }

        private PluralFormatInfo GetPluralFormatInfo(IFormatProvider provider)
        {
            // We need to get a PluralFormatInfo to process this request.
            PluralFormatInfo pluralFormatInfo = null;

            if (provider != null)
            {
                // Try to get the PluralFormatInfo from the IFormatProvider:
                // (this might return null)
                pluralFormatInfo = (PluralFormatInfo)provider.GetFormat(typeof(PluralFormatInfo));
            }

            return pluralFormatInfo ?? defaultPluralFormatInfo;
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
        public delegate int GetPluralIndexDelegate(decimal value, int pluralCount);

        private readonly GetPluralIndexDelegate _getPluralIndex;
        public PluralFormatInfo(GetPluralIndexDelegate _getPluralIndex)
        {
            this._getPluralIndex = _getPluralIndex;
        }

        public virtual int GetPluralIndex(CultureInfo cultureInfo, decimal value, int pluralCount)
        {
            var language = (cultureInfo != null) ? cultureInfo.TwoLetterISOLanguageName : null;
            switch (language)
            {
                    
            }
            return _getPluralIndex(value, pluralCount);
        }
    }

    public class CommonLanguagesPluralFormatInfo : PluralFormatInfo
    {
        public CommonLanguagesPluralFormatInfo() : base(null)
        {
        }

        public override int GetPluralIndex(CultureInfo cultureInfo, decimal value, int pluralCount)
        {
            var languageRule = GetLanguageRule(cultureInfo);
            if (languageRule == null)
            {
                return -1;
            }
            return languageRule(value, pluralCount);
        }

        #region: Common Language Rules :

        public static GetPluralIndexDelegate GetLanguageRule(CultureInfo cultureInfo)
        {
            var language = (cultureInfo != null) ? cultureInfo.TwoLetterISOLanguageName : null;

            // The following language codes came from:
            // http://www.loc.gov/standards/iso639-2/php/code_list.php
            switch (language)
            {
                case null:
                    return null;

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

        public static class CommonLanguageRules
        {
            // Much of this language information came from the following url:
            // http://www.gnu.org/s/hello/manual/gettext/Plural-forms.html

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
