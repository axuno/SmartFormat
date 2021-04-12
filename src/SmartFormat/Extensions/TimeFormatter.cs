//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Net.Utilities;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    public class TimeFormatter : IFormatter
    {
        public string[] Names { get; set; } = {"timespan", "time", "t", ""};

        #region Constructors

        /// <summary>
        /// Initializes the extension with a default TimeTextInfo.
        /// </summary>
        /// <param name="defaultTwoLetterLanguageName">This will be used when no CultureInfo is supplied.  Can be null.</param>
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
        /// The ISO language name, which will be used for getting the <see cref="TimeTextInfo"/>.
        /// </summary>
        public string DefaultTwoLetterISOLanguageName { get; set; }

        #endregion

        #region IFormatter

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            if (format != null && format.HasNested) return false;
            string options;
            if (!string.IsNullOrEmpty(formattingInfo.FormatterOptions))
                options = formattingInfo.FormatterOptions!;
            else if (format != null)
                options = format.GetLiteralText();
            else
                options = string.Empty;

            TimeSpan fromTime;
            
            switch (current)
            {
                case TimeSpan timeSpan:
                    fromTime = timeSpan;
                    break;
                case DateTime dateTime:
                    if (formattingInfo.FormatterOptions != string.Empty)
                    {
                        fromTime = SystemTime.Now().ToUniversalTime().Subtract(dateTime.ToUniversalTime());
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case DateTimeOffset dateTimeOffset:
                    if (formattingInfo.FormatterOptions != string.Empty)
                    {
                        fromTime = SystemTime.OffsetNow().UtcDateTime.Subtract(dateTimeOffset.UtcDateTime);
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            var timeTextInfo = GetTimeTextInfo(formattingInfo.FormatDetails.Provider);
            if (timeTextInfo == null) return false;
            var formattingOptions = TimeSpanFormatOptionsConverter.Parse(options);
            var timeString = fromTime.ToTimeString(formattingOptions, timeTextInfo);
            formattingInfo.Write(timeString);
            return true;
        }

        private TimeTextInfo? GetTimeTextInfo(IFormatProvider? provider)
        {
            // Return the default if there is no provider:
            if (provider == null)
                return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);
            
            // See if the provider can give us what we want:
            if (provider.GetFormat(typeof(TimeTextInfo)) is TimeTextInfo timeTextInfo) return timeTextInfo;

            // See if there is a rule for this culture:
            if (!(provider is CultureInfo cultureInfo))
                return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);

            // If cultureInfo was supplied,
            // we will always return, even if null:
            return CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
        }

        #endregion
    }
}