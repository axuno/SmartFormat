//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Handles escaped literals, like \\ or \n
    /// </summary>
    internal static class EscapedLiteral
    {
        private static Dictionary<char, char> _lookupTable = new() {
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
            {'v', '\v'},
            // Smart.Format characters used in formatter options
            {'(', ')'},
            {')', ')'},
            {'{', '{'},
            {'}', '}'},
            {':', ':'}
        };

        /// <summary>
        /// Tries to get the <see cref="char"/> that corresponds to an escaped input <see cref="char"/>.
        /// </summary>
        /// <param name="escaped">The input character.</param>
        /// <param name="result">The matching character.</param>
        /// <returns><see langword="true"/>, if a matching character was found.</returns>
        public static bool TryGetChar(char escaped, out char result)
        {
            return _lookupTable.TryGetValue(escaped, out result);
        }
    }
}
