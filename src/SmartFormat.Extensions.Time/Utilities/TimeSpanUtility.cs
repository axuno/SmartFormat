// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SmartFormat.Extensions.Time.Utilities;

/// <summary>
/// Utility class to format a <see cref="TimeSpan"/> as a <see langword="string"/>.
/// </summary>
public static class TimeSpanUtility
{
    private static TimeSpanFormatOptions _rangeMin;
    private static TimeSpanFormatOptions _truncate;
    private static bool _lessThan;
    private static bool _abbreviate;
    private static Func<double, double>? _round;
    private static TimeTextInfo? _timeTextInfo;

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

    #region: ToTimeString :

    /// <summary>
    /// <para>Turns a TimeSpan into a human-readable text.</para>
    /// <para>Uses the specified timeSpanFormatOptions.</para>
    /// <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
    /// </summary>
    /// <param name="fromTime"></param>
    /// <param name="options">
    /// <para>A combination of flags that determine the formatting options.</para>
    /// <para>These will be combined with the default timeSpanFormatOptions.</para>
    /// </param>
    /// <param name="timeTextInfo">An object that supplies the text to use for output</param>
    public static string ToTimeString(this TimeSpan fromTime, TimeSpanFormatOptions options,
        TimeTextInfo timeTextInfo)
    {
        return string.Join(" ", fromTime.ToTimeParts(options, timeTextInfo));
    }

    /// <summary>
    /// <para>Turns a TimeSpan into a list of human-readable text parts.</para>
    /// <para>Uses the specified timeSpanFormatOptions.</para>
    /// <para>For example: "31.23:59:00.555" = "31 days 23 hours 59 minutes 0 seconds 555 milliseconds"</para>
    /// </summary>
    /// <param name="fromTime"></param>
    /// <param name="options">
    /// <para>A combination of flags that determine the formatting options.</para>
    /// <para>These will be combined with the default timeSpanFormatOptions.</para>
    /// </param>
    /// <param name="timeTextInfo">An object that supplies the text to use for output</param>
    internal static IList<string> ToTimeParts(this TimeSpan fromTime, TimeSpanFormatOptions options,
        TimeTextInfo timeTextInfo)
    {
        // If there are any missing options, merge with the defaults:
        // Also, as a safeguard against missing DefaultFormatOptions, let's also merge with the AbsoluteDefaults:
        options = options.Merge(DefaultFormatOptions).Merge(AbsoluteDefaults);

        // Extract the individual options:
        var rangeMax = options.Mask(TimeSpanFormatOptionsPresets.Range).AllFlags().Last();
        _rangeMin = options.Mask(TimeSpanFormatOptionsPresets.Range).AllFlags().First();
        _truncate = options.Mask(TimeSpanFormatOptionsPresets.Truncate).AllFlags().First();
        _lessThan = options.Mask(TimeSpanFormatOptionsPresets.LessThan) != TimeSpanFormatOptions.LessThanOff;
        _abbreviate = options.Mask(TimeSpanFormatOptionsPresets.Abbreviate) != TimeSpanFormatOptions.AbbreviateOff;
        _round = _lessThan ? (Func<double, double>) Math.Floor : Math.Ceiling;
        _timeTextInfo = timeTextInfo;

        switch (_rangeMin)
        {
            case TimeSpanFormatOptions.RangeWeeks:
                fromTime = TimeSpan.FromDays(_round(fromTime.TotalDays / 7) * 7);
                break;
            case TimeSpanFormatOptions.RangeDays:
                fromTime = TimeSpan.FromDays(_round(fromTime.TotalDays));
                break;
            case TimeSpanFormatOptions.RangeHours:
                fromTime = TimeSpan.FromHours(_round(fromTime.TotalHours));
                break;
            case TimeSpanFormatOptions.RangeMinutes:
                fromTime = TimeSpan.FromMinutes(_round(fromTime.TotalMinutes));
                break;
            case TimeSpanFormatOptions.RangeSeconds:
                fromTime = TimeSpan.FromSeconds(_round(fromTime.TotalSeconds));
                break;
            case TimeSpanFormatOptions.RangeMilliSeconds:
                fromTime = TimeSpan.FromMilliseconds(_round(fromTime.TotalMilliseconds));
                break;
        }

        // Create our result:
        var result = new List<string>();
        for (var i = rangeMax; i >= _rangeMin; i = (TimeSpanFormatOptions) ((int) i >> 1))
        {
            // Determine the value and title:
            int value;
            switch (i)
            {
                case TimeSpanFormatOptions.RangeWeeks:
                    value = (int) Math.Floor(fromTime.TotalDays / 7);
                    fromTime -= TimeSpan.FromDays(value * 7);
                    break;
                case TimeSpanFormatOptions.RangeDays:
                    value = (int) Math.Floor(fromTime.TotalDays);
                    fromTime -= TimeSpan.FromDays(value);
                    break;
                case TimeSpanFormatOptions.RangeHours:
                    value = (int) Math.Floor(fromTime.TotalHours);
                    fromTime -= TimeSpan.FromHours(value);
                    break;
                case TimeSpanFormatOptions.RangeMinutes:
                    value = (int) Math.Floor(fromTime.TotalMinutes);
                    fromTime -= TimeSpan.FromMinutes(value);
                    break;
                case TimeSpanFormatOptions.RangeSeconds:
                    value = (int) Math.Floor(fromTime.TotalSeconds);
                    fromTime -= TimeSpan.FromSeconds(value);
                    break;
                case TimeSpanFormatOptions.RangeMilliSeconds:
                    value = (int) Math.Floor(fromTime.TotalMilliseconds);
                    fromTime -= TimeSpan.FromMilliseconds(value);
                    break;
                default:
                    // Should never happen. Ensures 'value' and 'fromTime' are always set.
                    continue;
            }

            //Determine whether to display this value
            if (!ShouldTruncate(value, result.Any(), out var displayThisValue)) continue;

            PrepareOutput(value, i == _rangeMin, result.Any(), result, ref displayThisValue);

            // Output the value:
            if (displayThisValue)
            {
                var unitTitle = _timeTextInfo.GetUnitText(i, value, _abbreviate);
                if (!string.IsNullOrEmpty(unitTitle)) result.Add(unitTitle);
            }
        }

        return result;
    }

    private static bool ShouldTruncate(int value, bool textStarted, out bool displayThisValue)
    {
        displayThisValue = false;
        switch (_truncate)
        {
            case TimeSpanFormatOptions.TruncateShortest:
                if (textStarted) return false; // continue with next for
                if (value > 0) displayThisValue = true;
                return true;
            case TimeSpanFormatOptions.TruncateAuto:
                if (value > 0) displayThisValue = true;
                return true;
            case TimeSpanFormatOptions.TruncateFill:
                if (textStarted || value > 0) displayThisValue = true;
                return true;
            case TimeSpanFormatOptions.TruncateFull:
                displayThisValue = true;
                return true;
        }

        // Should never happen
        return false;
    }

    private static void PrepareOutput(int value, bool isRangeMin, bool hasTextStarted, List<string> result, ref bool displayThisValue)
    {
        // we need to display SOMETHING (even if it's zero)
        if (isRangeMin && !hasTextStarted)
        {
            displayThisValue = true;
            if (_lessThan && value < 1)
            {
                // Output the "less than 1 unit" text:
                var unitTitle = _timeTextInfo!.GetUnitText(_rangeMin, 1, _abbreviate);
                result.Add(_timeTextInfo.GetLessThanText(unitTitle));
                displayThisValue = false;
            }
        }
    }

    #endregion

    #region: DefaultFormatOptions :

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
