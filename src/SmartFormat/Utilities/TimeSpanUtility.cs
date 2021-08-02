//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Utility class to format a <see cref="TimeSpan"/> as a <see langword="string"/>.
    /// </summary>
    public static class TimeSpanUtility
    {
        #region: ToTimeString :

        /// <summary>
        /// <para>Turns a TimeSpan into a human-readable text.</para>
        /// <para>Uses the specified timeSpanFormatOptions.</para>
        /// <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
        /// </summary>
        /// <param name="FromTime"></param>
        /// <param name="options">
        /// <para>A combination of flags that determine the formatting options.</para>
        /// <para>These will be combined with the default timeSpanFormatOptions.</para>
        /// </param>
        /// <param name="timeTextInfo">An object that supplies the text to use for output</param>
        public static string ToTimeString(this TimeSpan FromTime, TimeSpanFormatOptions options,
            TimeTextInfo timeTextInfo)
        {
            // If there are any missing options, merge with the defaults:
            // Also, as a safeguard against missing DefaultFormatOptions, let's also merge with the AbsoluteDefaults:
            options = options.Merge(DefaultFormatOptions).Merge(AbsoluteDefaults);
            
            // Extract the individual options:
            var rangeMax = options.Mask(TimeSpanFormatOptions._Range).AllFlags().Last();
            var rangeMin = options.Mask(TimeSpanFormatOptions._Range).AllFlags().First();
            var truncate = options.Mask(TimeSpanFormatOptions._Truncate).AllFlags().First();
            var lessThan = options.Mask(TimeSpanFormatOptions._LessThan) != TimeSpanFormatOptions.LessThanOff;
            var abbreviate = options.Mask(TimeSpanFormatOptions._Abbreviate) != TimeSpanFormatOptions.AbbreviateOff;
            var round = lessThan ? (Func<double, double>) Math.Floor : Math.Ceiling;

            switch (rangeMin)
            {
                case TimeSpanFormatOptions.RangeWeeks:
                    FromTime = TimeSpan.FromDays(round(FromTime.TotalDays / 7) * 7);
                    break;
                case TimeSpanFormatOptions.RangeDays:
                    FromTime = TimeSpan.FromDays(round(FromTime.TotalDays));
                    break;
                case TimeSpanFormatOptions.RangeHours:
                    FromTime = TimeSpan.FromHours(round(FromTime.TotalHours));
                    break;
                case TimeSpanFormatOptions.RangeMinutes:
                    FromTime = TimeSpan.FromMinutes(round(FromTime.TotalMinutes));
                    break;
                case TimeSpanFormatOptions.RangeSeconds:
                    FromTime = TimeSpan.FromSeconds(round(FromTime.TotalSeconds));
                    break;
                case TimeSpanFormatOptions.RangeMilliSeconds:
                    FromTime = TimeSpan.FromMilliseconds(round(FromTime.TotalMilliseconds));
                    break;
            }

            // Create our result:
            var textStarted = false;
            var result = new StringBuilder();
            for (var i = rangeMax; i >= rangeMin; i = (TimeSpanFormatOptions) ((int) i >> 1))
            {
                // Determine the value and title:
                int value;
                switch (i)
                {
                    case TimeSpanFormatOptions.RangeWeeks:
                        value = (int) Math.Floor(FromTime.TotalDays / 7);
                        FromTime -= TimeSpan.FromDays(value * 7);
                        break;
                    case TimeSpanFormatOptions.RangeDays:
                        value = (int) Math.Floor(FromTime.TotalDays);
                        FromTime -= TimeSpan.FromDays(value);
                        break;
                    case TimeSpanFormatOptions.RangeHours:
                        value = (int) Math.Floor(FromTime.TotalHours);
                        FromTime -= TimeSpan.FromHours(value);
                        break;
                    case TimeSpanFormatOptions.RangeMinutes:
                        value = (int) Math.Floor(FromTime.TotalMinutes);
                        FromTime -= TimeSpan.FromMinutes(value);
                        break;
                    case TimeSpanFormatOptions.RangeSeconds:
                        value = (int) Math.Floor(FromTime.TotalSeconds);
                        FromTime -= TimeSpan.FromSeconds(value);
                        break;
                    case TimeSpanFormatOptions.RangeMilliSeconds:
                        value = (int) Math.Floor(FromTime.TotalMilliseconds);
                        FromTime -= TimeSpan.FromMilliseconds(value);
                        break;
                    default:
                        // This code is unreachable, but it prevents compile-errors.
                        throw new ArgumentException("TimeSpanUtility");
                }


                //Determine whether to display this value
                var displayThisValue = false;

                switch (truncate)
                {
                    case TimeSpanFormatOptions.TruncateShortest:
                        if (textStarted) continue; // continue with next for
                        if (value > 0) displayThisValue = true;
                        break;
                    case TimeSpanFormatOptions.TruncateAuto:
                        if (value > 0) displayThisValue = true;
                        break;
                    case TimeSpanFormatOptions.TruncateFill:
                        if (textStarted || value > 0) displayThisValue = true;
                        break;
                    case TimeSpanFormatOptions.TruncateFull:
                        displayThisValue = true;
                        break;
                }

                // we need to display SOMETHING (even if it's zero)
                if (i == rangeMin && textStarted == false)
                {
                    displayThisValue = true;
                    if (lessThan && value < 1)
                    {
                        // Output the "less than 1 unit" text:
                        var unitTitle = timeTextInfo.GetUnitText(rangeMin, 1, abbreviate);
                        result.Append(timeTextInfo.GetLessThanText(unitTitle));
                        displayThisValue = false;
                    }
                }

                // Output the value:
                if (displayThisValue)
                {
                    if (textStarted) result.Append(' ');
                    var unitTitle = timeTextInfo.GetUnitText(i, value, abbreviate);
                    result.Append(unitTitle);
                    textStarted = true;
                }
            }

            return result.ToString();
        }

        #endregion

        #region: DefaultFormatOptions :

        static TimeSpanUtility()
        {
            // Create our defaults:
            DefaultFormatOptions =
                TimeSpanFormatOptions.AbbreviateOff
                | TimeSpanFormatOptions.LessThan
                | TimeSpanFormatOptions.TruncateAuto
                | TimeSpanFormatOptions.RangeSeconds
                | TimeSpanFormatOptions.RangeDays;
            AbsoluteDefaults = DefaultFormatOptions;
        }

        /// <summary>
        /// These are the default options that will be used when no option is specified.
        /// </summary>
        public static TimeSpanFormatOptions DefaultFormatOptions { get; set; }

        /// <summary>
        /// These are the absolute default options that will be used as
        /// a safeguard, just in case DefaultFormatOptions is missing a value.
        /// </summary>
        public static TimeSpanFormatOptions AbsoluteDefaults { get; }

        #endregion

        #region: TimeSpan Rounding :

        /// <summary>
        /// <para>Returns the <see cref="TimeSpan"/> closest to the specified interval.</para>
        /// <para>For example: <c>Round("00:57:00", TimeSpan.TicksPerMinute * 5) =&gt; "00:55:00"</c></para>
        /// </summary>
        /// <param name="fromTime">A <see cref="TimeSpan"/> to be rounded.</param>
        /// <param name="intervalTicks">Specifies the interval for rounding. Use <c>TimeSpan.TicksPer...</c> constants.</param>
        public static TimeSpan Round(this TimeSpan fromTime, long intervalTicks)
        {
            var extra = fromTime.Ticks % intervalTicks;
            if (extra >= intervalTicks >> 1) extra -= intervalTicks;
            return TimeSpan.FromTicks(fromTime.Ticks - extra);
        }

        #endregion
    }
}