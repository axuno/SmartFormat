//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// Class for <see cref="Parser"/> settings.
    /// </summary>
    public class ParserSettings
    {
        private readonly IList<char> _alphanumericSelectorChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-".ToCharArray();

        private readonly IList<char> _customSelectorChars = new List<char>();
        private readonly IList<char> _customOperatorChars = new List<char>();

        /// <summary>
        /// Gets or sets the <see cref="ParserSettings.ErrorAction" /> to use for the <see cref="Parser" />.
        /// The default is <see cref="Settings.ErrorAction.ThrowError"/>.
        /// </summary>
        public ParseErrorAction ErrorAction { get; set; } = ParseErrorAction.ThrowError;

        /// <summary>
        /// The list of alphanumeric selector characters.
        /// </summary>
        public IReadOnlyList<char> AlphanumericSelectorChars => (IReadOnlyList<char>) _alphanumericSelectorChars;

        /// <summary>
        /// Gets a read-only list of the custom selector characters, which were set with <see cref="AddCustomSelectorChars"/>.
        /// </summary>
        public IReadOnlyList<char> CustomSelectorChars => (IReadOnlyList<char>) _customSelectorChars;

        /// <summary>
        /// Gets a list of characters which are not allowed in a selector.
        /// </summary>
        public IReadOnlyList<char> DisallowedSelectorChars
        {
            get
            {
                var chars = new List<char> {
                    CharLiteralEscapeChar, FormatterNameSeparator, AlignmentOperator, SelectorOperator,
                    PlaceholderBeginChar, PlaceholderEndChar, FormatterOptionsBeginChar, FormatterOptionsEndChar
                };
                chars.AddRange(OperatorChars);
                return chars;
            }
        }

        /// <summary>
        /// Gets a read-only list of the custom operator characters, which were set with <see cref="AddCustomSelectorChars"/>.
        /// Contiguous operator characters are parsed as one operator (e.g. '?.').
        /// </summary>
        public IReadOnlyList<char> CustomOperatorChars => (IReadOnlyList<char>) _customOperatorChars;

        /// <summary>
        /// Add a list of allowable selector characters on top of the <see cref="AlphanumericSelectorChars"/> setting.
        /// This can be useful to support additional selector syntax such as math.
        /// Characters in <see cref="DisallowedSelectorChars"/> cannot be added.
        /// Operator chars and selector chars must be different.
        /// </summary>
        public void AddCustomSelectorChars(IList<char> characters)
        {
            foreach (var c in characters)
            {
                if (DisallowedSelectorChars.Contains(c) || _customOperatorChars.Contains(c))
                    throw new ArgumentException($"Cannot add '{c}' as a custom selector character. It is disallowed or in use as an operator.");

                if (!_customSelectorChars.Contains(c) && !_alphanumericSelectorChars.Contains(c))
                    _customSelectorChars.Add(c);
            }
        }

        /// <summary>
        /// Add a list of allowable operator characters on top of the standard <see cref="OperatorChars"/> setting.
        /// Characters in <see cref="DisallowedSelectorChars"/> cannot be added.
        /// Operator chars and selector chars must be different.
        ///  </summary>
        public void AddCustomOperatorChars(IList<char> characters)
        {
            foreach (var c in characters)
            {
                if (DisallowedSelectorChars.Where(_ => OperatorChars.All(ch => ch != c)).Contains(c) ||
                    _alphanumericSelectorChars.Contains(c) || _customSelectorChars.Contains(c))
                    throw new ArgumentException($"Cannot add '{c}' as a custom operator character. It is disallowed or in use as a selector.");

                if (!OperatorChars.Contains(c) && !_customOperatorChars.Contains(c))
                    _customOperatorChars.Add(c);
            }
        }

        /// <summary>
        /// This setting is relevant for the <see cref="LiteralText" />.
        /// If <see langword="true"/> (the default), character string literals are treated like in "normal" string.Format:
        /// string.Format("\t")   will return a "TAB" character
        /// If <see langword="false"/>, character string literals are not converted, just like with this string.Format:
        /// string.Format(@"\t")  will return the 2 characters "\" and "t"
        /// </summary>
        public bool ConvertCharacterStringLiterals { get; set; } = true;

        /// <summary>
        /// The character literal escape character for <see cref="PlaceholderBeginChar"/> and <see cref="PlaceholderEndChar"/>,
        /// but also others like for \t (TAB), \n (NEW LINE), \\ (BACKSLASH) and others defined in <see cref="EscapedLiteral"/>.
        /// </summary>
        internal char CharLiteralEscapeChar { get; set; } = '\\';

        /// <summary>
        /// The character which separates the formatter name (if any exists) from other parts of the placeholder.
        /// E.g.: {Variable:FormatterName:argument} or {Variable:FormatterName}
        /// </summary>
        internal char FormatterNameSeparator { get; } = ':';

        /// <summary>
        /// The standard operator characters.
        /// Contiguous operator characters are parsed as one operator (e.g. '?.').
        /// </summary>
        internal IReadOnlyList<char> OperatorChars => new List<char> {SelectorOperator, NullableOperator, AlignmentOperator, ListIndexBeginChar, ListIndexEndChar};

        /// <summary>
        /// The character which separates the selector for alignment. <c>E.g.: Smart.Format("Name: {name,10}")</c>
        /// </summary>
        internal char AlignmentOperator { get; } = ',';

        /// <summary>
        /// The character which separates two or more selectors <c>E.g.: "First.Second.Third"</c>
        /// </summary>
        internal char SelectorOperator { get; } = '.';

        /// <summary>
        /// The character which flags the selector as <see langword="nullable"/>.
        /// The character after <see cref="NullableOperator"/> must be the <see cref="SelectorOperator"/>.
        /// <c>E.g.: "First?.Second"</c>
        /// </summary>
        internal char NullableOperator { get; } = '?';

        /// <summary>
        /// Gets the character indicating the start of a <see cref="Placeholder"/>.
        /// </summary>
        public char PlaceholderBeginChar { get; } = '{';

        /// <summary>
        /// Gets the character indicating the end of a <see cref="Placeholder"/>.
        /// </summary>
        public char PlaceholderEndChar { get; } = '}';

        /// <summary>
        /// Gets the character indicating the begin of formatter options.
        /// </summary>
        public char FormatterOptionsBeginChar { get; } = '(';

        /// <summary>
        /// Gets the character indicating the end of formatter options.
        /// </summary>
        public char FormatterOptionsEndChar { get; } = ')';

        /// <summary>
        /// Gets the character indicating the begin of a list index, like in "{Numbers[0]}"
        /// </summary>
        internal char ListIndexBeginChar { get; } = '[';

        /// <summary>
        /// Gets the character indicating the end of a list index, like in "{Numbers[0]}"
        /// </summary>
        internal char ListIndexEndChar { get; } = ']';

        /// <summary>
        /// Characters which terminate parsing of format options.
        /// To use them as options, they must be escaped (preceded) by the <see cref="CharLiteralEscapeChar"/>.
        /// </summary>
        internal char[] FormatOptionsTerminatorChars => new[] {FormatterNameSeparator, FormatterOptionsBeginChar, FormatterOptionsEndChar, PlaceholderBeginChar, PlaceholderEndChar};
    }
}
