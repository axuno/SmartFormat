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
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex) : base(smartSettings, parent,
            startIndex)
        {
        }
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex, int endIndex) : base(smartSettings, parent,
            startIndex, endIndex)
        {
        }

        public LiteralText(SmartSettings smartSettings, Format parent) : base(smartSettings, parent, parent.startIndex)
        {
        }

        public override string ToString()
        {
            return SmartSettings.Parser.ConvertCharacterStringLiterals
                ? UnEscapeCharacterLiterals()
                : baseString.Substring(startIndex, endIndex - startIndex);
        }

        private string UnEscapeCharacterLiterals()
        {
            var source = baseString.AsSpan(startIndex, endIndex - startIndex);
            if (source.Length == 0) return string.Empty;

            return EscapedLiteral.UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar, source, false).ToString();
        }
    }
}