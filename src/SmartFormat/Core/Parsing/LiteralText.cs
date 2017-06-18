using System;
using System.Text;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents the literal text that is found
    /// in a parsed format string.
    /// </summary>
    public class LiteralText : FormatItem
    {
        public LiteralText(SmartSettings smartSettings, Format parent, int startIndex) : base(smartSettings, parent, startIndex)
        { }

        public LiteralText(SmartSettings smartSettings, Format parent) : base(smartSettings, parent, parent.startIndex)
        { }

        public override string ToString()
        {
            return SmartSettings.ConvertCharacterStringLiterals
                ? ConvertCharacterLiteralsToUnicode()
                : baseString.Substring(startIndex, endIndex - startIndex);
        }

        private string ConvertCharacterLiteralsToUnicode()
        {
            var source = this.baseString.Substring(startIndex, endIndex - startIndex);
            var target = new StringBuilder(source.Length);
            var sourcePos = 0;

            while (sourcePos < source.Length)
            {
                var c = source[sourcePos];
                if (c == '\\')
                {
                    sourcePos++; // move to the character after the escape character

                    if (sourcePos >= source.Length)
                        throw new ArgumentException($"Missing escape sequence in literal: \"{source}\"");

                    switch (source[sourcePos])
                    {
                        case '\'':
                            c = '\'';
                            break;
                        case '\"':
                            c = '\"';
                            break;
                        case '\\':
                            c = '\\';
                            break;
                        case '0':
                            c = '\0';
                            break;
                        case 'a':
                            c = '\a';
                            break;
                        case 'b':
                            c = '\b';
                            break;
                        case 'f':
                            c = '\f';
                            break;
                        case 'n':
                            c = '\n';
                            break;
                        case 'r':
                            c = '\r';
                            break;
                        case 't':
                            c = '\t';
                            break;
                        case 'v':
                            c = '\v';
                            break;
                        default:
                            throw new ArgumentException($"Unrecognized escape sequence in literal: \"{source}\"");
                    }
                }
                sourcePos++;
                target.Append(c);
            }
            return target.ToString();
        }
    }
}
