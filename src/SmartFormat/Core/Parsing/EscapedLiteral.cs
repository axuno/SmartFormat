//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Handles escaped literals, like \\ or \n
    /// </summary>
    internal static class EscapedLiteral
    {
        private static Dictionary<char, char> GeneralLookupTable = new() {
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

        private static Dictionary<char, char> FormatterOptionsLookupTable = new() {
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
        public static bool TryGetChar(char input, out char result, bool includeFormatterOptionChars = false)
        {
            return includeFormatterOptionChars
                ? GeneralLookupTable.TryGetValue(input, out result) ||
                  FormatterOptionsLookupTable.TryGetValue(input, out result)
                : GeneralLookupTable.TryGetValue(input, out result);
        }

        /// <summary>
        /// Converts escaped characters in input with real characters, e.g. "\\" => "\", if '\' is the escape character.
        /// </summary>
        /// <param name="escapingSequenceStart"></param>
        /// <param name="input"></param>
        /// <param name="includeFormatterOptionChars">If <see langword="true"/>, (){}: will be escaped, else not.</param>
        /// <returns>The input with escaped characters with their real value.</returns>
        public static ReadOnlySpan<char> UnescapeCharLiterals(char escapingSequenceStart, ReadOnlySpan<char> input, bool includeFormatterOptionChars = false)
        {
            // Note:
            // UnescapeCharacter does the same without memory allocation
            // https://github.com/nemesissoft/Nemesis.TextParsers/blob/master/Nemesis.TextParsers/SpanParserHelper.cs

            var length = input.Length;
            var result = new List<char>(length);
            for (var index = 0; index < length; index++)
            {
                int next;
                if (index + 1 < length)
                {
                    next = index + 1;
                }
                else
                {
                    result.Add(input[index]);
                    return (ReadOnlySpan<char>) result.ToArray();
                }

                if (input[index] == escapingSequenceStart)
                {
                    if (TryGetChar(input[next], out var realChar, includeFormatterOptionChars))
                    {
                        result.Add(realChar);
                    }
                    else
                    {
                        result.Add(input[index]);
                        result.Add(input[next]);
                    }
                    index++;
                }
                else
                {
                    result.Add(input[index]);
                }
            }
            return (ReadOnlySpan<char>) result.ToArray();
        }
    }
}
