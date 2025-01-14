// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions.Time.Utilities;

/// <summary>
/// The class contains <see cref="TimeTextInfo"/> definitions for common languages.
/// </summary>
public static class CommonLanguagesTimeTextInfo
{
    private static readonly Dictionary<string, TimeTextInfo> _customLanguage = new();

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the English language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"en\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo English => GetTimeTextInfo("en")!;

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the French language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"fr\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo French => GetTimeTextInfo("fr")!;

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the Spanish language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"es\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo Spanish => GetTimeTextInfo("es")!;

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the Portuguese language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"pt\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo Portuguese => GetTimeTextInfo("pt")!;

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the Italian language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"it\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo Italian => GetTimeTextInfo("it")!;

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for the German language.
    /// </summary>
    [Obsolete("Use GetTimeTextInfo(\"de\") instead")]
    [ExcludeFromCodeCoverage]
    public static TimeTextInfo German => GetTimeTextInfo("de")!;
 
    /// <summary>
    /// Adds a <see cref="TimeTextInfo"/> for a language.
    /// </summary>
    /// <param name="twoLetterIsoLanguageName">The string to get the associated <see cref="System.Globalization.CultureInfo"/></param>
    /// <param name="timeTextInfo">The localized <see cref="TimeTextInfo"/></param>
    public static void AddLanguage(string twoLetterIsoLanguageName, TimeTextInfo timeTextInfo)
    {
        var c = twoLetterIsoLanguageName.ToLower();
        _customLanguage.Add(c, timeTextInfo);
    }

    /// <summary>
    /// Gets the <see cref="TimeTextInfo"/> for a certain language.
    /// If the language is not implemented, the result will be <see langword="null"/>.
    /// </summary>
    /// <param name="twoLetterIsoLanguageName"></param>
    /// <returns>
    /// The <see cref="TimeTextInfo"/> for a certain language.
    /// If the language is not implemented, the result will be <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Custom languages can be added with <see cref="AddLanguage"/>.
    /// Custom languages override any built-in language with the same twoLetterISOLanguageName.
    /// </remarks>
    public static TimeTextInfo? GetTimeTextInfo(string twoLetterIsoLanguageName)
    {
        if (_customLanguage.TryGetValue(twoLetterIsoLanguageName, out var timeTextInfo))
            return timeTextInfo;

        timeTextInfo = LoadTimeTextInfo(twoLetterIsoLanguageName);
        if (timeTextInfo is null) return null;

        _customLanguage.Add(twoLetterIsoLanguageName, timeTextInfo);
        return timeTextInfo;
    }

    private static TimeTextInfo? LoadTimeTextInfo(string languageCode)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"SmartFormat.Extensions.Time.Resources.{languageCode}.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var data = JsonSerializer.Deserialize<TimeTextInfoData>(json);

        return new TimeTextInfo
        {
            PluralRule = PluralRules.GetPluralRule(data!.PluralRule),
            Ptxt_week = data.Ptxt_week,
            Ptxt_day = data.Ptxt_day,
            Ptxt_hour = data.Ptxt_hour,
            Ptxt_minute = data.Ptxt_minute,
            Ptxt_second = data.Ptxt_second,
            Ptxt_millisecond = data.Ptxt_millisecond,
            Ptxt_w = data.Ptxt_w,
            Ptxt_d = data.Ptxt_d,
            Ptxt_h = data.Ptxt_h,
            Ptxt_m = data.Ptxt_m,
            Ptxt_s = data.Ptxt_s,
            Ptxt_ms = data.Ptxt_ms,
            Ptxt_lessThan = data.Ptxt_lessThan
        };
    }
}
