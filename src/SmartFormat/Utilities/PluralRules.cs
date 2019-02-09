using System;
using System.Collections.Generic;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Assigns the ISO language code to a pluralization rule.
    /// </summary>
    public static class PluralRules
    {
        /// <summary>
        /// Holds the ISO langue code as key, and the <see cref="PluralRuleDelegate"/> with the pluralization rule.
        /// </summary>
        public static Dictionary<string, PluralRuleDelegate> IsoLangToDelegate =
            new Dictionary<string, PluralRuleDelegate>
            {
                // Singular
                {"az", Singular}, // Azerbaijani
                {"bm", Singular}, // Bambara
                {"bo", Singular}, // Tibetan
                {"dz", Singular}, // Dzongkha
                {"fa", Singular}, // Persian
                {"hu", Singular}, // Hungarian
                {"id", Singular}, // Indonesian
                {"ig", Singular}, // Igbo
                {"ii", Singular}, // Sichuan Yi
                {"ja", Singular}, // Japanese
                {"jv", Singular}, // Javanese
                {"ka", Singular}, // Georgian
                {"kde", Singular}, // Makonde
                {"kea", Singular}, // Kabuverdianu
                {"km", Singular}, // Khmer
                {"kn", Singular}, // Kannada
                {"ko", Singular}, // Korean
                {"ms", Singular}, // Malay
                {"my", Singular}, // Burmese
                {"root", Singular}, // Root (?)
                {"sah", Singular}, // Sakha
                {"ses", Singular}, // Koyraboro Senni
                {"sg", Singular}, // Sango
                {"th", Singular}, // Thai
                {"to", Singular}, // Tonga
                {"vi", Singular}, // Vietnamese
                {"wo", Singular}, // Wolof
                {"yo", Singular}, // Yoruba
                {"zh", Singular}, // Chinese
                // Dual: one (n == 1), other
                {"af", DualOneOther}, // Afrikaans
                {"bem", DualOneOther}, // Bembda
                {"bg", DualOneOther}, // Bulgarian
                {"bn", DualOneOther}, // Bengali
                {"brx", DualOneOther}, // Bodo
                {"ca", DualOneOther}, // Catalan
                {"cgg", DualOneOther}, // Chiga
                {"chr", DualOneOther}, // Cherokee
                {"da", DualOneOther}, // Danish
                {"de", DualOneOther}, // German
                {"dv", DualOneOther}, // Divehi
                {"ee", DualOneOther}, // Ewe
                {"el", DualOneOther}, // Greek
                {"en", DualOneOther}, // English
                {"eo", DualOneOther}, // Esperanto
                {"es", DualOneOther}, // Spanish
                {"et", DualOneOther}, // Estonian
                {"eu", DualOneOther}, // Basque
                {"fi", DualOneOther}, // Finnish
                {"fo", DualOneOther}, // Faroese
                {"fur", DualOneOther}, // Friulian
                {"fy", DualOneOther}, // Western Frisian
                {"gl", DualOneOther}, // Galician
                {"gsw", DualOneOther}, // Swiss German
                {"gu", DualOneOther}, // Gujarati
                {"ha", DualOneOther}, // Hausa
                {"haw", DualOneOther}, // Hawaiian
                {"he", DualOneOther}, // Hebrew
                {"is", DualOneOther}, // Icelandic
                {"it", DualOneOther}, // Italian
                {"kk", DualOneOther}, // Kazakh
                {"kl", DualOneOther}, // Kalaallisut
                {"ku", DualOneOther}, // Kurdish
                {"lb", DualOneOther}, // Luxembourgish
                {"lg", DualOneOther}, // Ganda
                {"lo", DualOneOther}, // Lao
                {"mas", DualOneOther}, // Masai
                {"ml", DualOneOther}, // Malayalam
                {"mn", DualOneOther}, // Mongolian
                {"mr", DualOneOther}, // Marathi
                {"nah", DualOneOther}, // Nahuatl
                {"nb", DualOneOther}, // Norwegian Bokmål
                {"ne", DualOneOther}, // Nepali
                {"nl", DualOneOther}, // Dutch
                {"nn", DualOneOther}, // Norwegian Nynorsk
                {"no", DualOneOther}, // Norwegian
                {"nyn", DualOneOther}, // Nyankole
                {"om", DualOneOther}, // Oromo
                {"or", DualOneOther}, // Oriya
                {"pa", DualOneOther}, // Punjabi
                {"pap", DualOneOther}, // Papiamento
                {"ps", DualOneOther}, // Pashto
                {"pt", DualOneOther}, // Portuguese
                {"rm", DualOneOther}, // Romansh
                {"saq", DualOneOther}, // Samburu
                {"so", DualOneOther}, // Somali
                {"sq", DualOneOther}, // Albanian
                {"ssy", DualOneOther}, // Saho
                {"sw", DualOneOther}, // Swahili
                {"sv", DualOneOther}, // Swedish
                {"syr", DualOneOther}, // Syriac
                {"ta", DualOneOther}, // Tamil
                {"te", DualOneOther}, // Telugu
                {"tk", DualOneOther}, // Turkmen
                {"tr", DualOneOther}, // Turkish
                {"ur", DualOneOther}, // Urdu
                {"wae", DualOneOther}, // Walser
                {"xog", DualOneOther}, // Soga
                {"zu", DualOneOther}, // Zulu
                // DualWithZero: one (n == 0..1), other
                {"ak", DualWithZero}, // Akan
                {"am", DualWithZero}, // Amharic
                {"bh", DualWithZero}, // Bihari
                {"fil", DualWithZero}, // Filipino
                {"guw", DualWithZero}, // Gun
                {"hi", DualWithZero}, // Hindi
                {"ln", DualWithZero}, // Lingala
                {"mg", DualWithZero}, // Malagasy
                {"nso", DualWithZero}, // Northern Sotho
                {"ti", DualWithZero}, // Tigrinya
                {"tl", DualWithZero}, // Tagalog
                {"wa", DualWithZero}, // Walloon
                // DualFromZeroToTwo: one (n == 0..2 fractionate and n != 2), other
                {"ff", DualFromZeroToTwo}, // Fulah
                {"fr", DualFromZeroToTwo}, // French
                {"kab", DualFromZeroToTwo}, // Kabyle
                // Triple: one (n == 1), two (n == 2), other
                {"ga", TripleOneTwoOther}, // Irish
                {"iu", TripleOneTwoOther}, // Inuktitut
                {"ksh", TripleOneTwoOther}, // Colognian
                {"kw", TripleOneTwoOther}, // Cornish
                {"se", TripleOneTwoOther}, // Northern Sami
                {"sma", TripleOneTwoOther}, // Southern Sami
                {"smi", TripleOneTwoOther}, // Sami language
                {"smj", TripleOneTwoOther}, // Lule Sami
                {"smn", TripleOneTwoOther}, // Inari Sami
                {"sms", TripleOneTwoOther}, // Skolt Sami
                // Russian & Serbo-Croatian
                {"be", RussianSerboCroatian}, // Belarusian
                {"bs", RussianSerboCroatian}, // Bosnian
                {"hr", RussianSerboCroatian}, // Croatian
                {"ru", RussianSerboCroatian}, // Russian
                {"sh", RussianSerboCroatian}, // Serbo-Croatian
                {"sr", RussianSerboCroatian}, // Serbian
                {"uk", RussianSerboCroatian}, // Ukrainian
                // Unique
                // Arabic
                {"ar", Arabic},
                // Breton
                {"br", Breton},
                // Czech
                {"cs", Czech},
                // Welsh
                {"cy", Welsh},
                // Manx
                {"gv", Manx},
                // Langi
                {"lag", Langi},
                // Lithuanian
                {"lt", Lithuanian},
                // Latvian
                {"lv", Latvian},
                // Macedonian
                {"mb", Macedonian},
                // Moldavian
                {"mo", Moldavian},
                // Maltese
                {"mt", Maltese},
                // Polish
                {"pl", Polish},
                // Romanian
                {"ro", Romanian},
                // Tachelhit
                {"shi", Tachelhit},
                // Slovak
                {"sk", Slovak},
                // Slovenian
                {"sl", Slovenian},
                // Central Morocco Tamazight
                {"tzm", CentralMoroccoTamazight}
            };

        private static PluralRuleDelegate Singular => (n, c) => 0;
        private static PluralRuleDelegate DualOneOther => (n, c) =>
        {
            if (c == 2) return n == 1 ? 0 : 1;
            if (c == 3) return n == 0 ? 0 : n == 1 ? 1 : 2;
            if (c == 4) return n < 0 ? 0 : n == 0 ? 1 : n == 1 ? 2 : 3;
            return -1;
        };// Dual: one (n == 1), other
        private static PluralRuleDelegate DualWithZero => (n, c) => n == 0 || n == 1 ? 0 : 1; // DualWithZero: one (n == 0..1), other
        private static PluralRuleDelegate DualFromZeroToTwo => (n, c) => n == 0 || n == 1 ? 0 : 1; // DualFromZeroToTwo: one (n == 0..2 fractionate and n != 2), other
        private static PluralRuleDelegate TripleOneTwoOther => (n, c) => n == 1 ? 0 : n == 2 ? 1 : 2; // Triple: one (n == 1), two (n == 2), other
        private static PluralRuleDelegate RussianSerboCroatian => (n, c) =>
            n % 10 == 1 && n % 100 != 11 ? 0 : // one
            (n % 10).Between(2, 4) && !(n % 100).Between(12, 14) ? 1 : // few
            2; // Russian & Serbo-Croatian
        private static PluralRuleDelegate Arabic => (n, c) =>
            n == 0 ? 0 : // zero
            n == 1 ? 1 : // one
            n == 2 ? 2 : // two
            (n % 100).Between(3, 10) ? 3 : // few
            (n % 100).Between(11, 99) ? 4 : // many
            5; // other
        private static PluralRuleDelegate Breton => (n, c) =>
            n == 0 ? 0 : // zero
            n == 1 ? 1 : // one
            n == 2 ? 2 : // two
            n == 3 ? 3 : // few
            n == 6 ? 4 : // many
            5; // other
        private static PluralRuleDelegate Czech => (n, c) =>
            n == 1 ? 0 : // one
            n.Between(2, 4) ? 1 : // few
            2;
        private static PluralRuleDelegate Welsh => (n, c) =>
            n == 0 ? 0 : // zero
            n == 1 ? 1 : // one
            n == 2 ? 2 : // two
            n == 3 ? 3 : // few
            n == 6 ? 4 : // many
            5;
        private static PluralRuleDelegate Manx => (n, c) =>
            (n % 10).Between(1, 2) || n % 20 == 0
                ? 0
                : // one
                1;
        private static PluralRuleDelegate Langi => (n, c) =>
            n == 0 ? 0 : // zero
            n > 0 && n < 2 ? 1 : // one
            2;
        private static PluralRuleDelegate Lithuanian => (n, c) =>
            n % 10 == 1 && !(n % 100).Between(11, 19) ? 0 : // one
            (n % 10).Between(2, 9) && !(n % 100).Between(11, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Latvian => (n, c) =>
            n == 0 ? 0 : // zero
            n % 10 == 1 && n % 100 != 11 ? 1 :
            2;
        private static PluralRuleDelegate Macedonian => (n, c) =>
            n % 10 == 1 && n != 11
                ? 0
                : // one
                1;
        private static PluralRuleDelegate Moldavian => (n, c) =>
            n == 1 ? 0 : // one
            n == 0 || n != 1 && (n % 100).Between(1, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Maltese => (n, c) =>
            n == 1 ? 0 : // one
            n == 0 || (n % 100).Between(2, 10) ? 1 : // few
            (n % 100).Between(11, 19) ? 2 : // many
            3;
        private static PluralRuleDelegate Polish => (n, c) =>
            n == 1 ? 0 : // one
            (n % 10).Between(2, 4) && !(n % 100).Between(12, 14) ? 1 : // few
            (n % 10).Between(0, 1) || (n % 10).Between(5, 9) || (n % 100).Between(12, 14) ? 2 : // many
            3;
        private static PluralRuleDelegate Romanian => (n, c) =>
            n == 1 ? 0 : // one
            n == 0 || (n % 100).Between(1, 19) ? 1 : // few
            2;
        private static PluralRuleDelegate Tachelhit => (n, c) =>
            n >= 0 && n <= 1 ? 0 : // one
            n.Between(2, 10) ? 1 : // few
            2;
        private static PluralRuleDelegate Slovak => (n, c) =>
            n == 1 ? 0 : // one
            n.Between(2, 4) ? 1 : // few
            2;
        private static PluralRuleDelegate Slovenian => (n, c) =>
            n % 100 == 1 ? 0 : // one
            n % 100 == 2 ? 1 : // two
            (n % 100).Between(3, 4) ? 2 : // few
            3;
        private static PluralRuleDelegate CentralMoroccoTamazight => (n, c) =>
            n.Between(0, 1) || n.Between(11, 99)
                ? 0
                : // one
                1;

        /// <summary>
        /// This delegate determines which singular or plural word should be chosen for the given quantity.
        /// This allows each language to define its own behavior for singular or plural words.
        /// </summary>
        /// <param name="value">The value that is being referenced by the singular or plural words</param>
        /// <param name="pluralCount"></param>
        /// <returns>Returns the index of the parameter to be used for pluralization.</returns>
        public delegate int PluralRuleDelegate(decimal value, int pluralCount);
        
        /// <summary>Construct a rule set for the language code.</summary>
        /// <param name="twoLetterISOLanguageName">The language code in two-letter ISO-639 format.</param>
        /// <remarks>
        /// The pluralization rules are taken from
        /// http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html
        /// </remarks>
        public static PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
        {
            return IsoLangToDelegate.ContainsKey(twoLetterISOLanguageName) ? IsoLangToDelegate[twoLetterISOLanguageName] : null;
        }

        /// <summary>
        /// Returns True if the value is inclusively between the min and max and has no fraction.
        /// </summary>
        private static bool Between(this decimal value, decimal min, decimal max)
        {
            return value % 1 == 0 && value >= min && value <= max;
        }
    }
}