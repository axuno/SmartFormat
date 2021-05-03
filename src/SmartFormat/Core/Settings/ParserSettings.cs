//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

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
        private readonly IList<char> _numericSelectorChars = "0123456789-".ToCharArray();

        private readonly IList<char> _customSelectorChars = new List<char>();
        private readonly IList<char> _customOperatorChars = new List<char>();

        /// <summary>
        /// Gets or sets the <see cref="ParserSettings.ErrorAction" /> to use for the <see cref="Parser" />.
        /// The default is <see cref="Settings.ErrorAction.ThrowError"/>.
        /// </summary>
        public ParseErrorAction ErrorAction { get; set; } = ParseErrorAction.ThrowError;

        /// <summary>
        /// If <see langword="true"/>, selectors can be alpha-numeric: They may contain
        /// letters, digits, and the underscore character (_), like it is for C# variable names.
        /// If <see langword="false"/>, only digits are allowed as selectors. Default is <see langword="true"/>.
        /// </summary>
        public bool AllowAlphanumericSelectors { get; set; } = true;

        /// <summary>
        /// The list of alphanumeric selector characters.
        /// </summary>
        public IReadOnlyList<char> AlphanumericSelectorChars => (IReadOnlyList<char>) _alphanumericSelectorChars;

        /// <summary>
        /// The list of numeric selector characters.
        /// </summary>
        public IReadOnlyList<char> NumericSelectorChars => (IReadOnlyList<char>) _numericSelectorChars;

        /// <summary>
        /// Gets a read-only list of the custom selector characters, which were set with <see cref="AddCustomSelectorChars"/>.
        /// </summary>
        public IReadOnlyList<char> CustomSelectorChars => (IReadOnlyList<char>) _customSelectorChars;

        /// <summary>
        /// Gets a read-only list of the custom operator characters, which were set with <see cref="AddCustomSelectorChars"/>.
        /// </summary>
        public IReadOnlyList<char> CustomOperatorChars => (IReadOnlyList<char>) _customOperatorChars;

        /// <summary>
        /// Add a list of allowable selector characters on top of the <see cref="AllowAlphanumericSelectors"/> setting.
        /// This can be useful to support additional selector syntax such as math.
        ///  </summary>
        public void AddCustomSelectorChars(IList<char> characters)
        {
            if (AllowAlphanumericSelectors)
            {
                foreach (var c in characters)
                {
                    if (!_customSelectorChars.Contains(c) && !_alphanumericSelectorChars.Contains(c))
                        _customSelectorChars.Add(c);
                }
            }
            else
            {
                foreach (var c in characters)
                {
                    if (!_customSelectorChars.Contains(c) && !_numericSelectorChars.Contains(c))
                        _customSelectorChars.Add(c);
                }
            }
        }

        /// <summary>
        /// Add a list of allowable operator characters on top of the standard <see cref="OperatorChars"/> setting.
        ///  </summary>
        public void AddCustomOperatorChars(IList<char> characters)
        {
            foreach (var c in characters)
            {
                if (!OperatorChars.Contains(c) && !_customOperatorChars.Contains(c))
                    _customOperatorChars.Add(c);
            }
        }

        /// <summary>
        /// Uses <c>string.Format</c>-compatible escaping of curly braces, {{ and }},
        /// instead of the <c>Smart.Format</c> default escaping, \{ and \},
        /// </summary>
        public bool UseStringFormatCompatibility { get; set; } = false;

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
        internal char FormatterNameSeparator { get; set; } = ':';

        /// <summary>
        /// The standard operator characters.
        /// </summary>
        internal IReadOnlyList<char> OperatorChars => new List<char> {SelectorOperator, AlignmentOperator, '[', ']'};

        /// <summary>
        /// The character which separates the selector for alignment. <c>E.g.: Smart.Format("Name: {name,10}")</c>
        /// </summary>
        internal char AlignmentOperator { get; set; } = ',';

        /// <summary>
        /// The character which separates two or more selectors <c>E.g.: "First.Second.Third"</c>
        /// </summary>
        internal char SelectorOperator { get; set; } = '.';

        /// <summary>
        /// Gets the character indicating the start of a <see cref="Placeholder"/>.
        /// </summary>
        public char PlaceholderBeginChar { get; internal set; } = '{';

        /// <summary>
        /// Gets the character indicating the end of a <see cref="Placeholder"/>.
        /// </summary>
        public char PlaceholderEndChar { get; internal set; } = '}';

        /// <summary>
        /// Gets the character indicating the begin of formatter options.
        /// </summary>
        public char FormatterOptionsBeginChar { get; internal set; } = '(';

        /// <summary>
        /// Gets the character indicating the end of formatter options.
        /// </summary>
        public char FormatterOptionsEndChar { get; internal set; } = ')';

        /// <summary>
        /// Characters which terminate parsing of format options.
        /// To use them as options, they must be escaped (preceded) by the <see cref="CharLiteralEscapeChar"/>.
        /// </summary>
        internal char[] FormatOptionsTerminatorChars => new[] {FormatterNameSeparator, FormatterOptionsBeginChar, FormatterOptionsEndChar, PlaceholderBeginChar, PlaceholderEndChar};
    }
}
