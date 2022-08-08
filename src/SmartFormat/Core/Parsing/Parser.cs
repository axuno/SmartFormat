// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Parses a format string.
/// </summary>
public class Parser
{
    private const int PositionUndefined = -1;
    private readonly ParsingErrorText _parsingErrorText = new ();

    #region: Parser.ParseFormat and callees :

    private IndexContainer _index;
    private string _inputFormat;
    private Format _resultFormat;

    #endregion

    #region: Settings :

    /// <summary>
    /// Gets or sets the <see cref="SmartSettings" /> for Smart.Format
    /// </summary>
    public SmartSettings Settings { get; }

    // Cache method results from settings
    private readonly List<char> _operatorChars;
    private readonly List<char> _customOperatorChars;
    private readonly ParserSettings _parserSettings;
    private readonly List<char> _validSelectorChars;
    private readonly List<char> _formatOptionsTerminatorChars;

    #endregion

    #region : EventHandlers :

    /// <summary>
    /// Event raising, if an error occurs during parsing.
    /// </summary>
    public event EventHandler<ParsingErrorEventArgs>? OnParsingFailure;

    #endregion

    #region: Constructor :

    /// <summary>
    /// Creates a new instance of a <see cref="Parser"/>.
    /// </summary>
    /// <param name="smartSettings">
    /// The <see cref="SmartSettings"/> to use, or <see langword="null"/> for default settings.
    /// Any changes after passing settings as a parameter may not have effect.
    /// </param>
    public Parser(SmartSettings? smartSettings = null)
    {
        Settings = smartSettings ?? new SmartSettings();
        _parserSettings = Settings.Parser;
        _operatorChars = _parserSettings.OperatorChars();
        _customOperatorChars = _parserSettings.CustomOperatorChars();
        _formatOptionsTerminatorChars = _parserSettings.FormatOptionsTerminatorChars();

        _validSelectorChars = new List<char>();
        _validSelectorChars.AddRange(_parserSettings.SelectorChars());
        _validSelectorChars.AddRange(_parserSettings.OperatorChars());
        _validSelectorChars.AddRange(_parserSettings.CustomSelectorChars());
            
        _inputFormat = string.Empty;
        _resultFormat = InitializationObject.Format; // will never be used
    }

    #endregion

    #region: Special Chars :

    /// <summary>
    /// Includes a-z and A-Z in the list of allowed selector chars.
    /// </summary>
    [Obsolete("Alphanumeric selectors are always enabled", true)]
    public void AddAlphanumericSelectors()
    {
        // Do nothing - this is the standard behavior
    }

    /// <summary>
    /// Adds specific characters to the allowed selector chars.
    /// </summary>
    /// <param name="chars"></param>
    [Obsolete("Use 'Settings.Parser.AddCustomSelectorChars' instead.", true)]
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
    [Obsolete("Use 'Settings.Parser.AddCustomOperatorChars' instead.", true)]
    public void AddOperators(string chars)
    {
        _parserSettings.AddCustomOperatorChars(chars.ToCharArray());
    }

    /// <summary>
    /// Sets the AlternativeEscaping option to True
    /// so that braces will only be escaped after the
    /// specified character. The only allowed escape character is the backslash '\'.
    /// </summary>
    /// <param name="alternativeEscapeChar">Defaults to backslash</param>
    [Obsolete("Use 'Settings.StringFormatCompatibility' instead.", true)]
    public void UseAlternativeEscapeChar(char alternativeEscapeChar = '\\')
    {
        if (alternativeEscapeChar != _parserSettings.CharLiteralEscapeChar)
        {
            throw new ArgumentException("Cannot set an escape character other than '\\'",
                nameof(alternativeEscapeChar));
        }
        Settings.StringFormatCompatibility = false;
    }

    /// <summary>
    /// Uses {{ and }} for escaping braces for compatibility with string.Format.
    /// However, this does not work very well with nested placeholders,
    /// so it is recommended to use an 'alternative' escape char, which is the
    /// backslash.
    /// </summary>
    [Obsolete("Use 'Settings.StringFormatCompatibility' instead.", true)]
    public void UseBraceEscaping()
    {
        throw new NotSupportedException($"Init-only property {nameof(Settings)}.{nameof(Settings.StringFormatCompatibility)} can only be set in an object initializer");
    }

