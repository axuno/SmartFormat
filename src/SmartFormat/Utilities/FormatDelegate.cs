//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// This class wraps a delegate, allowing it to be used as a parameter
    /// to any string-formatting method (such as <see cref="string.Format(string, object)" />).
    /// For example:
    /// <code>
    /// var textWithLink = string.Format("Please click on {0:this link}.", new FormatDelegate((text) => Html.ActionLink(text, "SomeAction"));
    /// </code>
    /// </summary>
    public class FormatDelegate : IFormattable
    {
        private readonly Func<string?, string>? _getFormat1;
        private readonly Func<string?, IFormatProvider?, string>? _getFormat2;

        /// <summary>
        /// Creates a new instance of a <see cref="FormatDelegate"/>.
        /// </summary>
        /// <param name="getFormat"></param>
        public FormatDelegate(Func<string?, string> getFormat)
        {
            _getFormat1 = getFormat;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="FormatDelegate"/>.
        /// </summary>
        /// <param name="getFormat"></param>
        public FormatDelegate(Func<string?, IFormatProvider?, string> getFormat)
        {
            _getFormat2 = getFormat;
        }

        /// <summary>
        /// Implements <see cref="IFormattable"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            if (_getFormat1 != null) return _getFormat1(format);
            if (_getFormat2 != null) return _getFormat2(format, formatProvider);
            return string.Empty;
        }
    }
}