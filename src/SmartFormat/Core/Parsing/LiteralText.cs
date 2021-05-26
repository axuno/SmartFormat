//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents the literal text that is found
    /// in a parsed format string.
    /// </summary>
    public class LiteralText : FormatItem
    {
        /// <summary>
        /// Creates an instance of <see cref="LiteralText"/>, representing the literal text that is found a parsed format string.
        /// </summary>
        /// <param name="smartSettings">The <see cref="SmartSettings"/>.</param>
        /// <param name="parent">The parent <see cref="Format"/> of the <see cref="LiteralText"/>.</param>
        /// <param name="startIndex">The start index of the <see cref="LiteralText"/> in the format string.</param>
        /// <param name="endIndex">The end index of the <see cref="LiteralText"/> in the format string.</param>
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex, int endIndex) : this(smartSettings, parent.BaseString,
            startIndex, endIndex)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="LiteralText"/>, representing the literal text that is found a parsed format string. 
        /// </summary>
        /// <param name="smartSettings">The <see cref="SmartSettings"/>.</param>
        /// <param name="parent">The parent <see cref="Format"/> of the <see cref="LiteralText"/>.</param>
        public LiteralText(SmartSettings smartSettings, Format parent) : this(smartSettings, parent.BaseString, parent.StartIndex, parent.EndIndex)
        {
        }

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
        /// </summary>
        /// <returns>The string representation of the <see cref="LiteralText"/>, with escaped characters converted.</returns>
        public override string ToString()
        {
            return SmartSettings.Parser.ConvertCharacterStringLiterals
                ? UnEscapeCharacterLiterals().ToString()
                : BaseString.Substring(StartIndex, Length);
        }

        private ReadOnlySpan<char> UnEscapeCharacterLiterals()
        {
            if (Length == 0) return ReadOnlySpan<char>.Empty;
            
            return EscapedLiteral.UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar, BaseString, StartIndex, Length, false);
        }
    }
}