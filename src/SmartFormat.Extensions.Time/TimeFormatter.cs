// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions.Time.Utilities;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

/// <summary>
/// A class to format and output <see cref="TimeSpan"/> values.
/// </summary>
public class TimeFormatter : IFormatter
{
    private string _fallbackLanguage = "en";

    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    [ExcludeFromCodeCoverage]
    public string[] Names { get; set; } = {"timespan", "time", string.Empty};

    ///<inheritdoc/>
    public string Name { get; set; } = "time";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = false;

    #region Constructors

    /// <summary>
    /// Initializes the extension with a default <see cref="TimeTextInfo"/>.
    /// </summary>
    /// <remarks>
    /// Culture is determined in this sequence:<br/>
    /// 1. Get the culture from the <see cref="FormattingInfo.FormatterOptions"/>.<br/>
    /// 2. Get the culture from the <see cref="IFormatProvider"/> argument (which may be a <see cref="CultureInfo"/>) to <see cref="SmartFormatter.Format(IFormatProvider, string, object?[])"/><br/>
    /// 3. The <see cref="CultureInfo.CurrentUICulture"/>.<br/><br/>
    /// <see cref="TimeFormatter"/> makes use of <see cref="PluralRules"/> and <see cref="PluralLocalizationFormatter"/>.
    /// </remarks>
    public TimeFormatter()
    {
        DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;
    }

    /// <summary>
    /// Initializes the extension with a default <see cref="TimeTextInfo"/>.
    /// </summary>
    /// <remarks>
    /// Culture is determined in this sequence:<br/>
    /// 1. Get the culture from the <see cref="FormattingInfo.FormatterOptions"/>.<br/>
    /// 2. Get the culture from the <see cref="IFormatProvider"/> argument (which may be a <see cref="CultureInfo"/>) to <see cref="SmartFormatter.Format(IFormatProvider, string, object?[])"/><br/>
    /// 3. The <see cref="CultureInfo.CurrentUICulture"/>.<br/><br/>
    /// <see cref="TimeFormatter"/> makes use of <see cref="PluralRules"/> and <see cref="PluralLocalizationFormatter"/>.
    /// </remarks>
    [Obsolete("This constructor is not required. Changed process to determine the default culture.", true)]
    [ExcludeFromCodeCoverage]   
    public TimeFormatter(string defaultTwoLetterLanguageName)
    {
        if (CommonLanguagesTimeTextInfo.GetTimeTextInfo(defaultTwoLetterLanguageName) == null)
            throw new ArgumentException($"Language '{defaultTwoLetterLanguageName}' for {nameof(defaultTwoLetterLanguageName)} is not implemented.");

        DefaultTwoLetterISOLanguageName = defaultTwoLetterLanguageName;
        DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;
    }

    #endregion

    #region Defaults

    /// <summary>
    /// Determines the options for time formatting.
    /// </summary>
    public TimeSpanFormatOptions DefaultFormatOptions { get; set; }

    /// <summary>
    /// Gets or sets, the fallback language that is used if no supported language was found.
    /// Default is "en". If no fallback language shall be used, set it to <see cref="string.Empty"/>.
    /// </summary>
    /// <exception cref="Exception">If no <see cref="TimeTextInfo"/> could be found for the language.</exception>
    public string FallbackLanguage
    {
        get
        {
            return _fallbackLanguage;
        }

        set
        {
            if(value == string.Empty)
                _fallbackLanguage = value;
            else if (CommonLanguagesTimeTextInfo.GetTimeTextInfo(value) != null)
                _fallbackLanguage = value;
            else
                throw new ArgumentException($"No {nameof(TimeTextInfo)} found for language '{value}'.");
        }
    }

    /// <summary>
    /// The ISO language name, which will be used for getting the <see cref="TimeTextInfo"/>.
    /// </summary>
    /// <remarks>
    /// Culture is now determined in this sequence:<br/>
    /// 1. Get the culture from the <see cref="FormattingInfo.FormatterOptions"/>.<br/>
    /// 2. Get the culture from the <see cref="IFormatProvider"/> argument (which may be a <see cref="CultureInfo"/>) to <see cref="SmartFormatter.Format(IFormatProvider, string, object?[])"/><br/>
    /// 3. The <see cref="CultureInfo.CurrentUICulture"/>.<br/>
    /// </remarks>
    [Obsolete("This property is not supported any more. Changed process to get or set the default culture.", true)]
    [ExcludeFromCodeCoverage]
    public string DefaultTwoLetterISOLanguageName { get; set; } = "en";

    #endregion

    #region IFormatter

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;

