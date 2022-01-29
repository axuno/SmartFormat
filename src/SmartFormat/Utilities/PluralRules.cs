//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;

namespace SmartFormat.Utilities
{
#pragma warning disable S3776 // disable sonar cognitive complexity warnings
#pragma warning disable S3358 // disable sonar ternary operations warnings

    /// <summary>
    /// Assigns the ISO language code to a pluralization rule.
    /// <see cref="PluralRules"/> are used by extensions like <c>TimeFormatter</c> and <c>PluralLocalizationFormatter</c>
    /// </summary>
    public static class PluralRules
    {
        /// <summary>
        /// Holds the ISO language code as key, and the <see cref="PluralRuleDelegate"/> with the pluralization rule.
        /// </summary>
        public static Dictionary<string, PluralRuleDelegate> IsoLangToDelegate { get; } =
            new() {
                // Singular
                { "az", Singular }, // Azerbaijani
                { "bm", Singular }, // Bambara
                { "bo", Singular }, // Tibetan
                { "dz", Singular }, // Dzongkha
                { "fa", Singular }, // Persian
                { "hu", Singular }, // Hungarian
                { "id", Singular }, // Indonesian
                { "ig", Singular }, // Igbo
                { "ii", Singular }, // Sichuan Yi
                { "ja", Singular }, // Japanese
                { "jv", Singular }, // Javanese
                { "ka", Singular }, // Georgian
                { "kde", Singular }, // Makonde
                { "kea", Singular }, // Kabuverdianu
                { "km", Singular }, // Khmer
                { "kn", Singular }, // Kannada
                { "ko", Singular }, // Korean
                { "ms", Singular }, // Malay
                { "my", Singular }, // Burmese
                { "root", Singular }, // Root (?)
                { "sah", Singular }, // Sakha
                { "ses", Singular }, // Koyraboro Senni
                { "sg", Singular }, // Sango
                { "th", Singular }, // Thai
                { "to", Singular }, // Tonga
                { "vi", Singular }, // Vietnamese
                { "wo", Singular }, // Wolof
                { "yo", Singular }, // Yoruba
                { "zh", Singular }, // Chinese
                // Dual: one (n == 1), other
                { "af", DualOneOther }, // Afrikaans
                { "bem", DualOneOther }, // Bembda
                { "bg", DualOneOther }, // Bulgarian
                { "bn", DualOneOther }, // Bengali
                { "brx", DualOneOther }, // Bodo
                { "ca", DualOneOther }, // Catalan
                { "cgg", DualOneOther }, // Chiga
                { "chr", DualOneOther }, // Cherokee
                { "da", DualOneOther }, // Danish
                { "de", DualOneOther }, // German
                { "dv", DualOneOther }, // Divehi
                { "ee", DualOneOther }, // Ewe
                { "el", DualOneOther }, // Greek
                { "en", DualOneOther }, // English
                { "eo", DualOneOther }, // Esperanto
                { "es", DualOneOther }, // Spanish
                { "et", DualOneOther }, // Estonian
                { "eu", DualOneOther }, // Basque
                { "fi", DualOneOther }, // Finnish
                { "fo", DualOneOther }, // Faroese
                { "fur", DualOneOther }, // Friulian
                { "fy", DualOneOther }, // Western Frisian
                { "gl", DualOneOther }, // Galician
                { "gsw", DualOneOther }, // Swiss German
                { "gu", DualOneOther }, // Gujarati
                { "ha", DualOneOther }, // Hausa
                { "haw", DualOneOther }, // Hawaiian
                { "he", DualOneOther }, // Hebrew
                { "is", DualOneOther }, // Icelandic
                { "it", DualOneOther }, // Italian
                { "kk", DualOneOther }, // Kazakh
                { "kl", DualOneOther }, // Kalaallisut
                { "ku", DualOneOther }, // Kurdish
                { "lb", DualOneOther }, // Luxembourgish
                { "lg", DualOneOther }, // Ganda
                { "lo", DualOneOther }, // Lao
                { "mas", DualOneOther }, // Masai
                { "ml", DualOneOther }, // Malayalam
                { "mn", DualOneOther }, // Mongolian
                { "mr", DualOneOther }, // Marathi
                { "nah", DualOneOther }, // Nahuatl
                { "nb", DualOneOther }, // Norwegian Bokmål
                { "ne", DualOneOther }, // Nepali
                { "nl", DualOneOther }, // Dutch
                { "nn", DualOneOther }, // Norwegian Nynorsk
                { "no", DualOneOther }, // Norwegian
                { "nyn", DualOneOther }, // Nyankole
                { "om", DualOneOther }, // Oromo
                { "or", DualOneOther }, // Oriya
                { "pa", DualOneOther }, // Punjabi
                { "pap", DualOneOther }, // Papiamento
                { "ps", DualOneOther }, // Pashto
                { "pt", DualOneOther }, // Portuguese
                { "rm", DualOneOther }, // Romansh
                { "saq", DualOneOther }, // Samburu
                { "so", DualOneOther }, // Somali
                { "sq", DualOneOther }, // Albanian
                { "ssy", DualOneOther }, // Saho
                { "sw", DualOneOther }, // Swahili
                { "sv", DualOneOther }, // Swedish
                { "syr", DualOneOther }, // Syriac
                { "ta", DualOneOther }, // Tamil
                { "te", DualOneOther }, // Telugu
                { "tk", DualOneOther }, // Turkmen
                { "tr", DualOneOther }, // Turkish
                { "ur", DualOneOther }, // Urdu
                { "wae", DualOneOther }, // Walser
                { "xog", DualOneOther }, // Soga
                { "zu", DualOneOther }, // Zulu
                // DualWithZero: one (n == 0..1), other
                { "ak", DualWithZero }, // Akan
                { "am", DualWithZero }, // Amharic
                { "bh", DualWithZero }, // Bihari
                { "fil", DualWithZero }, // Filipino
                { "guw", DualWithZero }, // Gun
                { "hi", DualWithZero }, // Hindi
                { "ln", DualWithZero }, // Lingala
                { "mg", DualWithZero }, // Malagasy
                { "nso", DualWithZero }, // Northern Sotho
                { "ti", DualWithZero }, // Tigrinya
                { "tl", DualWithZero }, // Tagalog
                { "wa", DualWithZero }, // Walloon
                // DualFromZeroToTwo: one (n == 0..2 fractionate and n != 2), other
                { "ff", DualFromZeroToTwo }, // Fulah
                { "fr", DualFromZeroToTwo }, // French
                { "kab", DualFromZeroToTwo }, // Kabyle
                // Triple: one (n == 1), two (n == 2), other
                { "ga", TripleOneTwoOther }, // Irish
                { "iu", TripleOneTwoOther }, // Inuktitut
                { "ksh", TripleOneTwoOther }, // Colognian
                { "kw", TripleOneTwoOther }, // Cornish
                { "se", TripleOneTwoOther }, // Northern Sami
                { "sma", TripleOneTwoOther }, // Southern Sami
                { "smi", TripleOneTwoOther }, // Sami language
                { "smj", TripleOneTwoOther }, // Lule Sami
                { "smn", TripleOneTwoOther }, // Inari Sami
                { "sms", TripleOneTwoOther }, // Skolt Sami
                // Russian & Serbo-Croatian
                { "be", RussianSerboCroatian }, // Belarusian
                { "bs", RussianSerboCroatian }, // Bosnian
                { "hr", RussianSerboCroatian }, // Croatian
                { "ru", RussianSerboCroatian }, // Russian
                { "sh", RussianSerboCroatian }, // Serbo-Croatian
                { "sr", RussianSerboCroatian }, // Serbian
                { "uk", RussianSerboCroatian }, // Ukrainian
                // Unique
                // Arabic
                { "ar", Arabic },
                // Breton
                { "br", Breton },
                // Czech
                { "cs", Czech },
                // Welsh
                { "cy", Welsh },
                // Manx
                { "gv", Manx },
                // Langi
                { "lag", Langi },
                // Lithuanian
                { "lt", Lithuanian },
                // Latvian
                { "lv", Latvian },
                // Macedonian
                { "mb", Macedonian },
                // Moldavian
                { "mo", Moldavian },
                // Maltese
                { "mt", Maltese },
                // Polish
                { "pl", Polish },
                // Romanian
                { "ro", Romanian },
                // Tachelhit
                { "shi", Tachelhit },
                // Slovak
                { "sk", Slovak },
                // Slovenian
                { "sl", Slovenian },
                // Central Morocco Tamazight
                { "tzm", CentralMoroccoTamazight }
            };

