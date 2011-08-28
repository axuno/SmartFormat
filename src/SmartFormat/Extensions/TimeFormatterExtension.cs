using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    public class TimeFormatterExtension : IFormatter
    {

        #region IFormatter

        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            if (format != null && format.HasNested) return;

            var formatText = format != null ? format.Text : "";
            if (current is TimeSpan)
            {
                var formattingOptions = TimeFormatOptionsConverter.Parse(formatText);
                output.Write(TimeFormatter.ToTimeString((TimeSpan)current, formattingOptions), formatDetails);
                handled = true;
            }
            else if (current is DateTime && formatText.StartsWith("timestring"))
            {
                formatText = formatText.Substring(10);
                var formattingOptions = TimeFormatOptionsConverter.Parse(formatText);
                output.Write(TimeFormatter.ToTimeString(DateTime.Now.Subtract((DateTime)current), formattingOptions), formatDetails);
                handled = true;
            }
        }

        #endregion

        #region Constructors

        public TimeFormatterExtension()
        {
            this.DefaultFormatOptions = TimeFormatter.DefaultFormatOptions;
        }

        #endregion

        #region FormattingOptions 

        public TimeFormatOptions DefaultFormatOptions { get; set; }

        #endregion

    }

    public static class TimeFormatter
    {
        #region: DefaultFormatOptions :

        static TimeFormatter()
        {
            // Create our defaults:
            DefaultFormatOptions = 
                 TimeFormatOptions.AbbreviateOff
                |TimeFormatOptions.LessThan
                |TimeFormatOptions.TruncateAuto
                |TimeFormatOptions.RangeSeconds
                |TimeFormatOptions.RangeDays;
            AbsoluteDefaults = DefaultFormatOptions;
        }

        /// <summary> 
        /// These are the default options that will be used when no option is specified.
        /// </summary>
        public static TimeFormatOptions DefaultFormatOptions { get; set; }
        /// <summary> 
        /// These are the absolute default options that will be used as
        /// a safeguard, just in case DefaultFormatOptions is missing a value. 
        /// </summary>
        public static TimeFormatOptions AbsoluteDefaults { get; private set; }

        #endregion

        #region: ToTimeString :

        /// <summary> 
        ///   <para>Turns a TimeSpan into a human-readable text.</para>
        ///   <para>Uses the default TimeFormatOptions.</para>
        ///   <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
        /// </summary>
        public static string ToTimeString(this TimeSpan FromTime)
        {
            return ToTimeString(FromTime, DefaultFormatOptions);
        }
        /// <summary>
        ///   <para>Turns a TimeSpan into a human-readable text.</para>
        ///   <para>Parses the specified TimeFormatOptions.</para>
        ///   <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
        /// </summary>
        /// <param name="FromTime"></param>
        /// <param name="options">
        ///   <para>A combination of flags that determine the formatting options.</para>
        ///   <para>These will be combined with the default TimeFormatOptions.</para>
        ///   <para>Syntax:</para>
        ///   <para><c>Range</c>: &#160; <c>w &#160; d &#160; h &#160; m &#160; s &#160; ms</c></para>
        ///   <para><c>Truncate</c>: &#160; <c>short &#160; auto &#160; fill &#160; full</c></para>
        ///   <para><c>Abbreviate</c>: &#160; <c>abbr &#160; noabbr</c></para>
        ///   <para><c>LessThan</c>: &#160; <c>less &#160; noless</c></para>
        /// </param>
        /// <remarks> The format options are case insensitive. </remarks>
        public static string ToTimeString(this TimeSpan FromTime, string options)
        {
            return ToTimeString(FromTime, TimeFormatOptionsConverter.Parse(options));
        }
        /// <summary> 
        ///   <para>Turns a TimeSpan into a human-readable text.</para>
        ///   <para>Uses the specified TimeFormatOptions.</para>
        ///   <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
		/// </summary>
		/// <param name="FromTime"></param>
		/// <param name="options">
		///   <para>A combination of flags that determine the formatting options.</para>
        ///   <para>These will be combined with the default TimeFormatOptions.</para>
		/// </param>
		public static string ToTimeString(this TimeSpan FromTime, TimeFormatOptions options)
		{
            // If there are any missing options, merge with the defaults:
            // Also, as a safeguard against missing DefaultFormatOptions, let's also merge with the AbsoluteDefaults:
            options = options.Merge(DefaultFormatOptions).Merge(AbsoluteDefaults);
            // Extract the individual options:
            var rangeMax = options.Mask(TimeFormatOptions._Range).AllFlags().Last();
            var rangeMin = options.Mask(TimeFormatOptions._Range).AllFlags().First();
            var truncate = options.Mask(TimeFormatOptions._Truncate).AllFlags().First();
            var lessThan = options.Mask(TimeFormatOptions._LessThan) != TimeFormatOptions.LessThanOff;
            var abbreviate = options.Mask(TimeFormatOptions._Abbreviate) != TimeFormatOptions.AbbreviateOff;

            Func<double, double> round = Math.Ceiling;
            if (lessThan) round = Math.Floor;
			switch (rangeMin) {
				case TimeFormatOptions.RangeWeeks:
				    FromTime = TimeSpan.FromDays(round(FromTime.TotalDays / 7) * 7);
					break;
                case TimeFormatOptions.RangeDays:
					FromTime = TimeSpan.FromDays(round(FromTime.TotalDays));
					break;
                case TimeFormatOptions.RangeHours:
					FromTime = TimeSpan.FromHours(round(FromTime.TotalHours));
					break;
                case TimeFormatOptions.RangeMinutes:
					FromTime = TimeSpan.FromMinutes(round(FromTime.TotalMinutes));
					break;
                case TimeFormatOptions.RangeSeconds:
					FromTime = TimeSpan.FromSeconds(round(FromTime.TotalSeconds));
					break;
                case TimeFormatOptions.RangeMilliSeconds:
					FromTime = TimeSpan.FromMilliseconds(round(FromTime.TotalMilliseconds));
					break;
			}

            // Create our result:
            bool textStarted = false;
            var result = new StringBuilder();
            for (var i = rangeMax; i >= rangeMin; i=(TimeFormatOptions)((int)i>>1)) {
                // Determine the value and title:
			    int value = 0;
                string timeTitle = "";
				switch (i) {
					case TimeFormatOptions.RangeWeeks:
						value = (int)Math.Floor(FromTime.TotalDays / 7);
						timeTitle = (abbreviate ? "w" : " week");
						FromTime -= TimeSpan.FromDays(value * 7);
						break;
                    case TimeFormatOptions.RangeDays:
                        value = (int)Math.Floor(FromTime.TotalDays);
						timeTitle = (abbreviate ? "d" : " day");
						FromTime -= TimeSpan.FromDays(value);
						break;
                    case TimeFormatOptions.RangeHours:
                        value = (int)Math.Floor(FromTime.TotalHours);
						timeTitle = (abbreviate ? "h" : " hour");
						FromTime -= TimeSpan.FromHours(value);
						break;
                    case TimeFormatOptions.RangeMinutes:
                        value = (int)Math.Floor(FromTime.TotalMinutes);
						timeTitle = (abbreviate ? "m" : " minute");
						FromTime -= TimeSpan.FromMinutes(value);
						break;
                    case TimeFormatOptions.RangeSeconds:
                        value = (int)Math.Floor(FromTime.TotalSeconds);
						timeTitle = (abbreviate ? "s" : " second");
						FromTime -= TimeSpan.FromSeconds(value);
						break;
                    case TimeFormatOptions.RangeMilliSeconds:
                        value = (int)Math.Floor(FromTime.TotalMilliseconds);
						timeTitle = (abbreviate ? "ms" : " millisecond");
						FromTime -= TimeSpan.FromMilliseconds(value);
						break;
				}


				//Determine whether to display this value
				bool displayThisValue = false;
                bool breakFor = false; // I wish C# supported "break for;" (like how VB supports "Exit For")
                switch (truncate)
                {
                    case TimeFormatOptions.TruncateShortest:
                        if (textStarted)
                        {
                            breakFor = true; 
                            break;
                        }
                        if (value > 0) displayThisValue = true;
                        break;
                    case TimeFormatOptions.TruncateAuto:
						if (value > 0) displayThisValue = true;
						break;
                    case TimeFormatOptions.TruncateFill:
						if (textStarted || value > 0) displayThisValue = true;
						break;
                    case TimeFormatOptions.TruncateFull:
						displayThisValue = true;
						break;
				}
                if (breakFor) break;

				//we need to display SOMETHING (even if it's zero)
                if (i == rangeMin && textStarted == false) {
					displayThisValue = true;
					if (lessThan && value < 1) {
					    result.Append("less than ");
                        value = 1;
					}
				}

                // Output the value:
				if (displayThisValue) {
                    if (textStarted) result.Append(" ");
				    result.Append(value).Append(timeTitle);
                    if (value != 1 && !abbreviate) result.Append("s");
					textStarted = true;
				}
			}

            return result.ToString();
        }

        #endregion

        #region: TimeSpan Rounding :

        /// <summary>
        ///   <para>Returns the largest <c>TimeSpan</c> less than or equal to the specified interval.</para>
        ///   <para>For example: <c>Floor("00:57:00", TimeSpan.TicksPerMinute * 5) =&gt; "00:55:00"</c></para>
        /// </summary>
        /// <param name="FromTime">A <c>TimeSpan</c> to be rounded.</param>
        /// <param name="intervalTicks">Specifies the interval for rounding.  Use <c>TimeSpan.TicksPer____</c>.</param>
        public static TimeSpan Floor(this TimeSpan FromTime, long intervalTicks)
        {
            var extra = FromTime.Ticks % intervalTicks;
            return TimeSpan.FromTicks(FromTime.Ticks - extra);
        }
        /// <summary>
        ///   <para>Returns the smallest <c>TimeSpan</c> greater than or equal to the specified interval.</para>
        ///   <para>For example: <c>Ceiling("00:57:00", TimeSpan.TicksPerMinute * 5) =&gt; "01:00:00"</c></para>
        /// </summary>
        /// <param name="FromTime">A <c>TimeSpan</c> to be rounded.</param>
        /// <param name="intervalTicks">Specifies the interval for rounding.  Use <c>TimeSpan.TicksPer____</c>.</param>
        public static TimeSpan Ceiling(this TimeSpan FromTime, long intervalTicks)
        {
            var extra = FromTime.Ticks % intervalTicks;
            if (extra == 0) return FromTime;
            return TimeSpan.FromTicks(FromTime.Ticks - extra + intervalTicks);
        }
        /// <summary>
        ///   <para>Returns the <c>TimeSpan</c> closest to the specified interval.</para>
        ///   <para>For example: <c>Round("00:57:00", TimeSpan.TicksPerMinute * 5) =&gt; "00:55:00"</c></para>
        /// </summary>
        /// <param name="FromTime">A <c>TimeSpan</c> to be rounded.</param>
        /// <param name="intervalTicks">Specifies the interval for rounding.  Use <c>TimeSpan.TicksPer____</c>.</param>
        public static TimeSpan Round(this TimeSpan FromTime, long intervalTicks)
        {
            var extra = FromTime.Ticks % intervalTicks;
            if (extra >= (intervalTicks >> 1)) extra -= intervalTicks;
            return TimeSpan.FromTicks(FromTime.Ticks - extra);
        }

        #endregion

    }

    #region: TimeFormatOptions :

    /// <summary>
    ///   <para>Determines all options for time formatting.</para>
    ///   <para>This one value actually contains 4 settings:</para>
    ///   <para><c>Abbreviate</c> / <c>AbbreviateOff</c></para>
    ///   <para><c>LessThan</c> / <c>LessThanOff</c></para>
    ///   <para><c>Truncate</c> &#160; <c>Auto</c> / <c>Shortest</c> / <c>Fill</c> / <c>Full</c></para>
    ///   <para><c>Range</c> &#160; <c>MilliSeconds</c> / <c>Seconds</c> / <c>Minutes</c> / <c>Hours</c> / <c>Days</c> / <c>Weeks</c> (Min / Max) </para>
    /// </summary>
    [Flags]
    public enum TimeFormatOptions
    {
        /// <summary>
        /// Specifies that all <c>TimeFormatOptions</c> should be inherited from <c>TimeFormatter.DefaultTimeFormatOptions</c>.
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
        ///   <para>Displays the highest non-zero value within the range.</para>
        ///   <para>Example: "00.23:00:59.000" = "23 hours"</para>
        /// </summary>
        TruncateShortest = 0x10,
        /// <summary>
        ///   <para>Displays all non-zero values within the range.</para>
        ///   <para>Example: "00.23:00:59.000" = "23 hours 59 minutes"</para>
        /// </summary>
        TruncateAuto = 0x20,
        /// <summary>
        ///   <para>Displays the highest non-zero value and all lesser values within the range.</para>
        ///   <para>Example: "00.23:00:59.000" = "23 hours 0 minutes 59 seconds 0 milliseconds"</para>
        /// </summary>
        TruncateFill = 0x40,
        /// <summary>
        ///   <para>Displays all values within the range.</para>
        ///   <para>Example: "00.23:00:59.000" = "0 days 23 hours 0 minutes 59 seconds 0 milliseconds"</para>
        /// </summary>
        TruncateFull = 0x80,
        
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeMilliSeconds = 0x100,
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeSeconds = 0x200,
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeMinutes = 0x400,
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeHours = 0x800,
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeDays = 0x1000,
        /// <summary>
        ///   <para>Determines the range of units to display.</para>
        ///   <para>You may combine two values to form the minimum and maximum for the range.</para>
        ///   <para>Example: (RangeMinutes) defines a range of Minutes only; (RangeHours | RangeSeconds) defines a range of Hours to Seconds.</para>
        /// </summary>
        RangeWeeks = 0x2000,

        /// <summary>(for internal use only)</summary>
        _Abbreviate = Abbreviate | AbbreviateOff,
        /// <summary>(for internal use only)</summary>
        _LessThan = LessThan | LessThanOff,
        /// <summary>(for internal use only)</summary>
        _Truncate = TruncateShortest | TruncateAuto | TruncateFill | TruncateFull,
        /// <summary>(for internal use only)</summary>
        _Range = RangeMilliSeconds | RangeSeconds | RangeMinutes | RangeHours | RangeDays | RangeWeeks,
    }
    internal static class TimeFormatOptionsConverter
    {
        public static TimeFormatOptions Merge(this TimeFormatOptions left, TimeFormatOptions right)
        {
            var masks = new[]{
                    TimeFormatOptions._Abbreviate, 
                    TimeFormatOptions._LessThan, 
                    TimeFormatOptions._Range, 
                    TimeFormatOptions._Truncate,
                };
            foreach (var mask in masks)
            {
                if ((left & mask) == 0) left |= right & mask;
            }
            return left;
        }
        public static TimeFormatOptions Mask(this TimeFormatOptions timeFormatOptions, TimeFormatOptions mask)
        {
            return timeFormatOptions & mask;
        }
        public static IEnumerable<TimeFormatOptions> AllFlags(this TimeFormatOptions timeFormatOptions)
        {
            uint value = 0x1;
            while (value <= (uint)timeFormatOptions)
            {
                if ((value & (uint)timeFormatOptions) != 0)
                {
                    yield return (TimeFormatOptions) value;
                }
                value <<= 1;
            }
        }
        
        private static readonly Regex parser = new Regex(@"\b(w|week|weeks|d|day|days|h|hour|hours|m|minute|minutes|s|second|seconds|ms|millisecond|milliseconds|auto|short|fill|full|abbr|noabbr|less|noless)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static TimeFormatOptions Parse(string formatOptionsString)
        {
            formatOptionsString = formatOptionsString.ToLower();

            var t = TimeFormatOptions.InheritDefaults;
            foreach (Match m in parser.Matches(formatOptionsString))
            {
                switch (m.Value)
                {
                    case "w":
                    case "week":
                    case "weeks":
                        t |= TimeFormatOptions.RangeWeeks;
                        break;
                    case "d":
                    case "day":
                    case "days":
                        t |= TimeFormatOptions.RangeDays;
                        break;
                    case "h":
                    case "hour":
                    case "hours":
                        t |= TimeFormatOptions.RangeHours;
                        break;
                    case "m":
                    case "minute":
                    case "minutes":
                        t |= TimeFormatOptions.RangeMinutes;
                        break;
                    case "s":
                    case "second":
                    case "seconds":
                        t |= TimeFormatOptions.RangeSeconds;
                        break;
                    case "ms":
                    case "millisecond":
                    case "milliseconds":
                        t |= TimeFormatOptions.RangeMilliSeconds;
                        break;


                    case "short":
                        t |= TimeFormatOptions.TruncateShortest;
                        break;
                    case "auto":
                        t |= TimeFormatOptions.TruncateAuto;
                        break;
                    case "fill":
                        t |= TimeFormatOptions.TruncateFill;
                        break;
                    case "full":
                        t |= TimeFormatOptions.TruncateFull;
                        break;


                    case "abbr":
                        t |= TimeFormatOptions.Abbreviate;
                        break;
                    case "noabbr":
                        t |= TimeFormatOptions.AbbreviateOff;
                        break;


                    case "less":
                        t |= TimeFormatOptions.LessThan;
                        break;
                    case "noless":
                        t |= TimeFormatOptions.LessThanOff;
                        break;
                }

            }

            return t;
        }
    }

    #endregion
}
