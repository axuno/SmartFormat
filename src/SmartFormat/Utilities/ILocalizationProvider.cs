// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System.Globalization;

namespace SmartFormat.Utilities;

/// <summary>
/// Provides the localized equivalent of an input string.
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// Gets the localized equivalent of the <paramref name="name"/> string.
    /// </summary>
    /// <param name="name">The string to be localized.</param>
    /// <returns>The localized equivalent of the <paramref name="name"/> string, or <see langref="null"/> if not found.</returns>
    public string? GetString(string name);

    /// <summary>
    /// Gets the localized equivalent of the <paramref name="name"/> string.
    /// </summary>
    /// <param name="name">The string to be localized.</param>
    /// <param name="cultureName">The culture name to use for localization.</param>
    /// <returns>The localized equivalent of the <paramref name="name"/> string, or <see langref="null"/> if not found.</returns>
    public string? GetString(string name, string cultureName);

    /// <summary>
    /// Gets the localized equivalent of the <paramref name="name"/> string.
    /// </summary>
    /// <param name="name">The string to be localized.</param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for localization.</param>
    /// <returns>The localized equivalent of the <paramref name="name"/> string, or <see langref="null"/> if not found.</returns>
    public string? GetString(string name, CultureInfo cultureInfo);
}