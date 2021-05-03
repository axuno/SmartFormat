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
                ? ConvertCharacterLiteralsToUnicode()
                : baseString.Substring(startIndex, endIndex - startIndex);
        }

        private string ConvertCharacterLiteralsToUnicode()
        {
            var source = baseString.Substring(startIndex, endIndex - startIndex);
            if (source.Length == 0) return source;

            // No character literal escaping - nothing to do
            if (source[0] != SmartSettings.Parser.CharLiteralEscapeChar)
                return source;

            // The string length should be 2: escape character \ and literal character
            if (source.Length < 2) throw new ArgumentException($"Missing escape sequence in literal: \"{source}\"");

            if (EscapedLiteral.TryGetChar(source[1], out var result))
            {
                return result.ToString();
            }

            throw new ArgumentException($"Unrecognized escape sequence in literal: \"{source}\"");
        }
    }
}