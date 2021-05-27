//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Parses a format string.
    /// </summary>
    public class Parser
    {
        private const int PositionUndefined = -1;

        private readonly ParsingErrorText _parsingErrorText = new ();

        #region: Settings :

        /// <summary>
        /// Gets or sets the <seealso cref="SmartSettings" /> for Smart.Format
        /// </summary>
        public SmartSettings Settings { get; internal set; }

        private readonly ParserSettings _parserSettings;

        #endregion

        #region : EventHandlers :

        /// <summary>
        /// Event raising, if an error occurs during parsing.
        /// </summary>
        public event EventHandler<ParsingErrorEventArgs>? OnParsingFailure;

        #endregion

        #region: Constructor :

        internal Parser(SmartSettings smartSettings)
        {
            Settings = smartSettings;
            _parserSettings = Settings.Parser;
        }

        #endregion

        #region: Special Chars :

        /// <summary>
        /// Includes a-z and A-Z in the list of allowed selector chars.
        /// </summary>
        [Obsolete("Use 'ParserSettings.AllowAlphanumericSelectors' instead.")]
        public void AddAlphanumericSelectors()
        {
            _parserSettings.AllowAlphanumericSelectors = true;
        }

        /// <summary>
        /// Adds specific characters to the allowed selector chars.
        /// </summary>
        /// <param name="chars"></param>
        [Obsolete("Use 'Settings.Parser.AddCustomSelectorChars' instead.")]
        public void AddAdditionalSelectorChars(string chars)
        {
            _parserSettings.AddCustomSelectorChars(chars.ToCharArray());
        }

        /// <summary>
        /// Adds specific characters to the allowed operator chars.
        /// An operator is a character that is in the selector string
        /// that splits the selectors.
        /// </summary>
        /// <param name="chars"></param>
        [Obsolete("Use 'Settings.Parser.AddCustomOperatorChars' instead.")]
        public void AddOperators(string chars)
        {
            _parserSettings.AddCustomOperatorChars(chars.ToCharArray());
        }

        /// <summary>
        /// Sets the AlternativeEscaping option to True
        /// so that braces will only be escaped after the
        /// specified character.
        /// </summary>
        /// <param name="alternativeEscapeChar">Defaults to backslash</param>
        [Obsolete("Use 'Settings.Parser.UseStringFormatCompatibility' instead.")]
        public void UseAlternativeEscapeChar(char alternativeEscapeChar = '\\')
        {
            _parserSettings.UseStringFormatCompatibility = false;
            _parserSettings.CharLiteralEscapeChar = alternativeEscapeChar;
        }

        /// <summary>
        /// [Default]
        /// Uses {{ and }} for escaping braces for compatibility with String.Format.
        /// However, this does not work very well with nested placeholders,
        /// so it is recommended to use an alternative escape char.
        /// </summary>
        [Obsolete("Use 'Settings.Parser.UseStringFormatCompatibility' instead.")]
        public void UseBraceEscaping()
        {
            _parserSettings.UseStringFormatCompatibility = true;
        }

        /// <summary>
        /// Set the closing and opening braces for the parser.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="closing"></param>
        [Obsolete("This feature has been removed", true)]
        public void UseAlternativeBraces(char opening, char closing)
        {
            _parserSettings.PlaceholderBeginChar = opening;
            _parserSettings.PlaceholderEndChar = opening;
        }

        #endregion

        #region: Parsing :

        /// <summary>
        /// The Container for indexes pointing to positions within the input format.
        /// </summary>
        private struct IndexContainer
        {
            /// <summary>
            /// The length of the target object, where indexes will be used.
            /// E.g.: ReadOnlySpan&lt;char&gt;().Length or string.Length
            /// </summary>
            public int ObjectLength;
            
            /// <summary>
            /// The current index within the input format
            /// </summary>
            public int Current;

            /// <summary>
            /// The index after an item (like <see cref="Placeholder"/>, <see cref="Selector"/>, <see cref="LiteralText"/> etc.) was added.
            /// </summary>
            public int LastEnd;

            /// <summary>
            /// The start index of the formatter name
            /// </summary>
            public int NamedFormatterStart;

            /// <summary>
            /// The start index of the formatter options.
            /// </summary>
            public int NamedFormatterOptionsStart;

            /// <summary>
            /// The end index of the formatter options.
            /// </summary>
            public int NamedFormatterOptionsEnd;

            /// <summary>
            /// The index of the operator.
            /// </summary>
            public int Operator;

            /// <summary>
            /// The current index of the selector.
            /// </summary>
            public int Selector;

            /// <summary>
            /// Adds a number to number to the index and returns the sum, but not more than <see cref="ObjectLength"/>.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="add"></param>
            /// <returns>The sum, but not more than <see cref="ObjectLength"/></returns>
            public int SafeAdd(int index, int add)
            {
                // The design is the way, that an end index
                // is always 1 above the last position.
                // Meaning that the maximum of 'FormatItem.endIndex' equals 'inputFormat.Length'
                index += add;
                System.Diagnostics.Debug.Assert(index >= 0);
                return index < ObjectLength ? index : ObjectLength;
            }
        }

        /// <summary>
        /// Parses a format string.
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="formatterExtensionNames"></param>
        /// <returns>The <see cref="Format"/> for the parsed string.</returns>
        public Format ParseFormat(string inputFormat, string[] formatterExtensionNames)
        {
            var index = new IndexContainer {
                ObjectLength = inputFormat.Length,
                Current = PositionUndefined, LastEnd = 0, NamedFormatterStart = PositionUndefined,
                NamedFormatterOptionsStart = PositionUndefined, NamedFormatterOptionsEnd = PositionUndefined,
                Operator = PositionUndefined, Selector = PositionUndefined
            };
            
            // Initialize - won't be re-assigned
            var resultFormat = new Format(Settings, inputFormat);

            // Store parsing errors until parsing is finished:
            var parsingErrors = new ParsingErrors(resultFormat);
            

            var currentFormat = resultFormat;
            Placeholder? currentPlaceholder = null;

            // Used for nested placeholders
            var nestedDepth = 0;

            for (index.Current = 0; index.Current < inputFormat.Length; index.Current++)
            {
                var inputChar = inputFormat[index.Current];
                if (currentPlaceholder == null)
                {
                    if (inputChar == _parserSettings.PlaceholderBeginChar)
                    {
                        AddLiteralCharsParsedBefore(currentFormat, ref index);

                        if (EscapeLikeStringFormat(inputFormat, ref index, _parserSettings.PlaceholderBeginChar)) continue;

                        currentPlaceholder = CreateNewPlaceholder(currentFormat, ref nestedDepth, ref index);

                    }
                    else if (inputChar == _parserSettings.PlaceholderEndChar)
                    {
                        AddLiteralCharsParsedBefore(currentFormat, ref index);

                        if(EscapeLikeStringFormat(inputFormat, ref index, _parserSettings.PlaceholderEndChar)) continue;

                        // Make sure that this is a nested placeholder before we un-nest it:
                        if (HasProcessedTooMayClosingBraces(currentFormat, parsingErrors, ref index)) continue;

                        // End of the placeholder's Format, currentFormat will change to parent.parent
                        FinishPlaceholderFormat(ref currentFormat, ref nestedDepth, ref index);
                    }
                    else if (inputChar == _parserSettings.CharLiteralEscapeChar && _parserSettings.ConvertCharacterStringLiterals ||
                             !_parserSettings.UseStringFormatCompatibility && inputChar == _parserSettings.CharLiteralEscapeChar)
                    {
                        ParseAlternativeEscaping(inputFormat, currentFormat, ref index);
                    }
                    else if (index.NamedFormatterStart != PositionUndefined)
                    {
                        if(!ParseNamedFormatter(inputFormat, currentFormat, ref index, formatterExtensionNames)) continue;
                    }
                }
                else
                {
                    // Placeholder is NOT null, so that means 
                    // we're parsing the selectors:
                    ParseSelector(inputFormat, ref currentFormat, ref index, ref currentPlaceholder, parsingErrors, ref nestedDepth);
                }
            }

            // We're at the end of the input string
            
            // 1. Is the last item a placeholder, that is not finished yet?
            if (currentFormat.Parent != null || currentPlaceholder != null)
            {
                parsingErrors.AddIssue(currentFormat, _parsingErrorText[ParsingError.MissingClosingBrace], inputFormat.Length,
                    inputFormat.Length);
                currentFormat.EndIndex = inputFormat.Length;
            }
            else if (index.LastEnd != inputFormat.Length)
            {
                // 2. The last item must be a literal, so add it
                currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.LastEnd, inputFormat.Length));
            }
            
            // Todo v2.7.0: There is no unit test for this condition!
            while (currentFormat.Parent != null)
            {
                currentFormat = currentFormat.Parent.Parent;
                currentFormat.EndIndex = inputFormat.Length;
            }

            // Check for any parsing errors:
            if (parsingErrors.HasIssues)
            {
                OnParsingFailure?.Invoke(this,
                    new ParsingErrorEventArgs(parsingErrors, Settings.Parser.ErrorAction == ParseErrorAction.ThrowError));

                return HandleParsingErrors(parsingErrors, resultFormat);
            }

            return resultFormat;
        }

        /// <summary>
        /// Adds a new <see cref="LiteralText"/> item, if there are characters left to process.
        /// Sets <see cref="IndexContainer.LastEnd"/>.
        /// </summary>
        /// <param name="currentFormat">The <see cref="Format"/> where the <see cref="LiteralText"/> should be added.</param>
        /// <param name="index">The current indexes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddLiteralCharsParsedBefore(Format currentFormat, ref IndexContainer index)
        {
            // Finish the last text item:
            if (index.Current != index.LastEnd)
            {
                currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.LastEnd, index.Current));
            }

            index.LastEnd = index.SafeAdd(index.Current, 1);
        }

        /// <summary>
        /// Checks, whether we are on top level and still there was a closing brace.
        /// In this case we add the redundant brace as literal and create a <see cref="ParsingError"/>.
        /// </summary>
        /// <param name="currentFormat">The current <see cref="Format"/>.</param>
        /// <param name="parsingErrors">The list of <see cref="ParsingErrors"/>.</param>
        /// <param name="index">The index container.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasProcessedTooMayClosingBraces(Format currentFormat, ParsingErrors parsingErrors, ref IndexContainer index)
        {
            if (currentFormat.Parent != null) return false;

            // Don't swallow-up redundant closing braces, but treat them as literals
            currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.Current, index.Current + 1));
            
            parsingErrors.AddIssue(currentFormat, _parsingErrorText[ParsingError.TooManyClosingBraces], index.Current,
                index.Current + 1);
            return true;
        }

        /// <summary>
        /// In case of string.Format compatibility, we escape the brace
        /// and treat it as a literal character.
        /// </summary>
        /// <param name="inputFormat">The input format string.</param>
        /// <param name="index">The index container.</param>
        /// <param name="brace">The brace { or } to process.</param>
        /// <returns><see langword="true">, if escaping was done.</see></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EscapeLikeStringFormat(string inputFormat, ref IndexContainer index, char brace)
        {
            if (!_parserSettings.UseStringFormatCompatibility) return false;

            if (index.LastEnd < inputFormat.Length && inputFormat[index.LastEnd] == brace)
            {
                index.Current = index.SafeAdd(index.Current, 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a new <see cref="Placeholder"/>, adds it to the current format and sets values in <see cref="IndexContainer"/>.
        /// </summary>
        /// <param name="currentFormat">The <see cref="Format"/> to which the new <see cref="Placeholder"/> will be added.</param>
        /// <param name="nestedDepth">The counter for nesting levels.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        /// <returns>The new <see cref="Placeholder"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Placeholder CreateNewPlaceholder(Format currentFormat, ref int nestedDepth, ref IndexContainer index)
        {
            nestedDepth++;
            var placeholder = new Placeholder(Settings, currentFormat, index.Current, nestedDepth);
            currentFormat.Items.Add(placeholder);
            currentFormat.HasNested = true;
            index.Operator = index.SafeAdd(index.Current, 1);
            index.Selector = 0;
            index.NamedFormatterStart = PositionUndefined;
            return placeholder;
        }

        /// <summary>
        /// Finishes the current placeholder <see cref="Format"/>.
        /// </summary>
        /// <param name="currentFormat">The <see cref="Format"/> which will change to parent.parent</param>
        /// <param name="nestedDepth">The counter for nesting levels.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FinishPlaceholderFormat(ref Format currentFormat, ref int nestedDepth, ref IndexContainer index)
        {
            if (currentFormat.Parent == null)
            {
                throw new NullReferenceException($"Unexpected null reference: {nameof(currentFormat.Parent)}");
            }

            nestedDepth--;
            currentFormat.EndIndex = index.Current;
            currentFormat.Parent.EndIndex = index.SafeAdd(index.Current, 1);
            currentFormat = currentFormat.Parent.Parent;
            index.NamedFormatterStart = index.NamedFormatterOptionsStart = index.NamedFormatterOptionsEnd = PositionUndefined; //2021-05-03 axuno
        }

        /// <summary>
        /// Processes the character if alternative escaping is used.
        /// </summary>
        /// <param name="inputFormat">The input format string.</param>
        /// <param name="currentFormat">The current <see cref="Format"/>.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseAlternativeEscaping(string inputFormat, Format currentFormat, ref IndexContainer index)
        {
            // 2021-05-03/axuno: Why? Does not make in difference in unit tests
            // index.NamedFormatterStart = PositionUndefined;

            // See what is the next character
            var indexNextChar = index.SafeAdd(index.Current, 1);

            // **** Alternative brace escaping with { or } following the escape character ****
            if (indexNextChar < inputFormat.Length && (inputFormat[indexNextChar] == _parserSettings.PlaceholderBeginChar || inputFormat[indexNextChar] == _parserSettings.PlaceholderEndChar))
            {
                // Finish the last text item:
                if (index.Current != index.LastEnd) currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.LastEnd, index.Current));
                index.LastEnd = index.SafeAdd(index.Current, 1);

                index.Current++;
            }
            else
            {
                // **** Escaping of character literals like \t, \n, \v etc. ****

                // Finish the last text item:
                if (index.Current != index.LastEnd) currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.LastEnd, index.Current));
                                
                // Is this a unicode escape sequence?
                if (inputFormat[indexNextChar] == 'u')
                {
                    // The next 4 characters must represent the unicode 
                    index.LastEnd = index.SafeAdd(index.Current, 6);
                    currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.Current, index.LastEnd));
                }
                else
                {
                    // Next add the character literal INCLUDING the escape character, which LiteralText will expect
                    index.LastEnd = index.SafeAdd(index.Current, 2);
                    currentFormat.Items.Add(new LiteralText(Settings, currentFormat, index.Current, index.LastEnd));
                }

                index.Current = index.SafeAdd(index.Current, 1);
            }
        }

        /// <summary>
        /// Handles named formatters.
        /// </summary>
        /// <param name="inputFormat">The input format string.</param>
        /// <param name="currentFormat">The current <see cref="Format"/>.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        /// <param name="formatterExtensionNames">The registered formatter names</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ParseNamedFormatter(string inputFormat, Format currentFormat, ref IndexContainer index, string[] formatterExtensionNames)
        {
            var inputChar = inputFormat[index.Current];
            if (inputChar == _parserSettings.FormatterOptionsBeginChar)
            {
                var emptyName = index.NamedFormatterStart == index.Current;
                if (emptyName)
                {
                    index.NamedFormatterStart = PositionUndefined;
                    return false;
                }

                // Note: This short-circuits the Parser.ParseFormat main loop
                ParseFormatOptions(inputFormat, ref index);
            }
            else if (inputChar == _parserSettings.FormatterOptionsEndChar || inputChar == _parserSettings.FormatterNameSeparator)
            {
                if (inputChar == _parserSettings.FormatterOptionsEndChar)
                {
                    var hasOpeningParenthesis = index.NamedFormatterOptionsStart != PositionUndefined;

                    // ensure no trailing chars past ')'
                    var nextCharIndex = index.SafeAdd(index.Current, 1);
                    var nextCharIsValid = nextCharIndex < inputFormat.Length &&
                                          (inputFormat[nextCharIndex] == _parserSettings.FormatterNameSeparator || inputFormat[nextCharIndex] == _parserSettings.PlaceholderEndChar);

                    if (!hasOpeningParenthesis || !nextCharIsValid)
                    {
                        index.NamedFormatterStart = PositionUndefined;
                        return false;
                    }

                    index.NamedFormatterOptionsEnd = index.Current;

                    if (inputFormat[nextCharIndex] == _parserSettings.FormatterNameSeparator) index.Current++; // Consume the ':'
                }

                var nameIsEmpty = index.NamedFormatterStart == index.Current;
                var missingClosingParenthesis =
                    index.NamedFormatterOptionsStart != PositionUndefined &&
                    index.NamedFormatterOptionsEnd == PositionUndefined;
                if (nameIsEmpty || missingClosingParenthesis)
                {
                    index.NamedFormatterStart = PositionUndefined;
                    return false;
                }
                
                index.LastEnd = index.SafeAdd(index.Current, 1);

                var parentPlaceholder = currentFormat.Parent;

                if (index.NamedFormatterOptionsStart == PositionUndefined)
                {
                    var formatterName = inputFormat.Substring(index.NamedFormatterStart,
                        index.Current - index.NamedFormatterStart);

                    if (FormatterNameExists(formatterName, formatterExtensionNames))
                    {
                        if (parentPlaceholder != null) parentPlaceholder.FormatterName = formatterName;
                    }
                    else
                        index.LastEnd = currentFormat.StartIndex;
                }
                else
                {
                    var formatterName = inputFormat.Substring(index.NamedFormatterStart,
                        index.NamedFormatterOptionsStart - index.NamedFormatterStart);

                    if (FormatterNameExists(formatterName, formatterExtensionNames))
                    {
                        if (parentPlaceholder != null)
                        {
                            parentPlaceholder.FormatterName = formatterName;
                            // Save the formatter options with CharLiteralEscapeChar removed
                            parentPlaceholder.FormatterOptionsRaw = inputFormat.Substring(index.NamedFormatterOptionsStart + 1,
                                    index.NamedFormatterOptionsEnd - (index.NamedFormatterOptionsStart + 1));
                        }
                    }
                    else
                    {
                        index.LastEnd = currentFormat.StartIndex;
                    }
                }

                // Set start index to start of formatter option arguments,
                // with {0:default:N2} the start index is on the second colon
                currentFormat.StartIndex = index.LastEnd;

                index.NamedFormatterStart = PositionUndefined;
            }

            return true;
        }

        /// <summary>
        /// Handles the selectors.
        /// </summary>
        /// <param name="inputFormat">The input format string.</param>
        /// <param name="currentFormat">The current <see cref="Format"/>.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        /// <param name="currentPlaceholder"></param>
        /// <param name="parsingErrors"></param>
        /// <param name="nestedDepth"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseSelector(string inputFormat, ref Format currentFormat, ref IndexContainer index,
            ref Placeholder? currentPlaceholder, ParsingErrors parsingErrors, ref int nestedDepth)
        {
            if (currentPlaceholder == null)
            {
                throw new NullReferenceException($"Unexpected null reference: {nameof(currentPlaceholder)}");
            }

            var inputChar = inputFormat[index.Current];
            if (_parserSettings.OperatorChars.Contains(inputChar))
            {
                // Add the selector:
                if (index.Current != index.LastEnd)
                {
                    currentPlaceholder.Selectors.Add(new Selector(Settings, inputFormat, index.LastEnd, index.Current,
                        index.Operator,
                        index.Selector));
                    index.Selector = index.SafeAdd(index.Selector, 1);
                    index.Operator = index.Current;
                }

                index.LastEnd = index.SafeAdd(index.Current, 1);
            }
            else if (inputChar == _parserSettings.FormatterNameSeparator)
            {
                // Add the selector:
                if (index.Current != index.LastEnd)
                    currentPlaceholder.Selectors.Add(new Selector(Settings, inputFormat, index.LastEnd, index.Current,
                        index.Operator,
                        index.Selector));
                else if (index.Operator != index.Current)
                    parsingErrors.AddIssue(currentFormat,
                        $"'0x{Convert.ToByte(inputChar):X}': " +
                        _parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                        index.Operator, index.Current);
                index.LastEnd = index.SafeAdd(index.Current, 1);

                // Start the format:
                currentPlaceholder.Format = new Format(Settings, currentPlaceholder, index.Current + 1);
                currentFormat = currentPlaceholder.Format;
                currentPlaceholder = null;
                index.NamedFormatterStart = index.LastEnd;
                index.NamedFormatterOptionsStart = PositionUndefined;
                index.NamedFormatterOptionsEnd = PositionUndefined;
            }
            else if (inputChar == _parserSettings.PlaceholderEndChar)
            {
                // Add the selector:
                if (index.Current != index.LastEnd)
                    currentPlaceholder.Selectors.Add(new Selector(Settings, inputFormat, index.LastEnd, index.Current,
                        index.Operator,
                        index.Selector));
                else if (index.Operator != index.Current)
                    parsingErrors.AddIssue(currentFormat,
                        $"'0x{Convert.ToByte(inputChar):X}': " +
                        _parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                        index.Operator, index.Current);
                index.LastEnd = index.SafeAdd(index.Current, 1);

                // End the placeholder with no format:
                nestedDepth--;
                currentPlaceholder.EndIndex = index.SafeAdd(index.Current, 1);
                currentFormat = currentPlaceholder.Parent;
                currentPlaceholder = null;
            }
            else
            {
                // Ensure the selector characters are valid:
                if (!IsValidSelectorChar(inputChar))
                    parsingErrors.AddIssue(currentFormat,
                        $"'0x{Convert.ToByte(inputChar):X}': " +
                        _parsingErrorText[ParsingError.InvalidCharactersInSelector],
                        index.Current, index.SafeAdd(index.Current, 1));
            }
        }

        /// <summary>
        /// Parses all option characters.
        /// This short-circuits the Parser.ParseFormat main loop.
        /// </summary>
        /// <param name="inputFormat">The input format string.</param>
        /// <param name="index">The <see cref="IndexContainer"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseFormatOptions(string inputFormat, ref IndexContainer index)
        {
            index.NamedFormatterOptionsStart = index.Current;

            var nextChar = inputFormat[index.SafeAdd(index.Current, 1)];
            // Handle empty options()
            if (_parserSettings.FormatOptionsTerminatorChars.Contains(nextChar)) return;
            
            while (++index.Current < index.ObjectLength)
            {
                nextChar = inputFormat[index.SafeAdd(index.Current, 1)];
                // Skip escaped terminating characters
                if (inputFormat[index.Current] == _parserSettings.CharLiteralEscapeChar &&
                    (_parserSettings.FormatOptionsTerminatorChars.Contains(nextChar) ||
                     EscapedLiteral.TryGetChar(nextChar, out _, true)))
                {
                    index.Current = index.SafeAdd(index.Current, 1);
                    if (_parserSettings.FormatOptionsTerminatorChars.Contains(
                        inputFormat[index.SafeAdd(index.Current, 1)]))
                    {
                        return;
                    }

                    continue;
                }

                // End of parsing options, when the NEXT character is terminating,
                // because this character will be handled in the Parser.ParseFormat main loop.
                if (_parserSettings.FormatOptionsTerminatorChars.Contains(inputFormat[index.Current + 1]))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Checks whether the selector character is valid,
        /// depending on <see cref="ParserSettings.AllowAlphanumericSelectors"/> and <see cref="ParserSettings.CustomSelectorChars"/>.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns><see langword="true"/> if the character is valid, else <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsValidSelectorChar(char c)
        {
            // C# variables: LetterOrDigit and "_"
            // Required for Alignment: "-" and ","
            return _parserSettings.AllowAlphanumericSelectors
                ? _parserSettings.AlphanumericSelectorChars.Contains(c) || _parserSettings.OperatorChars.Contains(c) || _parserSettings.CustomSelectorChars.Contains(c)
                : _parserSettings.NumericSelectorChars.Contains(c) || _parserSettings.OperatorChars.Contains(c);
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

        /// <summary>
        /// Handles <see cref="ParsingError"/>s as defined in <see cref="SmartSettings.ParseErrorAction"/>.
        /// </summary>
        /// <param name="parsingErrors"></param>
        /// <param name="currentResult"></param>
        /// <returns>The <see cref="Format"/> which will be further processed by the formatter.</returns>
        private Format HandleParsingErrors(ParsingErrors parsingErrors, Format currentResult)
        {
            switch (Settings.Parser.ErrorAction)
            {
                case ParseErrorAction.ThrowError:
                    throw parsingErrors;
                case ParseErrorAction.MaintainTokens:
                    // Replace erroneous Placeholders with tokens as LiteralText
                    // Placeholder without issues are left unmodified
                    for (var i = 0; i < currentResult.Items.Count; i++)
                    {
                        if (currentResult.Items[i] is Placeholder ph && parsingErrors.Issues.Any(errItem => errItem.Index >= currentResult.Items[i].StartIndex && errItem.Index <= currentResult.Items[i].EndIndex))
                        {
                            currentResult.Items[i] = new LiteralText(Settings, ph.Format ?? new Format(Settings, ph.BaseString), ph.StartIndex, ph.EndIndex);
                        }
                    }
                    return currentResult;
                case ParseErrorAction.Ignore:
                    // Replace erroneous Placeholders with an empty LiteralText
                    for (var i = 0; i < currentResult.Items.Count; i++)
                    {
                        if (currentResult.Items[i] is Placeholder ph && parsingErrors.Issues.Any(errItem => errItem.Index >= currentResult.Items[i].StartIndex && errItem.Index <= currentResult.Items[i].EndIndex))
                        {
                            currentResult.Items[i] = new LiteralText(Settings, ph.Format ?? new Format(Settings, ph.BaseString), ph.StartIndex, ph.StartIndex);
                        }
                    }
                    return currentResult;
                case ParseErrorAction.OutputErrorInResult:
                    var fmt = new Format(Settings, parsingErrors.Message, 0, parsingErrors.Message.Length);
                    fmt.Items.Add(new LiteralText(Settings, fmt));
                    return fmt;
                default:
                    throw new ArgumentException("Illegal type for ParsingErrors", parsingErrors);
            }
        }

        #endregion
    }
}