using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartFormat.Utilities
{
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
                var breakFor =
                    false; // I wish C# supported "break for;" (like how VB supports "Exit For" from within a "Select Case" statement)
                switch (truncate)
                {
                    case TimeSpanFormatOptions.TruncateShortest:
                        if (textStarted)
                        {
                            breakFor = true;
                            break;
                        }

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

                if (breakFor) break;

                //we need to display SOMETHING (even if it's zero)
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
                    if (textStarted) result.Append(" ");
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
        /// <para>Returns the <c>TimeSpan</c> closest to the specified interval.</para>
        /// <para>For example: <c>Round("00:57:00", TimeSpan.TicksPerMinute * 5) =&gt; "00:55:00"</c></para>
        /// </summary>
        /// <param name="FromTime">A <c>TimeSpan</c> to be rounded.</param>
        /// <param name="intervalTicks">Specifies the interval for rounding.  Use <c>TimeSpan.TicksPer____</c>.</param>
        public static TimeSpan Round(this TimeSpan FromTime, long intervalTicks)
        {
            var extra = FromTime.Ticks % intervalTicks;
            if (extra >= intervalTicks >> 1) extra -= intervalTicks;
            return TimeSpan.FromTicks(FromTime.Ticks - extra);
        }

        #endregion
    }

    #region: TimeSpanFormatOptions :

    /// <summary>
    /// <para>Determines all options for time formatting.</para>
    /// <para>This one value actually contains 4 settings:</para>
    /// <para><c>Abbreviate</c> / <c>AbbreviateOff</c></para>
    /// <para><c>LessThan</c> / <c>LessThanOff</c></para>
    /// <para><c>Truncate</c> &#160; <c>Auto</c> / <c>Shortest</c> / <c>Fill</c> / <c>Full</c></para>
    /// <para>
    ///     <c>Range</c> &#160; <c>MilliSeconds</c> / <c>Seconds</c> / <c>Minutes</c> / <c>Hours</c> / <c>Days</c> /
    ///     <c>Weeks</c> (Min / Max)
    /// </para>
    /// </summary>
    [Flags]
    public enum TimeSpanFormatOptions
    {
        /// <summary>
        /// Specifies that all <c>timeSpanFormatOptions</c> should be inherited from
        /// <c>TimeSpanUtility.DefaultTimeFormatOptions</c>.
        /// </summary>
        InheritDefaults = 0x0,

        /// <summary>
        /// Abbreviates units.
        /// Example: "1d 2h 3m 4s 5ms"
        /// </summary>
        Abbreviate = 0x1,

        /// <summary>
        /// Does not abbreviate units.
        /// Example: "1 day 2 hours 3 minutes 4 seconds 5 milliseconds"
        /// </summary>
        AbbreviateOff = 0x2,

        /// <summary>
        /// Displays "less than 1 (unit)" when the TimeSpan is smaller than the minimum range.
        /// </summary>
        LessThan = 0x4,

        /// <summary>
        /// Displays "0 (units)" when the TimeSpan is smaller than the minimum range.
        /// </summary>
        LessThanOff = 0x8,

        /// <summary>
        /// <para>Displays the highest non-zero value within the range.</para>
        /// <para>Example: "00.23:00:59.000" = "23 hours"</para>
        /// </summary>
        TruncateShortest = 0x10,

        /// <summary>
        /// <para>Displays all non-zero values within the range.</para>
        /// <para>Example: "00.23:00:59.000" = "23 hours 59 minutes"</para>
        /// </summary>
        TruncateAuto = 0x20,

        /// <summary>
        /// <para>Displays the highest non-zero value and all lesser values within the range.</para>
        /// <para>Example: "00.23:00:59.000" = "23 hours 0 minutes 59 seconds 0 milliseconds"</para>
        /// </summary>
        TruncateFill = 0x40,

        /// <summary>
        /// <para>Displays all values within the range.</para>
        /// <para>Example: "00.23:00:59.000" = "0 days 23 hours 0 minutes 59 seconds 0 milliseconds"</para>
        /// </summary>
        TruncateFull = 0x80,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeMilliSeconds = 0x100,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeSeconds = 0x200,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeMinutes = 0x400,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeHours = 0x800,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeDays = 0x1000,

        /// <summary>
        /// <para>Determines the range of units to display.</para>
        /// <para>You may combine two values to form the minimum and maximum for the range.</para>
        /// <para>
        ///     Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours
        ///     to Seconds.
        /// </para>
        /// </summary>
        RangeWeeks = 0x2000,

        /// <summary>(for internal use only)</summary>
        _Abbreviate = Abbreviate | AbbreviateOff,

        /// <summary>(for internal use only)</summary>
        _LessThan = LessThan | LessThanOff,

        /// <summary>(for internal use only)</summary>
        _Truncate = TruncateShortest | TruncateAuto | TruncateFill | TruncateFull,

        /// <summary>(for internal use only)</summary>
        _Range = RangeMilliSeconds | RangeSeconds | RangeMinutes | RangeHours | RangeDays | RangeWeeks
    }

    internal static class TimeSpanFormatOptionsConverter
    {
        private static readonly Regex parser =
            new Regex(
                @"\b(w|week|weeks|d|day|days|h|hour|hours|m|minute|minutes|s|second|seconds|ms|millisecond|milliseconds|auto|short|fill|full|abbr|noabbr|less|noless)\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static TimeSpanFormatOptions Merge(this TimeSpanFormatOptions left, TimeSpanFormatOptions right)
        {
            var masks = new[]
            {
                TimeSpanFormatOptions._Abbreviate,
                TimeSpanFormatOptions._LessThan,
                TimeSpanFormatOptions._Range,
                TimeSpanFormatOptions._Truncate
            };
            foreach (var mask in masks)
                if ((left & mask) == 0)
                    left |= right & mask;
            return left;
        }

        public static TimeSpanFormatOptions Mask(this TimeSpanFormatOptions timeSpanFormatOptions,
            TimeSpanFormatOptions mask)
        {
            return timeSpanFormatOptions & mask;
        }

        public static IEnumerable<TimeSpanFormatOptions> AllFlags(this TimeSpanFormatOptions timeSpanFormatOptions)
        {
            uint value = 0x1;
            while (value <= (uint) timeSpanFormatOptions)
            {
                if ((value & (uint) timeSpanFormatOptions) != 0) yield return (TimeSpanFormatOptions) value;
                value <<= 1;
            }
        }

        public static TimeSpanFormatOptions Parse(string formatOptionsString)
        {
            formatOptionsString = formatOptionsString.ToLower();

            var t = TimeSpanFormatOptions.InheritDefaults;
            foreach (Match m in parser.Matches(formatOptionsString))
                switch (m.Value)
                {
                    case "w":
                    case "week":
                    case "weeks":
                        t |= TimeSpanFormatOptions.RangeWeeks;
                        break;
                    case "d":
                    case "day":
                    case "days":
                        t |= TimeSpanFormatOptions.RangeDays;
                        break;
                    case "h":
                    case "hour":
                    case "hours":
                        t |= TimeSpanFormatOptions.RangeHours;
                        break;
                    case "m":
                    case "minute":
                    case "minutes":
                        t |= TimeSpanFormatOptions.RangeMinutes;
                        break;
                    case "s":
                    case "second":
                    case "seconds":
                        t |= TimeSpanFormatOptions.RangeSeconds;
                        break;
                    case "ms":
                    case "millisecond":
                    case "milliseconds":
                        t |= TimeSpanFormatOptions.RangeMilliSeconds;
                        break;


                    case "short":
                        t |= TimeSpanFormatOptions.TruncateShortest;
                        break;
                    case "auto":
                        t |= TimeSpanFormatOptions.TruncateAuto;
                        break;
                    case "fill":
                        t |= TimeSpanFormatOptions.TruncateFill;
                        break;
                    case "full":
                        t |= TimeSpanFormatOptions.TruncateFull;
                        break;


                    case "abbr":
                        t |= TimeSpanFormatOptions.Abbreviate;
                        break;
                    case "noabbr":
                        t |= TimeSpanFormatOptions.AbbreviateOff;
                        break;


                    case "less":
                        t |= TimeSpanFormatOptions.LessThan;
                        break;
                    case "noless":
                        t |= TimeSpanFormatOptions.LessThanOff;
                        break;
                }

            return t;
        }
    }

    #endregion

    #region: TimeTextInfo :

    /// <summary>
    /// Supplies the localized text used for TimeSpan formatting.
    /// </summary>
    public class TimeTextInfo
    {
        private readonly string[] d;
        private readonly string[] day;
        private readonly string[] h;
        private readonly string[] hour;
        private readonly string lessThan;
        private readonly string[] m;
        private readonly string[] millisecond;
        private readonly string[] minute;
        private readonly string[] ms;
        private readonly PluralRules.PluralRuleDelegate PluralRule;
        private readonly string[] s;
        private readonly string[] second;
        private readonly string[] w;
        private readonly string[] week;

        public TimeTextInfo(PluralRules.PluralRuleDelegate pluralRule, string[] week, string[] day, string[] hour,
            string[] minute, string[] second, string[] millisecond, string[] w, string[] d, string[] h, string[] m,
            string[] s, string[] ms, string lessThan)
        {
            PluralRule = pluralRule;

            this.week = week;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.millisecond = millisecond;

            this.w = w;
            this.d = d;
            this.h = h;
            this.m = m;
            this.s = s;
            this.ms = ms;

            this.lessThan = lessThan;
        }

        public TimeTextInfo(string week, string day, string hour, string minute, string second, string millisecond,
            string lessThan)
        {
            // Always use singular:
            PluralRule = (d, c) => 0;
            this.week = new[] {week};
            this.day = new[] {day};
            this.hour = new[] {hour};
            this.minute = new[] {minute};
            this.second = new[] {second};
            this.millisecond = new[] {millisecond};
            this.lessThan = lessThan;
        }

        private static string GetValue(PluralRules.PluralRuleDelegate pluralRule, int value, string[] units)
        {
            // Get the plural index from the plural rule,
            // unless there's only 1 unit in the first place:
            var pluralIndex = units.Length == 1 ? 0 : pluralRule(value, units.Length);
            return string.Format(units[pluralIndex], value);
        }

        public string GetLessThanText(string minimumValue)
        {
            return string.Format(lessThan, minimumValue);
        }

        public virtual string GetUnitText(TimeSpanFormatOptions unit, int value, bool abbr)
        {
            switch (unit)
            {
                case TimeSpanFormatOptions.RangeWeeks:
                    return GetValue(PluralRule, value, abbr ? w : week);
                case TimeSpanFormatOptions.RangeDays:
                    return GetValue(PluralRule, value, abbr ? d : day);
                case TimeSpanFormatOptions.RangeHours:
                    return GetValue(PluralRule, value, abbr ? h : hour);
                case TimeSpanFormatOptions.RangeMinutes:
                    return GetValue(PluralRule, value, abbr ? m : minute);
                case TimeSpanFormatOptions.RangeSeconds:
                    return GetValue(PluralRule, value, abbr ? s : second);
                case TimeSpanFormatOptions.RangeMilliSeconds:
                    return GetValue(PluralRule, value, abbr ? ms : millisecond);
            }

            // (should be unreachable)
            return null;
        }
    }

    public static class CommonLanguagesTimeTextInfo
    {
        public static TimeTextInfo English => new TimeTextInfo(
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

        public static TimeTextInfo GetTimeTextInfo(string twoLetterISOLanguageName)
        {
            switch (twoLetterISOLanguageName)
            {
                case "en":
                    return English;
                default:
                    return null;
            }
        }
    }

    #endregion
}