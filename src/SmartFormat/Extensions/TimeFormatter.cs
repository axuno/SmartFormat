using System;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    public class TimeFormatter : IFormatter
    {
        public string[] Names { get; set; } = {"timespan", "time", "t", ""};

        #region Constructors

        /// <summary>
        /// Initializes the extension with no default TimeTextInfo.
        /// </summary>
        public TimeFormatter() : this(null)
        {
        }

        /// <summary>
        /// Initializes the extension with a default TimeTextInfo.
        /// </summary>
        /// <param name="defaultTwoLetterLanguageName">This will be used when no CultureInfo is supplied.  Can be null.</param>
        public TimeFormatter(string defaultTwoLetterLanguageName)
        {
            DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;
            DefaultTwoLetterISOLanguageName = defaultTwoLetterLanguageName;
        }

        #endregion

        #region Defaults

        public TimeSpanFormatOptions DefaultFormatOptions { get; set; }
        public string DefaultTwoLetterISOLanguageName { get; set; }

        #endregion

        #region IFormatter

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            if (format != null && format.HasNested) return false;
            string options;
            if (formattingInfo.FormatterOptions != "")
                options = formattingInfo.FormatterOptions;
            else if (format != null)
                options = format.GetLiteralText();
            else
                options = "";

            TimeSpan fromTime;
            if (current is TimeSpan)
            {
                fromTime = (TimeSpan) current;
            }
            else if (current is DateTime && formattingInfo.FormatterOptions != "")
            {
                fromTime = DateTime.Now.Subtract((DateTime) current);
            }
            else if (current is DateTime && options.StartsWith("timestring"))
            {
                options = options.Substring(10);
                fromTime = DateTime.Now.Subtract((DateTime) current);
            }
            else
            {
                return false;
            }

            var timeTextInfo = GetTimeTextInfo(formattingInfo.FormatDetails.Provider);
            if (timeTextInfo == null) return false;
            var formattingOptions = TimeSpanFormatOptionsConverter.Parse(options);
            var timeString = fromTime.ToTimeString(formattingOptions, timeTextInfo);
            formattingInfo.Write(timeString);
            return true;
        }

        private TimeTextInfo GetTimeTextInfo(IFormatProvider provider)
        {
            if (provider != null)
            {
                // See if the provider can give us what we want:
                var timeTextInfo = (TimeTextInfo) provider.GetFormat(typeof(TimeTextInfo));
                if (timeTextInfo != null) return timeTextInfo;

                // See if there is a rule for this culture:
                var cultureInfo = provider as CultureInfo;
                if (cultureInfo != null)
                {
                    timeTextInfo = CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
                    // If cultureInfo was supplied,
                    // we will always return, even if null:
                    return timeTextInfo;
                }
            }

            // Return the default if the provider couldn't provide:
            return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);
        }

        #endregion
    }
}