        private static PluralRuleDelegate Singular => (value, pluralWordsCount) => 0;

        private static PluralRuleDelegate DualOneOther => (value, pluralWordsCount) =>
        {
            return pluralWordsCount switch {
                2 => value == 1 ? 0 : 1,
                3 => value switch {
                    0 => 0,
                    1 => 1,
                    _ => 2
                },
                4 => value < 0 ? 0 : value == 0 ? 1 : value == 1 ? 2 : 3,
                _ => -1
            };
        }; // Dual: one (n == 1), other

        private static PluralRuleDelegate DualWithZero =>
            (value, pluralWordsCount) => value == 0 || value == 1 ? 0 : 1; // DualWithZero: one (n == 0..1), other

        private static PluralRuleDelegate DualFromZeroToTwo => (value, pluralWordsCount) =>
        {
            if (pluralWordsCount == 2) return value < 2 ? 0 : 1;

            if (pluralWordsCount == 3) return GetWordsCount3Value(value);

            if (pluralWordsCount == 4) return GetWordsCount4Value(value);

            return -1;
        }; // DualFromZeroToTwo: one (n == 0..2 fractionate and n != 2), other

        private static int GetWordsCount3Value(decimal n)
        {
            return n switch {
                0 => 0,
                > 0 and < 2 => 1,
                > 2 => 2,
                _ => -1
            };
        }

