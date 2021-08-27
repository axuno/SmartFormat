//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents the literal text that is found
    /// in a parsed format string.
    /// </summary>
    public class LiteralText : FormatItem
    {
        private string? _toStringCache;

        /// <summary>
        /// Creates an instance of <see cref="LiteralText"/>, representing the literal text that is found a parsed format string.
        /// </summary>
        /// <param name="smartSettings">The <see cref="SmartSettings"/>.</param>
        /// <param name="baseString">The reference to the parsed format string.</param>
        /// <param name="startIndex">The start index of the <see cref="LiteralText"/> in the format string.</param>
        /// <param name="endIndex">The end index of the <see cref="LiteralText"/> in the format string.</param>
        public LiteralText(SmartSettings smartSettings, string baseString, int startIndex, int endIndex) : base(smartSettings, baseString, startIndex, endIndex)
        {
        }

        /// <summary>
        /// Get the string representation of the <see cref="LiteralText"/>, with escaped characters converted.
        /// Note: The <see cref="Parser"/> puts each escaped character of an input string
        /// into its own <see cref="LiteralText"/> item.
        /// </summary>
        /// <returns>The string representation of the <see cref="LiteralText"/>, with escaped characters converted.</returns>
        public override string ToString()
        {
            if (_toStringCache != null) return _toStringCache;
            if (Length == 0) _toStringCache = string.Empty;

            // The buffer is only 1 character
            _toStringCache = AsSpan().ToString();

            return _toStringCache;
        }

        /// <summary>
        /// Get the string representation of the <see cref="LiteralText"/>, with escaped characters converted.
        /// Note: The <see cref="Parser"/> puts each escaped character of an input string
        /// into its own <see cref="LiteralText"/> item.
        /// </summary>
        /// <returns>The string representation of the <see cref="LiteralText"/>, with escaped characters converted.</returns>
        public override ReadOnlySpan<char> AsSpan()
        {
            if (Length == 0) return ReadOnlySpan<char>.Empty;

            // The buffer is only for 1 character - each escaped char goes into its own LiteralText
            return SmartSettings.Parser.ConvertCharacterStringLiterals &&
                             BaseString.AsSpan(StartIndex)[0] == SmartSettings.Parser.CharLiteralEscapeChar
                ? EscapedLiteral.UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar,
                    BaseString.AsSpan(StartIndex, Length),
                    false, new char[1])
                : BaseString.AsSpan(StartIndex, Length);
        }

    }
}