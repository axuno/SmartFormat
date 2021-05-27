//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Handles escaped literals, like \\ or \n
    /// </summary>
    public static class EscapedLiteral
    {
        private static readonly Dictionary<char, char> GeneralLookupTable = new() {
            // General
            {'\'', '\''},
            {'\"', '\"'},
            {'\\', '\\'},
            {'0', '\0'},
            {'a', '\a'},
            {'b', '\b'},
            {'f', '\f'},
            {'n', '\n'},
            {'r', '\r'},
            {'t', '\t'},
            {'v', '\v'}
        };

        private static readonly Dictionary<char, char> FormatterOptionsLookupTable = new() {
            // Smart.Format characters used in formatter options
            {'(', '('},
            {')', ')'},
            {'{', '{'},
            {'}', '}'},
            {':', ':'}
        };

        /// <summary>
        /// Tries to get the <see cref="char"/> that corresponds to an escaped input <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input character.</param>
        /// <param name="result">The matching character.</param>
        /// <param name="includeFormatterOptionChars">If <see langword="true"/>, (){}: will be escaped, else not.</param>
        /// <returns><see langword="true"/>, if a matching character was found.</returns>
        public static bool TryGetChar(char input, out char result, bool includeFormatterOptionChars)
        {
            return includeFormatterOptionChars
                ? GeneralLookupTable.TryGetValue(input, out result) ||
                  FormatterOptionsLookupTable.TryGetValue(input, out result)
                : GeneralLookupTable.TryGetValue(input, out result);
        }

        private static char GetUnicode(string input, int startIndex)
        {
            var unicode = input.Length - startIndex >= 4
                ? input.Substring(startIndex, 4)
                : input.Substring(startIndex);

            if (int.TryParse(unicode, NumberStyles.HexNumber, null, out var result))
            {
                return (char) result;
            }

            throw new ArgumentException($"Unrecognized escape sequence in literal: \"\\u{unicode}\"");
        }

        /// <summary>
        /// Converts escaped characters in input with real characters, e.g. "\\" => "\", if '\' is the escape character.
        /// </summary>
        /// <param name="escapingSequenceStart"></param>
        /// <param name="input"></param>
        /// <param name="startIndex">The start index position inside the input.</param>
        /// <param name="length">The number of characters to escape beginning from <c>startIndex</c>.</param>
        /// <param name="includeFormatterOptionChars">If <see langword="true"/>, (){}: will be escaped, else not.</param>
        /// <returns>The input with escaped characters with their real value.</returns>
        public static ReadOnlySpan<char> UnEscapeCharLiterals(char escapingSequenceStart, string input, int startIndex, int length, bool includeFormatterOptionChars)
        {
            //var length = input.Length;
            // It's enough to have a buffer with the same size as input length
            var result = new Span<char>(new char[length]);
            var max = startIndex + length;
            var resultIndex = 0;
            for (var inputIndex = startIndex; inputIndex < max; inputIndex++)
            {
                int nextInputIndex;
                if (inputIndex + 1 < max)
                {
                    nextInputIndex = inputIndex + 1;
                }
                else
                {
                    result[resultIndex++] = input[inputIndex];
                    return result.Slice(0, resultIndex);
                }

                if (input[inputIndex] == escapingSequenceStart)
                {
                    if (input[nextInputIndex] == 'u')
                    {
                        // GetUnicode will throw if code is illegal
                        result[resultIndex++] = GetUnicode(input, nextInputIndex + 1);
                        inputIndex += 5;  // move to last unicode character
                        continue;
                    }

                    if (TryGetChar(input[nextInputIndex], out var realChar, includeFormatterOptionChars))
                    {
                        result[resultIndex++] = realChar;
                    }
                    else
                    {
                        throw new ArgumentException($"Unrecognized escape sequence \"{input[inputIndex]}{input[nextInputIndex]}\" in literal.");
                    }
                    inputIndex++;
                }
                else
                {
                    result[resultIndex++] = input[inputIndex];
                }
            }
            return result.Slice(0, resultIndex);
        }

        /// <summary>
        /// Escapes a string, that contains character which must be escaped.
        /// </summary>
        /// <param name="escapeSequenceStart">The character starting the escape sequence.</param>
        /// <param name="input">The string to escape.</param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="includeFormatterOptionChars"><see langword="true"/>, if characters for formatter options should be included. Default is <see langword="false"/>.</param>
        /// <returns>Returns the escaped characters.</returns>
        internal static IEnumerable<char> EscapeCharLiteralsAsEnumerable(char escapeSequenceStart, string input, int startIndex, int length, bool includeFormatterOptionChars)
        {
            var max = startIndex + length;
            for (var index = startIndex; index < max; index++)
            {
                var c = input[index];
                if (GeneralLookupTable.ContainsValue(c))
                {
                    yield return escapeSequenceStart;
                    yield return GeneralLookupTable.First(kv => kv.Value == c).Key;
                    continue;
                }

                if (includeFormatterOptionChars && FormatterOptionsLookupTable.ContainsValue(c))
                {
                    yield return escapeSequenceStart;
                    yield return FormatterOptionsLookupTable.First(kv => kv.Value == c).Key;
                    continue;
                }

                yield return c;
            }
        }

        /// <summary>
        /// Escapes a string, that contains character which must be escaped.
        /// </summary>
        /// <param name="escapeSequenceStart">The character starting the escape sequence.</param>
        /// <param name="input">The string to escape.</param>
        /// <param name="startIndex">This index position where to start with escaping.</param>
        /// <param name="length">The length of the segment to escape.</param>
        /// <param name="includeFormatterOptionChars"><see langword="true"/>, if characters for formatter options should be included. Default is <see langword="false"/>.</param>
        /// <returns>Returns the string with escaped characters.</returns>
        public static IEnumerable<char> EscapeCharLiterals(char escapeSequenceStart, string input, int startIndex, int length, bool includeFormatterOptionChars)
        {
            return EscapeCharLiteralsAsEnumerable(escapeSequenceStart, input, startIndex, length, includeFormatterOptionChars);
        }
    }
}
