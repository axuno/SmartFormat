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
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex) : base(smartSettings, parent.BaseString,
            startIndex,  parent.EndIndex)
        {
        }
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex, int endIndex) : base(smartSettings, parent.BaseString,
            startIndex, endIndex)
        {
        }

        public LiteralText(SmartSettings smartSettings, Format parent) : base(smartSettings, parent.BaseString, parent.StartIndex, parent.EndIndex)
        {
        }

        public LiteralText(SmartSettings smartSettings, string baseString, int startIndex, int endIndex) : base(smartSettings, baseString, startIndex, endIndex)
        {
        }

        public override string ToString()
        {
            return SmartSettings.Parser.ConvertCharacterStringLiterals
                ? UnEscapeCharacterLiterals()
                : BaseString.Substring(StartIndex, EndIndex - StartIndex);
        }

        private string UnEscapeCharacterLiterals()
        {
            var source = BaseString.AsSpan(StartIndex, EndIndex - StartIndex);
            if (source.Length == 0) return string.Empty;

            return EscapedLiteral.UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar, source, false).ToString();
        }
    }
}