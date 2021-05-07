//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.Collections.Generic;

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
            var length = input.Length;
            //Span<char> result = ArrayPool<char>.Shared.Rent(length);
            //It's enough to have the buffer with same size as input length
            var result = new Span<char>(new char[length]);
            var resultIndex = 0;
            for (var inputIndex = 0; inputIndex < length; inputIndex++)
            {
                int nextInputIndex;
                if (inputIndex + 1 < length)
                {
                    nextInputIndex = inputIndex + 1;
                }
                else
                {
                    result[resultIndex++] = input[inputIndex];
                    return (ReadOnlySpan<char>) result.Slice(0, resultIndex);
                }

                if (input[inputIndex] == escapingSequenceStart)
                {
                    if (TryGetChar(input[nextInputIndex], out var realChar, includeFormatterOptionChars))
                    {
                        result[resultIndex++] = realChar;
                    }
                    else
                    {
                        result[resultIndex++] = input[inputIndex];
                        result[resultIndex++] = input[nextInputIndex];
                    }
                    inputIndex++;
                }
                else
                {
                    result[resultIndex++] = input[inputIndex];
                }
            }
            return (ReadOnlySpan<char>) result.Slice(0, resultIndex);
        }
    }
}