        // Auto-detection calls just return a failure to evaluate
        if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
            return false;

#if NET6_0_OR_GREATER
        if (formattingInfo.CurrentValue is not (TimeSpan or DateTime or DateTimeOffset or TimeOnly))
            throw new FormattingException(formattingInfo.Format?.Items.FirstOrDefault(),
                $"'{nameof(TimeFormatter)}' can only process types of " +
                $"{nameof(TimeSpan)}, {nameof(DateTime)}, {nameof(DateTimeOffset)}, {nameof(TimeOnly)}, " +
                $"but not '{formattingInfo.CurrentValue?.GetType()}'", 0);
#else
        if (formattingInfo.CurrentValue is not (TimeSpan or DateTime or DateTimeOffset))
            throw new FormattingException(formattingInfo.Format?.Items.FirstOrDefault(),
                $"'{nameof(TimeFormatter)}' can only process types of " +
                $"{nameof(TimeSpan)}, {nameof(DateTime)}, {nameof(DateTimeOffset)}, " +
                $"but not '{formattingInfo.CurrentValue?.GetType()}'", 0);
#endif

        // Now we have to check for a nested format.
        // That is the one needed for the ListFormatter
        var timeParts = GetTimeParts(formattingInfo);
        if (timeParts is null) return false;

        if (format is { Length: > 1, HasNested: true })
        {
            // Remove the format for the TimeFormatter
            format.Items.RemoveAt(0);
            // Try to invoke the child format - usually the ListFormatter
            // to format the list of time parts
            formattingInfo.FormatAsChild(format, timeParts);
            return true;
        }

        formattingInfo.Write(string.Join(" ", timeParts));
        return true;
    }

    private IList<string>? GetTimeParts(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;
        var formatterName = formattingInfo.Placeholder?.FormatterName ?? string.Empty;

        var current = formattingInfo.CurrentValue;

        var options = formattingInfo.FormatterOptions.Trim();
        var formatText = format?.RawText.Trim() ?? string.Empty;

        // In SmartFormat 2.x, the format could be included in options, with empty format.
        // Using compatibility with v2, there is no reliable way to set a language as an option
        var v2Compatibility = options != string.Empty && formatText == string.Empty;
        var formattingOptions = v2Compatibility ? options : formatText;

        var fromTime = GetFromTime(current);

        if (fromTime == null) return null;

        var timeTextInfo = GetTimeTextInfo(formattingInfo, v2Compatibility);

        var timeSpanFormatOptions = TimeSpanFormatOptionsConverter.Parse(formattingOptions);
        return fromTime.Value.ToTimeParts(timeSpanFormatOptions, timeTextInfo);
    }

    private static TimeSpan? GetFromTime(object? current)
    {
        TimeSpan? fromTime = null;

        switch (current)
        {
            case TimeSpan timeSpan:
                fromTime = timeSpan;
                break;
#if NET6_0_OR_GREATER
            case TimeOnly timeOnly:
                fromTime = timeOnly.ToTimeSpan();
                break;
#endif
            case DateTime dateTime:
                fromTime = SystemTime.Now().ToUniversalTime().Subtract(dateTime.ToUniversalTime());
                break;
            case DateTimeOffset dateTimeOffset:
                fromTime = SystemTime.OffsetNow().UtcDateTime.Subtract(dateTimeOffset.UtcDateTime);
                break;
        }

        return fromTime;
    }

    private TimeTextInfo GetTimeTextInfo(IFormattingInfo formattingInfo, bool v2Compatibility)
    {
        // See if the provider can give us a TimeTextInfo:
        if (formattingInfo.FormatDetails.Provider?.GetFormat(typeof(TimeTextInfo)) is TimeTextInfo timeTextInfo) return timeTextInfo;

        // Figure out the culture to use
        var culture = GetCultureInfo(formattingInfo, v2Compatibility);
        // See if there is a rule for this culture:
        var timeTextInfoFromCulture = CommonLanguagesTimeTextInfo.GetTimeTextInfo(culture.TwoLetterISOLanguageName);

        if (timeTextInfoFromCulture != null) return timeTextInfoFromCulture;

        if(FallbackLanguage == string.Empty)
            throw new FormattingException(formattingInfo.Placeholder, $"{nameof(TimeTextInfo)} could not be found for the given culture argument '{formattingInfo.FormatterOptions}'.", 0);

        if(FallbackLanguage != string.Empty)
            return CommonLanguagesTimeTextInfo.GetTimeTextInfo(FallbackLanguage)!;

        throw new ArgumentException($"{nameof(TimeTextInfo)} could not be found for the given {nameof(IFormatProvider)}.", nameof(formattingInfo));
    }

#endregion

    private static CultureInfo GetCultureInfo(IFormattingInfo formattingInfo, bool v2Compatibility)
    {
        var culture = !v2Compatibility ? formattingInfo.FormatterOptions.Trim() : string.Empty;
        CultureInfo cultureInfo;
        if (culture == string.Empty)
        {
            if (formattingInfo.FormatDetails.Provider is CultureInfo ci)
                cultureInfo = ci;
            else
                cultureInfo = CultureInfo.CurrentUICulture; // also used this way by ResourceManager
        }
        else
        {
            cultureInfo = CultureInfo.GetCultureInfo(culture);
        }

        return cultureInfo;
    }
}
