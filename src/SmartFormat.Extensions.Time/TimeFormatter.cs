// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Globalization;
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
    public string[] Names { get; set; } = {"timespan", "time", string.Empty};

    ///<inheritdoc/>
    public string Name { get; set; } = "time";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = true;

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
    public string DefaultTwoLetterISOLanguageName { get; set; } = "en";

    #endregion

    #region IFormatter

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;
        var formatterName = formattingInfo.Placeholder?.FormatterName ?? string.Empty;
        var current = formattingInfo.CurrentValue;

        // Check whether arguments can be handled by this formatter
        if (format is {HasNested: true})
        {
            // Auto detection calls just return a failure to evaluate
            if(formatterName == string.Empty)
                return false;
                
            // throw, if the formatter has been called explicitly
            throw new FormatException($"Formatter named '{formatterName}' cannot handle nested formats.");
        }
            
        var options = formattingInfo.FormatterOptions.Trim();
        var formatText = format?.RawText.Trim() ?? string.Empty;

        // Not clear, whether we can process this format
        if (formatterName == string.Empty && options == string.Empty && formatText == string.Empty) return false;

        // In SmartFormat 2.x, the format could be included in options, with empty format.
        // Using compatibility with v2, there is no reliable way to set a language as an option
        var v2Compatibility = options != string.Empty && formatText == string.Empty;
        var formattingOptions = v2Compatibility ? options : formatText;

        var fromTime = GetFromTime(current, formattingOptions);
            
        if (fromTime is null)
        {
            // Auto detection calls just return a failure to evaluate
            if (formatterName == string.Empty)
                return false;

            // throw, if the formatter has been called explicitly
            throw new FormatException(
                $"Formatter named '{formatterName}' can only process types of {nameof(TimeSpan)}, {nameof(DateTime)}, {nameof(DateTimeOffset)}");
        }

        var timeTextInfo = GetTimeTextInfo(formattingInfo, v2Compatibility);
            
        var timeSpanFormatOptions = TimeSpanFormatOptionsConverter.Parse(v2Compatibility ? options : formatText);
        var timeString = fromTime.Value.ToTimeString(timeSpanFormatOptions, timeTextInfo);
        formattingInfo.Write(timeString);
        return true;
    }

    private static TimeSpan? GetFromTime(object? current, string? formattingOptions)
    {
        TimeSpan? fromTime = null;

        switch (current)
        {
            case TimeSpan timeSpan:
                fromTime = timeSpan;
                break;
            case DateTime dateTime:
                if (formattingOptions != string.Empty)
                {
                    fromTime = SystemTime.Now().ToUniversalTime().Subtract(dateTime.ToUniversalTime());
                }
                break;
            case DateTimeOffset dateTimeOffset:
                if (formattingOptions != string.Empty)
                {
                    fromTime = SystemTime.OffsetNow().UtcDateTime.Subtract(dateTimeOffset.UtcDateTime);
                }
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

        if(timeTextInfoFromCulture is null && FallbackLanguage == string.Empty)
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