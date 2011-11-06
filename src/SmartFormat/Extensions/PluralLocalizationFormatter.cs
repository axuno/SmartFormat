using System;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
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

        public static class CommonLanguageRules_Old
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
                        return English_Special;

                    // Romanic family
                    //     Brazilian Portuguese, French 
                    case "fr":
                        return French;

                    // Baltic family
                    //     Latvian 
                    case "lv":
                        return Latvian;

                    // Celtic
                    //     Gaeilge (Irish) 
                    case "ga":
                        return Irish;

                    // Romanic family
                    //     Romanian 
                    case "ro":
                        return Romanian;

                    //Baltic family
                    //    Lithuanian 
                    case "lt":
                        return Lithuanian;

                    // Slavic family
                    //     Russian, Ukrainian, Serbian, Croatian 
                    case "ru": case "uk": case "sr": case "hr":
                        return Russian;

                    // Slavic family
                    //     Czech, Slovak 
                    case "cs": case "sk":
                        return Czech;

                    // Slavic family
                    //     Polish 
                    case "pl":
                        return Polish;

                    // Slavic family
                    //     Slovenian 
                    case "sl":
                        return Slovenian;


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

    public static class CommonLanguageRules
    {
        /// <summary>Construct a ruleset for the language code.</summary>
        /// <param name="twoLetterISOLanguageName">The language code in two-letter ISO-639 format.</param>
        /// <remarks>The pluralisation rules are taken from <see cref="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html"/>.</remarks>
        public static PluralFormatInfo.PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
        {
            switch (twoLetterISOLanguageName)
            {
                // Singular
                case "az": // Azerbaijani
                case "bm": // Bambara
                case "bo": // Tibetan
                case "dz": // Dzongkha
                case "fa": // Persian
                case "hu": // Hungarian
                case "id": // Indonesian
                case "ig": // Igbo
                case "ii": // Sichuan Yi
                case "ja": // Japanese
                case "jv": // Javanese
                case "ka": // Georgian
                case "kde": // Makonde
                case "kea": // Kabuverdianu
                case "km": // Khmer
                case "kn": // Kannada
                case "ko": // Korean
                case "ms": // Malay
                case "my": // Burmese
                case "root": // Root (?)
                case "sah": // Sakha
                case "ses": // Koyraboro Senni
                case "sg": // Sango
                case "th": // Thai
                case "to": // Tonga
                case "vi": // Vietnamese
                case "wo": // Wolof
                case "yo": // Yoruba
                case "zh": // Chinese
                    return (n, c) => 0;

                // Dual: one (n == 1), other
                case "af": // Afrikaans
                case "bem": // Bembda
                case "bg": // Bulgarian
                case "bn": // Bengali
                case "brx": // Bodo
                case "ca": // Catalan
                case "cgg": // Chiga
                case "chr": // Cherokee
                case "da": // Danish
                case "de": // German
                case "dv": // Divehi
                case "ee": // Ewe
                case "el": // Greek
                case "en": // English
                case "eo": // Esperanto
                case "es": // Spanish
                case "et": // Estonian
                case "eu": // Basque
                case "fi": // Finnish
                case "fo": // Faroese
                case "fur": // Friulian
                case "fy": // Western Frisian
                case "gl": // Galician
                case "gsw": // Swiss German
                case "gu": // Gujarati
                case "ha": // Hausa
                case "haw": // Hawaiian
                case "he": // Hebrew
                case "is": // Icelandic
                case "it": // Italian
                case "kk": // Kazakh
                case "kl": // Kalaallisut
                case "ku": // Kurdish
                case "lb": // Luxembourgish
                case "lg": // Ganda
                case "lo": // Lao
                case "mas": // Masai
                case "ml": // Malayalam
                case "mn": // Mongolian
                case "mr": // Marathi
                case "nah": // Nahuatl
                case "nb": // Norwegian Bokmål
                case "ne": // Nepali
                case "nl": // Dutch
                case "nn": // Norwegian Nynorsk
                case "no": // Norwegian
                case "nyn": // Nyankole
                case "om": // Oromo
                case "or": // Oriya
                case "pa": // Punjabi
                case "pap": // Papiamento
                case "ps": // Pashto
                case "pt": // Portuguese
                case "rm": // Romansh
                case "saq": // Samburu
                case "so": // Somali
                case "sq": // Albanian
                case "ssy": // Saho
                case "sw": // Swahili
                case "sv": // Swedish
                case "syr": // Syriac
                case "ta": // Tamil
                case "te": // Telugu
                case "tk": // Turkmen
                case "tr": // Turkish
                case "ur": // Urdu
                case "wae": // Walser
                case "xog": // Soga
                case "zu": // Zulu
                    return (n, c) =>
                    {
                        if (c == 2) return (n == 1) ? 0 : 1;
                        if (c == 3) return (n == 0) ? 0 : (n == 1) ? 1 : 2;
                        if (c == 4) return (n < 0) ? 0 : (n == 0) ? 1 : (n == 1) ? 2 : 3;
                        return -1;
                    };

                // DualWithZero: one (n == 0..1), other
                case "ak": // Akan
                case "am": // Amharic
                case "bh": // Bihari
                case "fil": // Filipino
                case "guw": // Gun
                case "hi": // Hindi
                case "ln": // Lingala
                case "mg": // Malagasy
                case "nso": // Northern Sotho
                case "ti": // Tigrinya
                case "tl": // Tagalog
                case "wa": // Walloon
                    return (n, c) => (n == 0 || n == 1) ? 0 : 1;

                // DualFromZeroToTwo: one (n == 0..2 fractionate and n != 2), other
                case "ff": // Fulah
                case "fr": // French
                case "kab": // Kabyle
                    return (n, c) => (n >= 0 && n < 2) ? 0 : 1;

                // Triple: one (n == 1), two (n == 2), other
                case "ga": // Irish
                case "iu": // Inuktitut
                case "ksh": // Colognian
                case "kw": // Cornish
                case "se": // Northern Sami
                case "sma": // Southern Sami
                case "smi": // Sami language
                case "smj": // Lule Sami
                case "smn": // Inari Sami
                case "sms": // Skolt Sami
                    return (n, c) => (n == 1) ? 0 : (n == 2) ? 1 : 2;

                // Russian & Serbo-Croatian
                case "be": // Belarusian
                case "bs": // Bosnian
                case "hr": // Croatian
                case "ru": // Russian
                case "sh": // Serbo-Croatian
                case "sr": // Serbian
                case "uk": // Ukrainian
                    return (n, c) =>
                        (n % 10 == 1) && !(n % 100 == 11) ? 0 : // one
                        (n % 10).Between(2, 4) && !(n % 100).Between(12, 14) ? 1 : // few
                        2;

                // Unique:

                // Arabic
                case "ar":
                    return (n, c) =>
                        n == 0 ? 0 : // zero
                        n == 1 ? 1 : // one
                        n == 2 ? 2 : // two
                        (n % 100).Between(3, 10) ? 3 : // few
                        (n % 100).Between(11, 99) ? 4 : // many
                        5; // other

                // Breton
                case "br":
                    return (n, c) =>
                        n == 0 ? 0 : // zero
                        n == 1 ? 1 : // one
                        n == 2 ? 2 : // two
                        n == 3 ? 3 : // few
                        n == 6 ? 4 : // many
                        5; // other

                // Czech
                case "cs":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        n.Between(2, 4) ? 1 : // few
                        2;

                // Welsh
                case "cy":
                    return (n, c) =>
                        n == 0 ? 0 : // zero
                        n == 1 ? 1 : // one
                        n == 2 ? 2 : // two
                        n == 3 ? 3 : // few
                        n == 6 ? 4 : // many
                        5;

                // Manx
                case "gv":
                    return (n, c) =>
                        (n % 10).Between(1, 2) || (n % 20) == 0 ? 0 :  // one
                        1;

                // Langi
                case "lag":
                    return (n, c) =>
                        n == 0 ? 0 : // zero
                        (n > 0) && (n < 2) ? 1 : // one
                        2;

                // Lithuanian
                case "lt":
                    return (n, c) =>
                        (n % 10) == 1 && !(n % 100).Between(11, 19) ? 0 : // one
                        (n % 10).Between(2, 9) && !(n % 100).Between(11, 19) ? 1 : // few
                        2;

                // Latvian
                case "lv":
                    return (n, c) =>
                        n == 0 ? 0 : // zero
                        (n % 10) == 1 && (n % 100) != 11 ? 1 :
                        2;

                // Macedonian
                case "mb":
                    return (n, c) =>
                        (n % 10) == 1 && n != 11 ? 0 : // one
                        1;

                // Moldavian
                case "mo":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        n == 0 || (n != 1 && (n % 100).Between(1, 19)) ? 1 : // few
                        2;

                // Maltese
                case "mt":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        n == 0 || (n % 100).Between(2, 10) ? 1 : // few
                        (n % 100).Between(11, 19) ? 2 : // many
                        3;

                // Polish
                case "pl":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        (n % 10).Between(2, 4) && !(n % 100).Between(12, 14) ? 1 : // few
                        (n % 10).Between(0, 1) || (n % 10).Between(5, 9) || (n % 100).Between(12, 14) ? 2 : // many
                        3;

                // Romanian
                case "ro":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        n == 0 || (n % 100).Between(1, 19) ? 1 : // few
                        2;

                // Tachelhit
                case "shi":
                    return (n, c) =>
                        (n >= 0 && n <= 1) ? 0 : // one
                        n.Between(2, 10) ? 1 : // few
                        2;

                // Slovak
                case "sk":
                    return (n, c) =>
                        n == 1 ? 0 : // one
                        n.Between(2, 4) ? 1 : // few
                        2;

                // Slovenian
                case "sl":
                    return (n, c) =>
                        (n % 100) == 1 ? 0 : // one
                        (n % 100) == 2 ? 1 : // two
                        (n % 100).Between(3, 4) ? 2 : // few
                        3;

                // Central Morocco Tamazight
                case "tzm":
                    return (n, c) =>
                        n.Between(0, 1) || n.Between(11, 99) ? 0 : // one
                        1;


                // Unknown language
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns True if the value is inclusively between the min and max and has no fraction.
        /// </summary>
        private static bool Between(this decimal value, decimal min, decimal max)
        {
            return (value%1 == 0) && (value >= min) && (value <= max);
        }
    }

}
