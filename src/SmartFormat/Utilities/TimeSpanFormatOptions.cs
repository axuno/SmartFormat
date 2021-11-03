// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
// 

using System;

namespace SmartFormat.Utilities
{
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
        None = 0x0,

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
}