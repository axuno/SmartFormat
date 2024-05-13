// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Handles escaped literals, like \\ or \n
/// </summary>
public static class EscapedLiteral
{
    private static readonly Dictionary<char, char> GeneralLookupTable = new() {
        // General
        {'\\', '\\'},
        {'{', '{'},
        {'}', '}'},
        {'0', '\0'},
        {'a', '\a'},
        {'b', '\b'},
        {'f', '\f'},
        {'n', '\n'},
        {'r', '\r'},
        {'t', '\t'},
        {'v', '\v'},
        {':', ':'} // escaped colons can be used anywhere in the format string
    };

    private static readonly Dictionary<char, char> FormatterOptionsLookupTable = new() {
        // Smart.Format characters used in formatter options
        {'(', '('},
        {')', ')'}
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

    private static char GetUnicode(ReadOnlySpan<char> input, int startIndex)
    {
        var unicode = input.Length - startIndex >= 4
            ? input.Slice(startIndex, 4)
            : input.Slice(startIndex);
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
        if (int.TryParse(unicode, NumberStyles.HexNumber, null, out var result))
#else
            if (int.TryParse(unicode.ToString(), NumberStyles.HexNumber, null, out var result))
#endif
        {
            return (char) result;
        }

        throw new ArgumentException($"Unrecognized escape sequence in literal: \"\\u{unicode.ToString()}\"");
    }

    /// <summary>
    /// Converts escaped characters in input with real characters, e.g. "\\" => "\", if '\' is the escape character.
    /// </summary>
    /// <param name="escapingSequenceStart"></param>
    /// <param name="input"></param>
    /// <param name="includeFormatterOptionChars">If <see langword="true"/>, (){}: will be escaped, else not.</param>
    /// <param name="resultBuffer">The buffer to fill. It's enough to have a buffer with the same size as the input length.</param>
    /// <returns>The input having escaped characters replaced with their real value.</returns>
    public static ReadOnlySpan<char> UnEscapeCharLiterals(char escapingSequenceStart, ReadOnlySpan<char> input, bool includeFormatterOptionChars, Span<char> resultBuffer)
    {
        var max = input.Length;
        var resultIndex = 0;

        var inputIndex = 0;
        while (inputIndex < max)
        {
            int nextInputIndex;

            if (inputIndex + 1 < max)
            {
                nextInputIndex = inputIndex + 1;
            }
            else
            {
                resultBuffer[resultIndex++] = input[inputIndex];
                return resultBuffer.Slice(0, resultIndex);
            }

            if (input[inputIndex] == escapingSequenceStart)
            {
                if (input[nextInputIndex] == 'u')
                {
                    // GetUnicode will throw if code is illegal
                    resultBuffer[resultIndex++] = GetUnicode(input, nextInputIndex + 1);
                    inputIndex += 6;  // move to last unicode character
                }
                else if (TryGetChar(input[nextInputIndex], out var realChar, includeFormatterOptionChars))
                {
                    resultBuffer[resultIndex++] = realChar;
                    inputIndex += 2;
                }
                else
                {
                    throw new ArgumentException($"Unrecognized escape sequence \"{input[inputIndex]}{input[nextInputIndex]}\" in literal.");
                }
            }
            else
            {
                resultBuffer[resultIndex++] = input[inputIndex];
                inputIndex++;
            }
        }

        return resultBuffer.Slice(0, resultIndex);
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
    public static IEnumerable<char> EscapeCharLiterals(char escapeSequenceStart, string input, int startIndex, int length, bool includeFormatterOptionChars)
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
}
