// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
// 

namespace SmartFormat.Utilities
{
    /// <summary>
    /// The class contains <see cref="TimeTextInfo"/> definitions for common languages.
    /// </summary>
    public static class CommonLanguagesTimeTextInfo
    {
        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the English language.
        /// </summary>
        public static TimeTextInfo English => new (
            PluralRules.GetPluralRule("en"),
            new[] {"{0} week", "{0} weeks"},
            new[] {"{0} day", "{0} days"},
            new[] {"{0} hour", "{0} hours"},
            new[] {"{0} minute", "{0} minutes"},
            new[] {"{0} second", "{0} seconds"},
            new[] {"{0} millisecond", "{0} milliseconds"},
            new[] {"{0}w"},
            new[] {"{0}d"},
            new[] {"{0}h"},
            new[] {"{0}m"},
            new[] {"{0}s"},
            new[] {"{0}ms"},
            "less than {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for a certain language.
        /// If the language is not implemented, the result will be <see langword="null"/>.
        /// </summary>
        /// <param name="twoLetterISOLanguageName"></param>
        /// <returns>
        /// The <see cref="TimeTextInfo"/> for a certain language.
        /// If the language is not implemented, the result will be <see langword="null"/>.
        /// </returns>
        public static TimeTextInfo? GetTimeTextInfo(string twoLetterISOLanguageName)
        {
            return twoLetterISOLanguageName switch
            {
                "en" => English,
                _ => null
            };
        }
    }
}