    /// <summary>
    /// Set the closing and opening braces for the parser.
    /// </summary>
    /// <param name="opening"></param>
    /// <param name="closing"></param>
    [Obsolete("This feature has been removed", true)]
    public void UseAlternativeBraces(char opening, char closing)
    {
        throw new NotSupportedException("This feature has been removed");
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
        /// The index within the input format after an item (like <see cref="Placeholder"/>, <see cref="Selector"/>, <see cref="LiteralText"/> etc.) was added.
        /// </summary>
        public int LastEnd;

        /// <summary>
        /// The start index of the formatter name within the input format.
        /// </summary>
        public int NamedFormatterStart;

        /// <summary>
        /// The start index of the formatter options within the input format.
        /// </summary>
        public int NamedFormatterOptionsStart;

        /// <summary>
        /// The end index of the formatter options within the input format.
        /// </summary>
        public int NamedFormatterOptionsEnd;

        /// <summary>
        /// The index of the operator within the input format.
        /// </summary>
        public int Operator;

        /// <summary>
        /// The current index of the selector <b>across all</b> <see cref="Placeholder"/>s.
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
            // Meaning that the maximum of 'FormatItem.EndIndex' equals 'inputFormat.Length'
            index += add;
            System.Diagnostics.Debug.Assert(index >= 0);
            return index < ObjectLength ? index : ObjectLength;
        }
    }

    /// <summary>
    /// Parses a format string.
    /// </summary>
    /// <param name="inputFormat"></param>
    /// <returns>The <see cref="Format"/> for the parsed string.</returns>
    public Format ParseFormat(string inputFormat)
    {
        _inputFormat = inputFormat;

        _index = new IndexContainer {
            ObjectLength = _inputFormat.Length,
            Current = PositionUndefined, LastEnd = 0, NamedFormatterStart = PositionUndefined,
            NamedFormatterOptionsStart = PositionUndefined, NamedFormatterOptionsEnd = PositionUndefined,
            Operator = PositionUndefined, Selector = PositionUndefined
        };
            
        // Initialize - will be re-assigned with new placeholders
        _resultFormat = FormatPool.Instance.Get().Initialize(Settings, _inputFormat);

        // Store parsing errors until parsing is finished:
        var parsingErrors = ParsingErrorsPool.Instance.Get().Initialize(_resultFormat);

        Placeholder? currentPlaceholder = null;

        // Used for nested placeholders
        var nestedDepth = 0;

        for (_index.Current = 0; _index.Current < _inputFormat.Length; _index.Current++)
        {
            var inputChar = _inputFormat[_index.Current];
            if (currentPlaceholder == null)
            {
                // We're parsing literal text with an HTML tag
                if (_parserSettings.ParseInputAsHtml && inputChar == '<')
                {
                    ParseHtmlTags();
                    continue;
                }

                if (inputChar == _parserSettings.PlaceholderBeginChar)
                {
                    AddLiteralCharsParsedBefore();

                    if (EscapeLikeStringFormat(_parserSettings.PlaceholderBeginChar)) continue;

                    CreateNewPlaceholder(ref nestedDepth, out currentPlaceholder);
                }
                else if (inputChar == _parserSettings.PlaceholderEndChar)
                {
                    AddLiteralCharsParsedBefore();

                    if(EscapeLikeStringFormat(_parserSettings.PlaceholderEndChar)) continue;

                    // Make sure that this is a nested placeholder before we un-nest it:
                    if (HasProcessedTooMayClosingBraces(parsingErrors)) continue;

                    // End of the placeholder's Format, _resultFormat will change to parent.parent
                    FinishPlaceholderFormat(ref nestedDepth);
                }
                else if (inputChar == _parserSettings.CharLiteralEscapeChar && _parserSettings.ConvertCharacterStringLiterals ||
                         !Settings.StringFormatCompatibility && inputChar == _parserSettings.CharLiteralEscapeChar)
                {
                    ParseAlternativeEscaping();
                }
                else if (_index.NamedFormatterStart != PositionUndefined && !ParseNamedFormatter())
                {
                    // continue the loop
                }
            }
            else
            {
                // Placeholder is NOT null, so that means 
                // we're parsing the selectors:
                ParseSelector(ref currentPlaceholder, parsingErrors, ref nestedDepth);
            }
        }

        // We're at the end of the input string
            
        // 1. Is the last item a placeholder, that is not finished yet?
        if (_resultFormat.ParentPlaceholder != null || currentPlaceholder != null)
        {
            parsingErrors.AddIssue(_resultFormat, _parsingErrorText[ParsingError.MissingClosingBrace], _inputFormat.Length,
                _inputFormat.Length);
            _resultFormat.EndIndex = _inputFormat.Length;
        }
        else if (_index.LastEnd != _inputFormat.Length)
        {
            // 2. The last item must be a literal, so add it
            _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.LastEnd, _inputFormat.Length));
        }
            
        // This may happen with a missing closing brace, e.g. "{0:yyyy/MM/dd HH:mm:ss"
        while (_resultFormat.ParentPlaceholder != null)
        {
            _resultFormat = _resultFormat.ParentPlaceholder.Parent;
            _resultFormat.EndIndex = _inputFormat.Length;
        }

        // Check for any parsing errors:
        if (parsingErrors.HasIssues)
        {
            OnParsingFailure?.Invoke(this,
                new ParsingErrorEventArgs(parsingErrors, Settings.Parser.ErrorAction == ParseErrorAction.ThrowError));

            return HandleParsingErrors(parsingErrors, _resultFormat);
        }

        ParsingErrorsPool.Instance.Return(parsingErrors);
        return _resultFormat;
    }

    /// <summary>
    /// Adds a new <see cref="LiteralText"/> item, if there are characters left to process.
    /// Sets <see cref="IndexContainer.LastEnd"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLiteralCharsParsedBefore()
    {
        // Finish the last text item:
        if (_index.Current != _index.LastEnd)
        {
            _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.LastEnd, _index.Current));
        }

        _index.LastEnd = _index.SafeAdd(_index.Current, 1);
    }

    /// <summary>
    /// Checks, whether we are on top level and still there was a closing brace.
    /// In this case we add the redundant brace as literal and create a <see cref="ParsingError"/>.
    /// </summary>
    /// <param name="parsingErrors">The list of <see cref="ParsingErrors"/>.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasProcessedTooMayClosingBraces(ParsingErrors parsingErrors)
    {
        if (_resultFormat.ParentPlaceholder != null) return false;

        // Don't swallow-up redundant closing braces, but treat them as literals
        _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.Current, _index.Current + 1));
            
        parsingErrors.AddIssue(_resultFormat, _parsingErrorText[ParsingError.TooManyClosingBraces], _index.Current,
            _index.Current + 1);
        return true;
    }

    /// <summary>
    /// In case of string.Format compatibility, we escape the brace
    /// and treat it as a literal character.
    /// </summary>
    /// <param name="brace">The brace { or } to process.</param>
    /// <returns><see langword="true">, if escaping was done.</see></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EscapeLikeStringFormat(char brace)
    {
        if (!Settings.StringFormatCompatibility) return false;

        if (_index.LastEnd < _inputFormat.Length && _inputFormat[_index.LastEnd] == brace)
        {
            _index.Current = _index.SafeAdd(_index.Current, 1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new <see cref="Placeholder"/>, adds it to the current format and sets values in <see cref="IndexContainer"/>.
    /// </summary>
    /// <param name="nestedDepth">The counter for nesting levels.</param>
    /// <param name="newPlaceholder"></param>
    /// <returns>The new <see cref="Placeholder"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateNewPlaceholder(ref int nestedDepth, out Placeholder newPlaceholder)
    {
        nestedDepth++;
        newPlaceholder = PlaceholderPool.Instance.Get().Initialize(_resultFormat, _index.Current, nestedDepth);
        _resultFormat.Items.Add(newPlaceholder);
        _resultFormat.HasNested = true;
        _index.Operator = _index.SafeAdd(_index.Current, 1);
        _index.Selector = 0;
        _index.NamedFormatterStart = PositionUndefined;
    }

    /// <summary>
    /// Finishes the current placeholder <see cref="Format"/>.
    /// </summary>
    /// <param name="nestedDepth">The counter for nesting levels.</param>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FinishPlaceholderFormat(ref int nestedDepth)
    {
        System.Diagnostics.Debug.Assert(_resultFormat.ParentPlaceholder is not null);

        nestedDepth--;
        _resultFormat.EndIndex = _index.Current;
        _resultFormat.ParentPlaceholder!.EndIndex = _index.SafeAdd(_index.Current, 1);
        _resultFormat = _resultFormat.ParentPlaceholder.Parent;
        _index.NamedFormatterStart = _index.NamedFormatterOptionsStart = _index.NamedFormatterOptionsEnd = PositionUndefined;
    }

    /// <summary>
    /// Processes the character if alternative escaping is used.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseAlternativeEscaping()
    {
        // 2021-05-03/axuno: Removed "index.NamedFormatterStart = PositionUndefined"

        // See what is the next character
        var indexNextChar = _index.SafeAdd(_index.Current, 1);
        if (indexNextChar >= _inputFormat.Length)
            throw new ArgumentException($"Unrecognized escape sequence at the end of the literal");

        // **** Alternative brace escaping with { or } following the escape character ****
        if (_inputFormat[indexNextChar] == _parserSettings.PlaceholderBeginChar || _inputFormat[indexNextChar] == _parserSettings.PlaceholderEndChar)
        {
            // Finish the last text item:
            if (_index.Current != _index.LastEnd) _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.LastEnd, _index.Current));
            _index.LastEnd = _index.SafeAdd(_index.Current, 1);

            _index.Current++;
        }
        else
        {
            // **** Escaping of character literals like \t, \n, \v etc. ****

            // Finish the last text item:
            if (_index.Current != _index.LastEnd) _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.LastEnd, _index.Current));
                                
            // Is this a unicode escape sequence?
            if (_inputFormat[indexNextChar] == 'u')
            {
                // The next 4 characters must represent the unicode 
                _index.LastEnd = _index.SafeAdd(_index.Current, 6);
                _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.Current, _index.LastEnd));
            }
            else
            {
                // Next add the character literal INCLUDING the escape character, which LiteralText will expect
                _index.LastEnd = _index.SafeAdd(_index.Current, 2);
                _resultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, _resultFormat, _inputFormat, _index.Current, _index.LastEnd));
            }

            _index.Current = _index.SafeAdd(_index.Current, 1);
        }
    }

    /// <summary>
    /// Handles named formatters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ParseNamedFormatter()
    {
        var inputChar = _inputFormat[_index.Current];
        if (inputChar == _parserSettings.FormatterOptionsBeginChar)
        {
            var emptyName = _index.NamedFormatterStart == _index.Current;
            if (emptyName)
            {
                _index.NamedFormatterStart = PositionUndefined;
                return false;
            }

            // Note: This short-circuits the Parser.ParseFormat main loop
            ParseFormatOptions();
        }
        else if (inputChar == _parserSettings.FormatterOptionsEndChar || inputChar == _parserSettings.FormatterNameSeparator)
        {
            if (inputChar == _parserSettings.FormatterOptionsEndChar)
            {
                var hasOpeningParenthesis = _index.NamedFormatterOptionsStart != PositionUndefined;

                // ensure no trailing chars past ')'
                var nextCharIndex = _index.SafeAdd(_index.Current, 1);
                var nextCharIsValid = nextCharIndex < _inputFormat.Length &&
                                      (_inputFormat[nextCharIndex] == _parserSettings.FormatterNameSeparator || _inputFormat[nextCharIndex] == _parserSettings.PlaceholderEndChar);

                if (!hasOpeningParenthesis || !nextCharIsValid)
                {
                    _index.NamedFormatterStart = PositionUndefined;
                    return false;
                }

                _index.NamedFormatterOptionsEnd = _index.Current;

                if (_inputFormat[nextCharIndex] == _parserSettings.FormatterNameSeparator) _index.Current++; // Consume the ':'
            }

            var nameIsEmpty = _index.NamedFormatterStart == _index.Current;
            var missingClosingParenthesis =
                _index.NamedFormatterOptionsStart != PositionUndefined &&
                _index.NamedFormatterOptionsEnd == PositionUndefined;
            if (nameIsEmpty || missingClosingParenthesis)
            {
                _index.NamedFormatterStart = PositionUndefined;
                return false;
            }
                
            _index.LastEnd = _index.SafeAdd(_index.Current, 1);

            var parentPlaceholder = _resultFormat.ParentPlaceholder;

            if (_index.NamedFormatterOptionsStart == PositionUndefined)
            {
                if (parentPlaceholder != null)
                {
                    parentPlaceholder.FormatterNameStartIndex = _index.NamedFormatterStart;
                    parentPlaceholder.FormatterNameLength = _index.Current - _index.NamedFormatterStart;
                }
            }
            else
            {
                if (parentPlaceholder != null)
                {
                    parentPlaceholder.FormatterNameStartIndex = _index.NamedFormatterStart;
                    parentPlaceholder.FormatterNameLength = _index.NamedFormatterOptionsStart - _index.NamedFormatterStart;

                    // Save the formatter options with CharLiteralEscapeChar removed
                    parentPlaceholder.FormatterOptionsStartIndex = _index.NamedFormatterOptionsStart + 1;
                    parentPlaceholder.FormatterOptionsLength = _index.NamedFormatterOptionsEnd - (_index.NamedFormatterOptionsStart + 1);
                }
            }

            // Set start index to start of formatter option arguments,
            // with {0:default:N2} the start index is on the second colon
            _resultFormat.StartIndex = _index.LastEnd;

            _index.NamedFormatterStart = PositionUndefined;
        }

        return true;
    }

    /// <summary>
    /// Handles the selectors.
    /// </summary>
    /// <param name="currentPlaceholder"></param>
    /// <param name="parsingErrors"></param>
    /// <param name="nestedDepth"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseSelector(ref Placeholder? currentPlaceholder, ParsingErrors parsingErrors, ref int nestedDepth)
    {
        if (currentPlaceholder == null)
        {
            throw new ArgumentNullException(nameof(currentPlaceholder), $"Unexpected null reference");
        }

        var inputChar = _inputFormat[_index.Current];
        if (_operatorChars.Contains(inputChar) || _customOperatorChars.Contains(inputChar))
        {
            // Add the selector:
            if (_index.Current != _index.LastEnd) // if equal, we're already parsing a selector
            {
                currentPlaceholder.AddSelector(SelectorPool.Instance.Get().Initialize(Settings, currentPlaceholder, _inputFormat, _index.LastEnd, _index.Current, _index.Operator, _index.Selector));
                _index.Selector++;
                _index.Operator = _index.Current;
            }

            _index.LastEnd = _index.SafeAdd(_index.Current, 1);
        }
        else if (inputChar == _parserSettings.FormatterNameSeparator)
        {
            // Add the selector:
            AddLastSelector(ref currentPlaceholder, parsingErrors);
                
            // Start the format:
            var newFormat = FormatPool.Instance.Get().Initialize(Settings, currentPlaceholder, _index.Current + 1);
            currentPlaceholder.Format = newFormat;
            //FormatPool.Instance.Return(_resultFormat); // return to pool before reassigning
            _resultFormat = newFormat;
            currentPlaceholder = null;
            // named formatters will not be parsed with string.Format compatibility switched ON.
            // But this way we can handle e.g. Smart.Format("{Date:yyyy/MM/dd HH:mm:ss}") like string.Format
            _index.NamedFormatterStart = Settings.StringFormatCompatibility ? PositionUndefined : _index.LastEnd;
            _index.NamedFormatterOptionsStart = PositionUndefined;
            _index.NamedFormatterOptionsEnd = PositionUndefined;
        }
        else if (inputChar == _parserSettings.PlaceholderEndChar)
        {
            AddLastSelector(ref currentPlaceholder, parsingErrors);

            // End the placeholder with no format:
            nestedDepth--;
            currentPlaceholder.EndIndex = _index.SafeAdd(_index.Current, 1);
            //_resultFormat = currentPlaceholder.Parent;  // removed 2021-08-08: The parent always is the _resultFormat
            currentPlaceholder = null;
        }
        else
        {
            // Ensure the selector characters are valid:
            if (!_validSelectorChars.Contains(inputChar))
                parsingErrors.AddIssue(_resultFormat,
                    $"'0x{Convert.ToUInt32(inputChar):X}': " +
                    _parsingErrorText[ParsingError.InvalidCharactersInSelector],
                    _index.Current, _index.SafeAdd(_index.Current, 1));
        }
    }

    /// <summary>
    /// Adds a <see cref="Selector"/> to the current <see cref="Placeholder"/>
    /// because the current character ':' or '}' indicates the end of a selector.
    /// </summary>
    /// <param name="currentPlaceholder"></param>
    /// <param name="parsingErrors"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLastSelector(ref Placeholder currentPlaceholder, ParsingErrors parsingErrors)
    {
        if (_index.Current != _index.LastEnd ||
            currentPlaceholder.Selectors.Count > 0 && currentPlaceholder.Selectors[currentPlaceholder.Selectors.Count - 1].Length > 0 &&
            _index.Current - _index.Operator == 1 &&
            (_inputFormat[_index.Operator] == _parserSettings.ListIndexEndChar ||
             _inputFormat[_index.Operator] == _parserSettings.NullableOperator))
            currentPlaceholder.AddSelector(SelectorPool.Instance.Get().Initialize(Settings, currentPlaceholder, _inputFormat, _index.LastEnd, _index.Current,_index.Operator, _index.Selector));
        else if (_index.Operator != _index.Current) // the selector only contains illegal ("trailing") operator characters
            parsingErrors.AddIssue(_resultFormat,
                $"'0x{Convert.ToInt32(_inputFormat[_index.Operator]):X}': " +
                _parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                _index.Operator, _index.Current);
        _index.LastEnd = _index.SafeAdd(_index.Current, 1);
    }

    /// <summary>
    /// Parses all option characters.
    /// This short-circuits the Parser.ParseFormat main loop.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseFormatOptions()
    {
        _index.NamedFormatterOptionsStart = _index.Current;

        var nextChar = _inputFormat[_index.SafeAdd(_index.Current, 1)];
        // Handle empty options()
        if (_formatOptionsTerminatorChars.Contains(nextChar)) return;
            
        while (++_index.Current < _index.ObjectLength)
        {
            nextChar = _inputFormat[_index.SafeAdd(_index.Current, 1)];
            // Skip escaped terminating characters
            if (_inputFormat[_index.Current] == _parserSettings.CharLiteralEscapeChar &&
                (_formatOptionsTerminatorChars.Contains(nextChar) ||
                 EscapedLiteral.TryGetChar(nextChar, out _, true)))
            {
                _index.Current = _index.SafeAdd(_index.Current, 1);
                if (_formatOptionsTerminatorChars.Contains(
                        _inputFormat[_index.SafeAdd(_index.Current, 1)]))
                {
                    return;
                }

                continue;
            }

            // End of parsing options, when the NEXT character is terminating,
            // because this character will be handled in the Parser.ParseFormat main loop.
            if (_formatOptionsTerminatorChars.Contains(_inputFormat[_index.Current + 1]))
            {
                return;
            }
        }
    }

    /// <summary>
    /// 'style' and 'script' tags may contain curly or square braces, which SmartFormat uses to identify <see cref="Placeholder"/>s.
    /// Also, comments may contain any characters, which could mix up the parser.
    /// That's why the parser will treat all content inside 'style' and 'script' tags as <see cref="LiteralText"/>,
    /// if <see cref="ParserSettings.ParseInputAsHtml"/> is <see langword="true"/>.
    /// </summary>
    private void ParseHtmlTags()
    {
        // The tags we will be parsing with this method
        var scriptTagName = "script".AsSpan();
        var styleTagName = "style".AsSpan();
            
        // The first position is the start of an HTML tag
        // Move forward to the tag name
        _index.Current++;

        // Is it a script tag starting with <script
        var currentTagName = ReadOnlySpan<char>.Empty;
        if (_inputFormat.AsSpan(_index.Current).StartsWith(scriptTagName, StringComparison.InvariantCultureIgnoreCase))
        {
            currentTagName = scriptTagName;
            _index.Current += currentTagName.Length; // move behind tag name
        }

        // Is it a style tag starting with <style
        if (_inputFormat.AsSpan(_index.Current).StartsWith(styleTagName, StringComparison.InvariantCultureIgnoreCase))
        {
            currentTagName = styleTagName;
            _index.Current += currentTagName.Length; // move behind tag name
        }

        // Not a tag we should parse, stop processing
        if (currentTagName == ReadOnlySpan<char>.Empty) return;

        // Initialize quoting variables
        var isQuoting = false;
        var endQuoteChar = '\"';

        // Parse characters inside script or style tag
        while (true)
        {
            // done
            if (_index.Current >= _inputFormat.Length) return;

            #region ** Quoted characters (inside of single quotes or double quotes) **

            // text inside quotes (e.g.: const variable = "</script>"; could mix-up the parser                
            switch (isQuoting)
            {
                case false when _inputFormat[_index.Current] == '\'' || _inputFormat[_index.Current] == '\"':
                    isQuoting = !isQuoting;
                    endQuoteChar = _inputFormat[_index.Current]; // start and end quoting char must be equal
                    _index.Current++;
                    continue;
                case true when _inputFormat[_index.Current] == endQuoteChar:
                    isQuoting = !isQuoting;
                    _index.Current++;
                    continue;
                case true:
                    _index.Current++;
                    continue;
            }

            #endregion

            #region ** Parse script and style tags **

            // Is it a self-closing tag like <script/>
            if (_inputFormat[_index.Current] == '/' && _inputFormat[_index.Current + 1] == '>' && _inputFormat
                    .AsSpan(_index.Current - 1 - currentTagName.Length)
                    .StartsWith(currentTagName, StringComparison.InvariantCultureIgnoreCase))
            {
                _index.Current++;
                return;
            }

            // Is it the begin of </script> or </style>?
            if (_inputFormat[_index.Current] == '<' 
                && _inputFormat[_index.Current + 1] == '/' 
                && currentTagName != ReadOnlySpan<char>.Empty 
                && _inputFormat
                    .AsSpan(_index.Current + 2)
                    .StartsWith(currentTagName, StringComparison.InvariantCultureIgnoreCase))
            {
                _index.Current = _index.SafeAdd(_index.Current, 2 + currentTagName.Length); // move behind tag name
                if (_index.Current < _inputFormat.Length && _inputFormat[_index.Current] == '>') // closing char
                    return;
            }

            if (_inputFormat.Length > _index.Current)
            {
                _index.Current++;
                continue;
            }

            #endregion

            // We get here, when a script or style tag is not closed
            return;
        }
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
        private readonly Dictionary<ParsingError, string> _errors = new() {
            {ParsingError.TooManyClosingBraces, "Format string has too many closing braces"},
            {ParsingError.TrailingOperatorsInSelector, "There are illegal trailing operators in the selector"},
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
                        var parent = ph.Format ?? FormatPool.Instance.Get().Initialize(Settings, ph.BaseString);
                        currentResult.Items[i] = LiteralTextPool.Instance.Get().Initialize(Settings, parent, parent.BaseString, ph.StartIndex, ph.EndIndex);
                    }
                }
                return currentResult;
            case ParseErrorAction.Ignore:
                // Replace erroneous Placeholders with an empty LiteralText
                for (var i = 0; i < currentResult.Items.Count; i++)
                {
                    if (currentResult.Items[i] is Placeholder ph && parsingErrors.Issues.Any(errItem => errItem.Index >= currentResult.Items[i].StartIndex && errItem.Index <= currentResult.Items[i].EndIndex))
                    {
                        var parent = ph.Format ?? FormatPool.Instance.Get().Initialize(Settings, ph.BaseString);
                        currentResult.Items[i] = LiteralTextPool.Instance.Get().Initialize(Settings, parent, parent.BaseString, ph.StartIndex, ph.StartIndex);
                    }
                }
                return currentResult;
            case ParseErrorAction.OutputErrorInResult:
                var fmt = FormatPool.Instance.Get().Initialize(Settings, parsingErrors.Message, 0, parsingErrors.Message.Length);
                fmt.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, fmt, parsingErrors.Message, 0, parsingErrors.Message.Length));
                return fmt;
            default:
                throw new ArgumentException("Illegal type for ParsingErrors", parsingErrors);
        }
    }

    #endregion
}
