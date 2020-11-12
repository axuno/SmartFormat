using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Parses a format string.
    /// </summary>
    public class Parser
    {
        #region: Settings :

        /// <summary>
        /// Gets or sets the <seealso cref="Core.Settings.SmartSettings" /> for Smart.Format
        /// </summary>
        public SmartSettings Settings { get; internal set; } = new SmartSettings();

        #endregion

        #region : EventHandlers :

        /// <summary>
        /// Event raising, if an error occurs during parsing.
        /// </summary>
        public event EventHandler<ParsingErrorEventArgs> OnParsingFailure;

        #endregion

        #region: Constructor :

        internal Parser(SmartSettings smartSettings)
        {
            Settings = smartSettings;
        }

        #endregion

        #region: Special Chars :

        // The following fields are points of extensibility

        /// <summary>
        /// If false, only digits are allowed as selectors.
        /// If true, selectors can be alpha-numeric.
        /// This allows optimized alpha-character detection.
        /// Specify any additional selector chars in AllowedSelectorChars.
        /// </summary>
        private bool _alphanumericSelectors;

        /// <summary>
        /// A list of allowable selector characters,
        /// to support additional selector syntaxes such as math.
        /// Digits are always included, and letters can be included
        /// with AlphanumericSelectors.
        /// </summary>
        private string _allowedSelectorChars = "";

        /// <summary>
        /// A list of characters that come between selectors.
        /// This can be "." for dot-notation, "[]" for arrays,
        /// or even math symbols.
        /// By default, there are no operators.
        /// </summary>
        private string _operators = "";

        /// <summary>
        /// If false, double-curly braces are escaped.
        /// If true, the AlternativeEscapeChar is used for escaping braces.
        /// </summary>
        private bool _alternativeEscaping;

        /// <summary>
        /// If AlternativeEscaping is true, then this character is
        /// used to escape curly braces.
        /// </summary>
        private char _alternativeEscapeChar = '\\';

        /// <summary>
        /// The character literal escape character e.g. for \t (TAB) and others.
        /// This is kind of overlapping functionality with <see cref="UseAlternativeEscapeChar" />.
        /// Note: In a future release escape characters for placeholders and character literals should become the same.
        /// </summary>
        internal const char CharLiteralEscapeChar = '\\';

        /// <summary>
        /// Includes a-z and A-Z in the list of allowed selector chars.
        /// </summary>
        public void AddAlphanumericSelectors()
        {
            _alphanumericSelectors = true;
        }

        /// <summary>
        /// Adds specific characters to the allowed selector chars.
        /// </summary>
        /// <param name="chars"></param>
        public void AddAdditionalSelectorChars(string chars)
        {
            foreach (var c in chars)
                if (_allowedSelectorChars.IndexOf(c) == -1)
                    _allowedSelectorChars += c;
        }

        /// <summary>
        /// Adds specific characters to the allowed operator chars.
        /// An operator is a character that is in the selector string
        /// that splits the selectors.
        /// </summary>
        /// <param name="chars"></param>
        public void AddOperators(string chars)
        {
            foreach (var c in chars)
                if (_operators.IndexOf(c) == -1)
                    _operators += c;
        }

        /// <summary>
        /// Sets the AlternativeEscaping option to True
        /// so that braces will only be escaped after the
        /// specified character.
        /// </summary>
        /// <param name="alternativeEscapeChar">Defaults to backslash</param>
        public void UseAlternativeEscapeChar(char alternativeEscapeChar = '\\')
        {
            _alternativeEscapeChar = alternativeEscapeChar;
            _alternativeEscaping = true;
        }

        /// <summary>
        /// [Default]
        /// Uses {{ and }} for escaping braces for compatibility with String.Format.
        /// However, this does not work very well with nested placeholders,
        /// so it is recommended to use an alternative escape char.
        /// </summary>
        public void UseBraceEscaping()
        {
            _alternativeEscaping = false;
        }


        private char _openingBrace = '{';
        private char _closingBrace = '}';

        /// <summary>
        /// Set the closing and opening braces for the parser.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="closing"></param>
        public void UseAlternativeBraces(char opening, char closing)
        {
            _openingBrace = opening;
            _closingBrace = closing;
        }

        #endregion

        #region: Parsing :

        /// <summary>
        /// Parses a format string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatterExtensionNames"></param>
        /// <returns>Returns the <see cref="Format"/> for the parsed string.</returns>
        public Format ParseFormat(string format, string[] formatterExtensionNames)
        {
            var result = new Format(Settings, format);
            var current = result;
            Placeholder currentPlaceholder = null;
            var namedFormatterStartIndex = -1;
            var namedFormatterOptionsStartIndex = -1;
            var namedFormatterOptionsEndIndex = -1;

            // Store parsing errors until the end:
            var parsingErrors = new ParsingErrors(result);
            var parsingErrorText = new ParsingErrorText();

            // Cache properties:
            var openingBrace = _openingBrace;
            var closingBrace = _closingBrace;


            var nestedDepth = 0;
            var lastI = 0;
            var operatorIndex = 0;
            var selectorIndex = 0;
            for (int i = 0, length = format.Length; i < length; i++)
            {
                var c = format[i];
                if (currentPlaceholder == null)
                {
                    if (c == openingBrace)
                    {
                        // Finish the last text item:
                        if (i != lastI) current.Items.Add(new LiteralText(Settings, current, lastI) {endIndex = i});
                        lastI = i + 1;

                        // See if this brace should be escaped:
                        if (!_alternativeEscaping)
                        {
                            var nextI = lastI;
                            if (nextI < length && format[nextI] == openingBrace)
                            {
                                i++;
                                continue;
                            }
                        }

                        // New placeholder:
                        nestedDepth++;
                        currentPlaceholder = new Placeholder(Settings, current, i, nestedDepth);
                        current.Items.Add(currentPlaceholder);
                        current.HasNested = true;
                        operatorIndex = i + 1;
                        selectorIndex = 0;
                        namedFormatterStartIndex = -1;
                    }
                    else if (c == closingBrace)
                    {
                        // Finish the last text item:
                        if (i != lastI)
                            current.Items.Add(new LiteralText(Settings, current, lastI) {endIndex = i});
                        lastI = i + 1;

                        // See if this brace should be escaped:
                        if (!_alternativeEscaping)
                        {
                            var nextI = lastI;
                            if (nextI < length && format[nextI] == closingBrace)
                            {
                                i++;
                                continue;
                            }
                        }

                        // Make sure that this is a nested placeholder before we un-nest it:
                        if (current.parent == null)
                        {
                            parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TooManyClosingBraces], i,
                                i + 1);
                            continue;
                        }

                        // End of the placeholder's Format:
                        nestedDepth--;
                        current.endIndex = i;
                        current.parent.endIndex = i + 1;
                        current = current.parent.parent;
                        namedFormatterStartIndex = -1;
                    }
                    else if (c == CharLiteralEscapeChar && Settings.ConvertCharacterStringLiterals ||
                             _alternativeEscaping && c == _alternativeEscapeChar)
                    {
                        namedFormatterStartIndex = -1;

                        // See that is the next character
                        var nextI = i + 1;

                        // **** Alternative brace escaping with { or } following the escape character ****
                        if (nextI < length && (format[nextI] == openingBrace || format[nextI] == closingBrace))
                        {
                            // Finish the last text item:
                            if (i != lastI) current.Items.Add(new LiteralText(Settings, current, lastI) {endIndex = i});
                            lastI = i + 1;

                            i++;
                        }
                        else
                        {
                            // **** Escaping of charater literals like \t, \n, \v etc. ****

                            // Finish the last text item:
                            if (i != lastI) current.Items.Add(new LiteralText(Settings, current, lastI) {endIndex = i});
                            lastI = i + 2;
                            if (lastI > length) lastI = length;

                            // Next add the character literal INCLUDING the escape character, which LiteralText will expect
                            current.Items.Add(new LiteralText(Settings, current, i) {endIndex = lastI});

                            i++;
                        }
                    }
                    else if (namedFormatterStartIndex != -1)
                    {
                        if (c == '(')
                        {
                            var emptyName = namedFormatterStartIndex == i;
                            if (emptyName)
                            {
                                namedFormatterStartIndex = -1;
                                continue;
                            }

                            namedFormatterOptionsStartIndex = i;
                        }
                        else if (c == ')' || c == ':')
                        {
                            if (c == ')')
                            {
                                var hasOpeningParenthesis = namedFormatterOptionsStartIndex != -1;

                                // ensure no trailing chars past ')'
                                var nextI = i + 1;
                                var nextCharIsValid = nextI < format.Length &&
                                                      (format[nextI] == ':' || format[nextI] == closingBrace);

                                if (!hasOpeningParenthesis || !nextCharIsValid)
                                {
                                    namedFormatterStartIndex = -1;
                                    continue;
                                }

                                namedFormatterOptionsEndIndex = i;

                                if (format[nextI] == ':') i++; // Consume the ':'
                            }

                            var nameIsEmpty = namedFormatterStartIndex == i;
                            var missingClosingParenthesis =
                                namedFormatterOptionsStartIndex != -1 && namedFormatterOptionsEndIndex == -1;
                            if (nameIsEmpty || missingClosingParenthesis)
                            {
                                namedFormatterStartIndex = -1;
                                continue;
                            }


                            lastI = i + 1;

                            var parentPlaceholder = current.parent;

                            if (namedFormatterOptionsStartIndex == -1)
                            {
                                var formatterName = format.Substring(namedFormatterStartIndex,
                                    i - namedFormatterStartIndex);

                                if (FormatterNameExists(formatterName, formatterExtensionNames))
                                    parentPlaceholder.FormatterName = formatterName;
                                else
                                    lastI = current.startIndex;
                            }
                            else
                            {
                                var formatterName = format.Substring(namedFormatterStartIndex,
                                    namedFormatterOptionsStartIndex - namedFormatterStartIndex);

                                if (FormatterNameExists(formatterName, formatterExtensionNames))
                                {
                                    parentPlaceholder.FormatterName = formatterName;
                                    parentPlaceholder.FormatterOptions = format.Substring(
                                        namedFormatterOptionsStartIndex + 1,
                                        namedFormatterOptionsEndIndex - (namedFormatterOptionsStartIndex + 1));
                                }
                                else
                                {
                                    lastI = current.startIndex;
                                }
                            }

                            current.startIndex = lastI;

                            namedFormatterStartIndex = -1;
                        }
                    }
                }
                else
                {
                    // Placeholder is NOT null, so that means 
                    // we're parsing the selectors:
                    if (_operators.IndexOf(c) != -1)
                    {
                        // Add the selector:
                        if (i != lastI)
                        {
                            currentPlaceholder.Selectors.Add(new Selector(Settings, format, lastI, i, operatorIndex,
                                selectorIndex));
                            selectorIndex++;
                            operatorIndex = i;
                        }

                        lastI = i + 1;
                    }
                    else if (c == ':')
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(new Selector(Settings, format, lastI, i, operatorIndex,
                                selectorIndex));
                        else if (operatorIndex != i)
                            parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                                operatorIndex, i);
                        lastI = i + 1;

                        // Start the format:
                        currentPlaceholder.Format = new Format(Settings, currentPlaceholder, i + 1);
                        current = currentPlaceholder.Format;
                        currentPlaceholder = null;
                        namedFormatterStartIndex = lastI;
                        namedFormatterOptionsStartIndex = -1;
                        namedFormatterOptionsEndIndex = -1;
                    }
                    else if (c == closingBrace)
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(new Selector(Settings, format, lastI, i, operatorIndex,
                                selectorIndex));
                        else if (operatorIndex != i)
                            parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                                operatorIndex, i);
                        lastI = i + 1;

                        // End the placeholder with no format:
                        nestedDepth--;
                        currentPlaceholder.endIndex = i + 1;
                        current = currentPlaceholder.parent;
                        currentPlaceholder = null;
                    }
                    else
                    {
                        // Let's make sure the selector characters are valid:
                        // Make sure it's alphanumeric:
                        var isValidSelectorChar =
                            '0' <= c && c <= '9'
                            || _alphanumericSelectors && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')
                            || _allowedSelectorChars.IndexOf(c) != -1;
                        if (!isValidSelectorChar)
                            parsingErrors.AddIssue(current, parsingErrorText[ParsingError.InvalidCharactersInSelector],
                                i, i + 1);
                    }
                }
            }

            // finish the last text item:
            if (lastI != format.Length)
                current.Items.Add(new LiteralText(Settings, current, lastI) {endIndex = format.Length});

            // Check that the format is finished:
            if (current.parent != null || currentPlaceholder != null)
            {
                parsingErrors.AddIssue(current, parsingErrorText[ParsingError.MissingClosingBrace], format.Length,
                    format.Length);
                current.endIndex = format.Length;
                while (current.parent != null)
                {
                    current = current.parent.parent;
                    current.endIndex = format.Length;
                }
            }

            // Check for any parsing errors:
            if (parsingErrors.HasIssues)
            {
                OnParsingFailure?.Invoke(this,
                    new ParsingErrorEventArgs(parsingErrors, Settings.ParseErrorAction == ErrorAction.ThrowError));

                if (Settings.ParseErrorAction == ErrorAction.ThrowError)
                    throw parsingErrors;
            }

            return result;
        }

        private static bool FormatterNameExists(string name, string[] formatterExtensionNames)
        {
            return formatterExtensionNames.Any(n => n == name);
        }

        #endregion

        #region: Errors :

        /// <summary>
        /// Parsing errors.
        /// </summary>
        public enum ParsingError
        {
            /// <summary>
            /// Too many closing braces.
            /// </summary>
            TooManyClosingBraces = 1,
            /// <summary>
            /// Trailing operators in the selector.
            /// </summary>
            TrailingOperatorsInSelector,
            /// <summary>
            /// Invalid characters in the selector.
            /// </summary>
            InvalidCharactersInSelector,
            /// <summary>
            /// Missing closing brace.
            /// </summary>
            MissingClosingBrace
        }

        /// <summary>
        /// Supplies error text for the <see cref="Parser"/>.
        /// </summary>
        public class ParsingErrorText
        {
            private readonly Dictionary<ParsingError, string> _errors = new Dictionary<ParsingError, string>
            {
                {ParsingError.TooManyClosingBraces, "Format string has too many closing braces"},
                {ParsingError.TrailingOperatorsInSelector, "There are trailing operators in the selector"},
                {ParsingError.InvalidCharactersInSelector, "Invalid character in the selector"},
                {ParsingError.MissingClosingBrace, "Format string is missing a closing brace"}
            };

            /// <summary>
            /// CTOR.
            /// </summary>
            internal ParsingErrorText()
            {
            }

            /// <summary>
            /// Gets the string representation of the ParsingError enum.
            /// </summary>
            /// <param name="parsingErrorKey"></param>
            /// <returns>The string representation of the ParsingError enum</returns>
            public string this[ParsingError parsingErrorKey] => _errors[parsingErrorKey];
        }

        #endregion
    }
}