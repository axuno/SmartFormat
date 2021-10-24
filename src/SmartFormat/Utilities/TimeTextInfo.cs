// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
// 

using System;
using System.Collections.Generic;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Supplies the localized text used for <see cref="TimeSpan"/> formatting.
    /// </summary>
    public class TimeTextInfo
    {
        private readonly string[] _d;
        private readonly string[] _day;
        private readonly string[] _h;
        private readonly string[] _hour;
        private readonly string _lessThan;
        private readonly string[] _m;
        private readonly string[] _millisecond;
        private readonly string[] _minute;
        private readonly string[] _ms;
        private readonly PluralRules.PluralRuleDelegate _pluralRule;
        private readonly string[] _s;
        private readonly string[] _second;
        private readonly string[] _w;
        private readonly string[] _week;

        /// <summary>
        /// Creates a new instance of type <see cref="TimeTextInfo"/>.
        /// </summary>
        /// <param name="pluralRule"></param>
        /// <param name="week"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <param name="w"></param>
        /// <param name="d"></param>
        /// <param name="h"></param>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <param name="ms"></param>
        /// <param name="lessThan"></param>
        public TimeTextInfo(PluralRules.PluralRuleDelegate pluralRule, string[] week, string[] day, string[] hour,
            string[] minute, string[] second, string[] millisecond, string[] w, string[] d, string[] h, string[] m,
            string[] s, string[] ms, string lessThan)
        {
            _pluralRule = pluralRule;

            _week = week;
            _day = day;
            _hour = hour;
            _minute = minute;
            _second = second;
            _millisecond = millisecond;

            _w = w;
            _d = d;
            _h = h;
            _m = m;
            _s = s;
            _ms = ms;

            _lessThan = lessThan;
        }

        /// <summary>
        /// Creates a new instance of type <see cref="TimeTextInfo"/>.
        /// </summary>
        /// <param name="week"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <param name="lessThan"></param>
        public TimeTextInfo(string week, string day, string hour, string minute, string second, string millisecond,
            string lessThan)
        {
            // must not be null here
            _d = _h = _m = _ms = _s = _w = Array.Empty<string>(); 

            // Always use singular:
            _pluralRule = (d, c) => 0;
            _week = new[] {week};
            _day = new[] {day};
            _hour = new[] {hour};
            _minute = new[] {minute};
            _second = new[] {second};
            _millisecond = new[] {millisecond};
            _lessThan = lessThan;
        }

        private static string GetValue(PluralRules.PluralRuleDelegate pluralRule, int value, IReadOnlyList<string> units)
        {
            // Get the plural index from the plural rule,
            // unless there's only 1 unit in the first place:
            var pluralIndex = units.Count == 1 ? 0 : pluralRule(value, units.Count);
            return string.Format(units[pluralIndex], value);
        }

        /// <summary>
        /// Gets the "less than" text for the given threshold.
        /// </summary>
        /// <param name="minimumValue"></param>
        /// <returns>The "less than" text for the given threshold.</returns>
        public virtual string GetLessThanText(string minimumValue)
        {
            return string.Format(_lessThan, minimumValue);
        }

        /// <summary>
        /// Gets the text for <see cref="TimeSpanFormatOptions"/> ranges,
        /// that correspond to a certain value.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="value"></param>
        /// <param name="abbr"></param>
        /// <returns>
        /// The text for <see cref="TimeSpanFormatOptions"/> ranges,
        /// that correspond to a certain value.
        /// </returns>
        public virtual string GetUnitText(TimeSpanFormatOptions unit, int value, bool abbr)
        {
            return unit switch
            {
                TimeSpanFormatOptions.RangeWeeks => GetValue(_pluralRule, value, abbr ? _w : _week),
                TimeSpanFormatOptions.RangeDays => GetValue(_pluralRule, value, abbr ? _d : _day),
                TimeSpanFormatOptions.RangeHours => GetValue(_pluralRule, value, abbr ? _h : _hour),
                TimeSpanFormatOptions.RangeMinutes => GetValue(_pluralRule, value, abbr ? _m : _minute),
                TimeSpanFormatOptions.RangeSeconds => GetValue(_pluralRule, value, abbr ? _s : _second),
                TimeSpanFormatOptions.RangeMilliSeconds => GetValue(_pluralRule, value, abbr ? _ms : _millisecond),
                // (should be unreachable)
                _ => string.Empty
            };
        }
    }
}