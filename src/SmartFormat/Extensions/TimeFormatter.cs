//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// A class to format and output <see cref="TimeSpan"/> values.
    /// </summary>
    public class TimeFormatter : IFormatter
    {
        ///<inheritdoc/>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"timespan", "time", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "time";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;

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
            
            string options;
            if (!string.IsNullOrEmpty(formattingInfo.FormatterOptions))
                options = formattingInfo.FormatterOptions!;
            else if (format != null)
                options = format.GetLiteralText();
            else
                options = string.Empty;

            TimeSpan? fromTime = null;
            
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
                    break;
                case DateTimeOffset dateTimeOffset:
                    if (formattingInfo.FormatterOptions != string.Empty)
                    {
                        fromTime = SystemTime.OffsetNow().UtcDateTime.Subtract(dateTimeOffset.UtcDateTime);
                    }
                    break;
            }

            if (fromTime is null)
            {
                // Auto detection calls just return a failure to evaluate
                if (formatterName == string.Empty)
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formatterName}' can only process types of {nameof(TimeSpan)}, {nameof(DateTime)}, {nameof(DateTimeOffset)}");
            }

            var timeTextInfo = GetTimeTextInfo(formattingInfo.FormatDetails.Provider);
            if (timeTextInfo == null) throw new FormatException($"{nameof(TimeTextInfo)} could not be found for the given {nameof(IFormatProvider)}.");
            var formattingOptions = TimeSpanFormatOptionsConverter.Parse(options);
            var timeString = fromTime.Value.ToTimeString(formattingOptions, timeTextInfo);
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
            if (provider is not CultureInfo cultureInfo)
                return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);

            // If cultureInfo was supplied,
            // we will always return, even if null:
            return CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
        }

        #endregion
    }
}