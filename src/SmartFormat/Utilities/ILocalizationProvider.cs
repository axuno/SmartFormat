//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.Globalization;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Provides the localized equivalent of an input string.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Gets the localized equivalent of the <paramref name="input"/> string.
        /// </summary>
        /// <param name="input">The string to be localized.</param>
        /// <returns>The localized equivalent of the <paramref name="input"/> string, or <see langref="null"/> if not found.</returns>
        public string? GetString(string input);

        /// <summary>
        /// Gets the localized equivalent of the <paramref name="input"/> string.
        /// </summary>
        /// <param name="input">The string to be localized.</param>
        /// <param name="cultureName">The culture name to use for localization.</param>
        /// <returns>The localized equivalent of the <paramref name="input"/> string, or <see langref="null"/> if not found.</returns>
        public string? GetString(string input, string cultureName);

        /// <summary>
        /// Gets the localized equivalent of the <paramref name="input"/> string.
        /// </summary>
        /// <param name="input">The string to be localized.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use for localization.</param>
        /// <returns>The localized equivalent of the <paramref name="input"/> string, or <see langref="null"/> if not found.</returns>
        public string? GetString(string input, CultureInfo culture);
    }
}