        private static int GetWordsCount4Value(decimal n)
        {
            return n switch {
                < 0 => 0,
                0 => 1,
                > 0 and < 2 => 2,
                > 2 => 3,
                _ => -1
            };
        }
        
        private static PluralRuleDelegate TripleOneTwoOther => (value, pluralWordsCount) => value == 1 ? 0 : value == 2 ? 1 : 2; // Triple: one (n == 1), two (n == 2), other
        private static PluralRuleDelegate RussianSerboCroatian => (value, pluralWordsCount) =>
            value % 10 == 1 && value % 100 != 11 ? 0 : // one
            (value % 10).Between(2, 4) && !(value % 100).Between(12, 14) ? 1 : // few
            2; // Russian & Serbo-Croatian
        private static PluralRuleDelegate Arabic => (value, pluralWordsCount) =>
            value == 0 ? 0 : // zero
            value == 1 ? 1 : // one
            value == 2 ? 2 : // two
            (value % 100).Between(3, 10) ? 3 : // few
            (value % 100).Between(11, 99) ? 4 : // many
            5; // other
        private static PluralRuleDelegate Breton => (value, pluralWordsCount) =>
            value switch
            {
                0 => 0, // zero
                1 => 1, // one
                2 => 2, // two
                3 => 3, // few
                6 => 4, // many
                _ => 5  // other
            }; 
        private static PluralRuleDelegate Czech => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            value.Between(2, 4) ? 1 : // few
            2;
        private static PluralRuleDelegate Welsh => (value, pluralWordsCount) =>
            value switch
            {
                0 => 0, // zero
                1 => 1, // one
                2 => 2, // two
                3 => 3, // few
                6 => 4, // many
                _ => 5  // other
            };
        private static PluralRuleDelegate Manx => (value, pluralWordsCount) =>
            (value % 10).Between(1, 2) || value % 20 == 0
                ? 0
                : // one
                1;
        private static PluralRuleDelegate Langi => (value, pluralWordsCount) =>
            value == 0 ? 0 : // zero
            value is > 0 and < 2 ? 1 : // one
            2;
        private static PluralRuleDelegate Lithuanian => (value, pluralWordsCount) =>
            value % 10 == 1 && !(value % 100).Between(11, 19) ? 0 : // one
            (value % 10).Between(2, 9) && !(value % 100).Between(11, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Latvian => (value, pluralWordsCount) =>
            value == 0 ? 0 : // zero
            value % 10 == 1 && value % 100 != 11 ? 1 :
            2;
        private static PluralRuleDelegate Macedonian => (value, pluralWordsCount) =>
            value % 10 == 1 && value != 11
                ? 0
                : // one
                1;
        private static PluralRuleDelegate Moldavian => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            value == 0 || value != 1 && (value % 100).Between(1, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Maltese => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            value == 0 || (value % 100).Between(2, 10) ? 1 : // few
            (value % 100).Between(11, 19) ? 2 : // many
            3;
        private static PluralRuleDelegate Polish => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            (value % 10).Between(2, 4) && !(value % 100).Between(12, 14) ? 1 : // few
            (value % 10).Between(0, 1) || (value % 10).Between(5, 9) || (value % 100).Between(12, 14) ? 2 : // many
            3;
        private static PluralRuleDelegate Romanian => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            value == 0 || (value % 100).Between(1, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Tachelhit => (value, pluralWordsCount) =>
            value >= 0 && value <= 1 ? 0 : // one
            value.Between(2, 10) ? 1 : // few
            2;
        private static PluralRuleDelegate Slovak => (value, pluralWordsCount) =>
            value == 1 ? 0 : // one
            value.Between(2, 4) ? 1 : // few
            2;
        private static PluralRuleDelegate Slovenian => (value, pluralWordsCount) =>
            value % 100 == 1 ? 0 : // one
            value % 100 == 2 ? 1 : // two
            (value % 100).Between(3, 4) ? 2 : // few
            3;
        private static PluralRuleDelegate CentralMoroccoTamazight => (value, pluralWordsCount) =>
            value.Between(0, 1) || value.Between(11, 99)
                ? 0
                : // one
                1;

        /// <summary>
        /// This delegate determines which singular or plural word should be chosen for the given quantity.
        /// This allows each language to define its own behavior for singular or plural words.
        /// </summary>
        /// <param name="value">The value that is being referenced by the singular or plural words</param>
        /// <param name="pluralWordsCount">The number of plural words</param>
        /// <returns>Returns the index of the parameter to be used for pluralization.</returns>
        public delegate int PluralRuleDelegate(decimal value, int pluralWordsCount);
        
        /// <summary>Construct a rule set for the language code.</summary>
        /// <param name="twoLetterIsoLanguageName">The language code in two-letter ISO-639 format.</param>
        /// <remarks>
        /// The pluralization rules are taken from
        /// http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html
        /// </remarks>
        public static PluralRuleDelegate GetPluralRule(string? twoLetterIsoLanguageName)
        {
            if (twoLetterIsoLanguageName != null && IsoLangToDelegate.ContainsKey(twoLetterIsoLanguageName)) 
                return IsoLangToDelegate[twoLetterIsoLanguageName];

            throw new ArgumentException($"{nameof(IsoLangToDelegate)} not found for {twoLetterIsoLanguageName ?? "'null'"}", nameof(twoLetterIsoLanguageName));
        }

        /// <summary>
        /// Returns <see langword="true"/> if the value is inclusively between the min and max and has no fraction.
        /// </summary>
        private static bool Between(this decimal value, decimal min, decimal max)
        {
            return value % 1 == 0 && value >= min && value <= max;
        }
#pragma warning restore S3358
#pragma warning restore S3776
    }
}
