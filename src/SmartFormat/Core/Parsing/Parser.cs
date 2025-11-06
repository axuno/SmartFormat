// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Parses a format string.
/// <para/>
/// <para>
/// <b>Thread-safety</b>:<br/>
/// The <see cref="ParseFormat"/> method is stateless w.r.t. the instance
/// is safe for concurrent calls provided <see cref="SmartSettings"/> are not concurrently mutated,
/// and <see cref="SmartSettings.IsThreadSafeMode"/> is <see langword="true"/>.
/// </para>
/// </summary>
public class Parser
{
    private const int PositionUndefined = ParserState.IndexContainer.PositionUndefined;
    private readonly ParsingErrorText _parsingErrorText = new ();

    #region: Settings :

    /// <summary>
    /// Gets or sets the <see cref="SmartSettings" /> for Smart.Format
    /// </summary>
    public SmartSettings Settings { get; }

    // Cache method results from settings
    private readonly List<char> _operatorChars;
    private readonly List<char> _customOperatorChars;
    private readonly ParserSettings _parserSettings;
    private readonly HashSet<char> _disallowedSelectorChars;
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
        _operatorChars = ParserSettings.OperatorChars;
        _customOperatorChars = _parserSettings.CustomOperatorChars;
        _formatOptionsTerminatorChars = ParserSettings.FormatOptionsTerminatorChars;

        _disallowedSelectorChars = _parserSettings.DisallowedSelectorChars();
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

    #region: Parsing and ParseContext :

    /// <summary>
    /// Defines the current state of the parser within the main loop.
    /// </summary>
    private enum ParseContext
    {
        /// <summary>
        /// Top-level literal text or inside a placeholder's Format section
        /// </summary>
        LiteralText,
        /// <summary>
        /// Inside the selector / header portion of a placeholder.
        /// It is only the header (selectors + optional formatter name start),
        /// not the entire placeholder including nested formats
        /// </summary>
        SelectorHeader
    }

    /// <summary>
    /// Parses a format string. This method is thread-safe.
    /// <para/>
    /// <para>
    /// <b>Thread-safety</b>:<br/>
    /// The <see cref="ParseFormat"/> method is stateless w.r.t. the instance
    /// is safe for concurrent calls provided <see cref="SmartSettings"/> are not concurrently mutated,
    /// and <see cref="SmartSettings.IsThreadSafeMode"/> is <see langword="true"/>.
    /// </para>
    /// </summary>
    /// <param name="inputFormat"></param>
    /// <returns>The <see cref="Format"/> for the parsed string.</returns>
    public Format ParseFormat(string inputFormat)
    {
        using var statePool = ParserStatePool.Instance.Get(out var state);
        // The result format must not be returned to the pool by the parser
        state.Initialize(inputFormat, FormatPool.Instance.Get().Initialize(Settings, inputFormat));

        var indexContainer = state.Index;

        var parsingErrors = ParsingErrorsPool.Instance.Get().Initialize(state.ResultFormat);

        // Context variables
        var currentContext = ParseContext.LiteralText;
        Placeholder? currentPlaceholder = null;
        var nestedDepth = 0;

        for (indexContainer.Current = 0; indexContainer.Current < state.InputFormat.Length; indexContainer.Current++)
        {
            var inputChar = state.InputFormat[indexContainer.Current];

            switch (currentContext)
            {
                case ParseContext.SelectorHeader:
                    ProcessSelector(inputChar, state, parsingErrors, ref currentContext, ref currentPlaceholder,
                        ref nestedDepth);
                    break;
                case ParseContext.LiteralText:
                    ProcessLiteralText(inputChar, state, parsingErrors, ref currentContext, ref currentPlaceholder,
                        ref nestedDepth);
                    break;
            }
        }

        // Finalize parsing and handle any remaining issues
        FinalizeParsing(state, parsingErrors, currentPlaceholder);

        // Check for any parsing errors:
        if (parsingErrors.HasIssues)
        {
            OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(parsingErrors, Settings.Parser.ErrorAction == ParseErrorAction.ThrowError));
            return HandleParsingErrors(parsingErrors, state.ResultFormat);
        }

        ParsingErrorsPool.Instance.Return(parsingErrors);
        return state.ResultFormat;
    }

    /// <summary>
    /// Handles parsing when the current state is LiteralText.
    /// This method is responsible for identifying the start of placeholders, handling escaped characters,
    /// and managing the closing of nested placeholders.
    /// </summary>
    private void ProcessLiteralText(char inputChar, ParserState state, ParsingErrors parsingErrors,
        ref ParseContext currentContext, ref Placeholder? currentPlaceholder, ref int nestedDepth)
    {
        // We're parsing literal text with an HTML tag
        if (_parserSettings.ParseInputAsHtml && inputChar == '<')
        {
            ParseHtmlTags(state);
            return;
        }

        if (inputChar == ParserSettings.PlaceholderBeginChar)
        {
            AddLiteralCharsParsedBefore(state);
            if (EscapeLikeStringFormat(ParserSettings.PlaceholderBeginChar, state)) return;

            // Context transition
            CreateNewPlaceholder(ref nestedDepth, state, out currentPlaceholder);
            currentContext = ParseContext.SelectorHeader;
        }
        else if (inputChar == ParserSettings.PlaceholderEndChar)
        {
            AddLiteralCharsParsedBefore(state);
            if (EscapeLikeStringFormat(ParserSettings.PlaceholderEndChar, state)) return;
            if (HasProcessedTooManyClosingBraces(parsingErrors, state)) return;

            // End of a nested placeholder's Format.
            FinishPlaceholderFormat(ref nestedDepth, state);
        }
        else if (inputChar == _parserSettings.CharLiteralEscapeChar &&
                 (_parserSettings.ConvertCharacterStringLiterals || !Settings.StringFormatCompatibility))
        {
            ParseAlternativeEscaping(state);
        }
        else if (state.Index.NamedFormatterStart != PositionUndefined && !ParseNamedFormatter(state))
        {
            // continue the loop
        }
    }

    /// <summary>
    /// Handles parsing when the current context is <see cref="ParseContext.SelectorHeader"/>.
    /// This method is responsible for parsing selectors, operators, and identifying the start
    /// of a format specifier ':' or the end of the placeholder '}'.
    /// </summary>
    private void ProcessSelector(char inputChar, ParserState state, ParsingErrors parsingErrors,
        ref ParseContext currentContext, ref Placeholder? currentPlaceholder, ref int nestedDepth)
    {
        if (currentPlaceholder == null)
        {
            throw new InvalidOperationException($"Invalid parser context: {nameof(ProcessSelector)} called with a null {nameof(currentPlaceholder)}.");
        }

        if (_operatorChars.Contains(inputChar) || _customOperatorChars.Contains(inputChar))
        {
            // Add the selector segment before the operator:
            if (state.Index.Current != state.Index.LastEnd)
            {
                currentPlaceholder.AddSelector(SelectorPool.Instance.Get().Initialize(Settings, currentPlaceholder, state.InputFormat, state.Index.LastEnd, state.Index.Current, state.Index.Operator, state.Index.Selector));
                state.Index.Selector++;
                state.Index.Operator = state.Index.Current;
            }
            state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 1);
        }
        else if (inputChar == ParserSettings.FormatterNameSeparator)
        {
            AddLastSelector(ref currentPlaceholder, state, parsingErrors);

            // Start the format section of the placeholder.
            var newFormat = FormatPool.Instance.Get().Initialize(Settings, currentPlaceholder, state.Index.Current + 1);
            currentPlaceholder.Format = newFormat;
            state.ResultFormat = newFormat;
            currentPlaceholder = null; // We are now parsing the format, not the selectors.
            state.Index.NamedFormatterStart = Settings.StringFormatCompatibility ? PositionUndefined : state.Index.LastEnd;
            state.Index.NamedFormatterOptionsStart = PositionUndefined;
            state.Index.NamedFormatterOptionsEnd = PositionUndefined;

            // We are now parsing the literal text *inside* the placeholder's format.
            currentContext = ParseContext.LiteralText;
        }
        else if (inputChar == ParserSettings.PlaceholderEndChar)
        {
            AddLastSelector(ref currentPlaceholder, state, parsingErrors);

            // End the placeholder with no format.
            nestedDepth--;
            currentPlaceholder.EndIndex = state.Index.SafeAdd(state.Index.Current, 1);
            currentPlaceholder = null;

            // Switch Context
            currentContext = ParseContext.LiteralText;
        }
        else
        {
            // Ensure the selector characters are valid:
            if (_disallowedSelectorChars.Contains(inputChar))
                parsingErrors.AddIssue(state.ResultFormat,
                    $"'0x{Convert.ToUInt32(inputChar):X}': " +
                    _parsingErrorText[ParsingError.InvalidCharactersInSelector],
                    state.Index.Current, state.Index.SafeAdd(state.Index.Current, 1));
        }
    }

    /// <summary>
    /// Finalizes parsing at the end of the input string.
    /// </summary>
    private void FinalizeParsing(ParserState state, ParsingErrors parsingErrors, Placeholder? currentPlaceholder)
    {
        // 1. Is the last item a placeholder that is not finished yet?
        if (state.ResultFormat.ParentPlaceholder != null || currentPlaceholder != null)
        {
            parsingErrors.AddIssue(state.ResultFormat, _parsingErrorText[ParsingError.MissingClosingBrace],
                state.InputFormat.Length, state.InputFormat.Length);
            state.ResultFormat.EndIndex = state.InputFormat.Length;
        }
        // 2. The last item must be a literal, so add it if necessary
        else if (state.Index.LastEnd != state.InputFormat.Length)
        {
            state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat, state.InputFormat,
                state.Index.LastEnd, state.InputFormat.Length));
        }

        // Unwind any unclosed nested formats (due to missing closing braces)
        while (state.ResultFormat.ParentPlaceholder != null)
        {
            state.ResultFormat = state.ResultFormat.ParentPlaceholder.Parent;
            state.ResultFormat.EndIndex = state.InputFormat.Length;
        }
    }

    /// <summary>
    /// Adds a new <see cref="LiteralText"/> item, if there are characters left to process.
    /// Sets <see cref="ParserState.IndexContainer.LastEnd"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLiteralCharsParsedBefore(ParserState state)
    {
        // Finish the last text item:
        if (state.Index.Current != state.Index.LastEnd)
        {
            state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat, state.InputFormat, state.Index.LastEnd, state.Index.Current));
        }

        state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 1);
    }

    /// <summary>
    /// Checks, whether we are on top level and still there was a closing brace.
    /// In this case we add the redundant brace as literal and create a <see cref="ParsingError"/>.
    /// </summary>
    /// <param name="parsingErrors">The list of <see cref="ParsingErrors"/>.</param>
    /// <param name="state"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasProcessedTooManyClosingBraces(ParsingErrors parsingErrors, ParserState state)
    {
        if (state.ResultFormat.ParentPlaceholder != null) return false;

        // Don't swallow-up redundant closing braces, but treat them as literals
        state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat,
            state.InputFormat, state.Index.Current, state.Index.Current + 1));

        parsingErrors.AddIssue(state.ResultFormat, _parsingErrorText[ParsingError.TooManyClosingBraces],
            state.Index.Current, state.Index.Current + 1);

        return true;
    }

    /// <summary>
    /// In case of string.Format compatibility, we escape the brace
    /// and treat it as a literal character.
    /// </summary>
    /// <param name="brace">The brace { or } to process.</param>
    /// <param name="state"></param>
    /// <returns><see langword="true">, if escaping was done.</see></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EscapeLikeStringFormat(char brace, ParserState state)
    {
        if (!Settings.StringFormatCompatibility) return false;

        if (state.Index.LastEnd < state.InputFormat.Length && state.InputFormat[state.Index.LastEnd] == brace)
        {
            state.Index.Current = state.Index.SafeAdd(state.Index.Current, 1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new <see cref="Placeholder"/>, adds it to the current format and sets values in <see cref="ParserState.IndexContainer"/>.
    /// </summary>
    /// <param name="nestedDepth">The counter for nesting levels.</param>
    /// <param name="state"></param>
    /// <param name="newPlaceholder"></param>
    /// <returns>The new <see cref="Placeholder"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CreateNewPlaceholder(ref int nestedDepth, ParserState state, out Placeholder newPlaceholder)
    {
        nestedDepth++;
        newPlaceholder = PlaceholderPool.Instance.Get().Initialize(state.ResultFormat, state.Index.Current, nestedDepth);
        state.ResultFormat.Items.Add(newPlaceholder);
        state.Index.Operator = state.Index.SafeAdd(state.Index.Current, 1);
        state.Index.Selector = 0;
        state.Index.NamedFormatterStart = PositionUndefined;
    }

    /// <summary>
    /// Finishes the current placeholder <see cref="Format"/>.
    /// </summary>
    /// <param name="nestedDepth">The counter for nesting levels.</param>
    /// <param name="state"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FinishPlaceholderFormat(ref int nestedDepth, ParserState state)
    {
        System.Diagnostics.Debug.Assert(state.ResultFormat.ParentPlaceholder is not null);

        nestedDepth--;
        state.ResultFormat.EndIndex = state.Index.Current;
        state.ResultFormat.ParentPlaceholder!.EndIndex = state.Index.SafeAdd(state.Index.Current, 1);
        state.ResultFormat = state.ResultFormat.ParentPlaceholder.Parent;
        state.Index.NamedFormatterStart = state.Index.NamedFormatterOptionsStart = state.Index.NamedFormatterOptionsEnd = PositionUndefined;
    }

    /// <summary>
    /// Processes the character if alternative escaping is used.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseAlternativeEscaping(ParserState state)
    {
        // See what is the next character
        var indexNextChar = state.Index.SafeAdd(state.Index.Current, 1);
        if (indexNextChar >= state.InputFormat.Length)
            throw new ArgumentException($"Unrecognized escape sequence at the end of the literal");

        // **** Alternative brace escaping with { or } following the escape character ****
        if (state.InputFormat[indexNextChar] == ParserSettings.PlaceholderBeginChar ||
            state.InputFormat[indexNextChar] == ParserSettings.PlaceholderEndChar)
        {
            // Finish the last text item:
            if (state.Index.Current != state.Index.LastEnd)
                state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat,
                    state.InputFormat, state.Index.LastEnd, state.Index.Current));
            state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 1);

            state.Index.Current++;
        }
        else
        {
            // **** Escaping of character literals like \t, \n, \v etc. ****

            // Finish the last text item:
            if (state.Index.Current != state.Index.LastEnd)
                state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat,
                    state.InputFormat, state.Index.LastEnd, state.Index.Current));

            // Is this a unicode escape sequence?
            if (state.InputFormat[indexNextChar] == 'u')
            {
                // The next 4 characters must represent the unicode 
                state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 6);
            }
            else
            {
                // Next add the character literal INCLUDING the escape character, which LiteralText will expect
                state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 2);
            }

            state.ResultFormat.Items.Add(LiteralTextPool.Instance.Get().Initialize(Settings, state.ResultFormat, state.InputFormat, state.Index.Current, state.Index.LastEnd));
            state.Index.Current = state.Index.SafeAdd(state.Index.Current, 1);
        }
    }

    /// <summary>
    /// Handles named formatters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ParseNamedFormatter(ParserState state)
    {
        var inputChar = state.InputFormat[state.Index.Current];
        if (inputChar == ParserSettings.FormatterOptionsBeginChar)
        {
            var emptyName = state.Index.NamedFormatterStart == state.Index.Current;
            if (emptyName)
            {
                state.Index.NamedFormatterStart = PositionUndefined;
                return false;
            }

            // Note: This short-circuits the Parser.ParseFormat main loop
            ParseFormatOptions(state);
        }
        else if (inputChar == ParserSettings.FormatterOptionsEndChar || inputChar == ParserSettings.FormatterNameSeparator)
        {
            if (inputChar == ParserSettings.FormatterOptionsEndChar)
            {
                var hasOpeningParenthesis = state.Index.NamedFormatterOptionsStart != PositionUndefined;

                // ensure no trailing chars past ')'
                var nextCharIndex = state.Index.SafeAdd(state.Index.Current, 1);
                var nextCharIsValid = nextCharIndex < state.InputFormat.Length &&
                                      (state.InputFormat[nextCharIndex] == ParserSettings.FormatterNameSeparator || state.InputFormat[nextCharIndex] == ParserSettings.PlaceholderEndChar);

                if (!hasOpeningParenthesis || !nextCharIsValid)
                {
                    state.Index.NamedFormatterStart = PositionUndefined;
                    return false;
                }

                state.Index.NamedFormatterOptionsEnd = state.Index.Current;

                if (state.InputFormat[nextCharIndex] == ParserSettings.FormatterNameSeparator) state.Index.Current++;
            }

            var nameIsEmpty = state.Index.NamedFormatterStart == state.Index.Current;
            var missingClosingParenthesis =
                state.Index.NamedFormatterOptionsStart != PositionUndefined &&
                state.Index.NamedFormatterOptionsEnd == PositionUndefined;
            if (nameIsEmpty || missingClosingParenthesis)
            {
                state.Index.NamedFormatterStart = PositionUndefined;
                return false;
            }

            state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 1);

            var parentPlaceholder = state.ResultFormat.ParentPlaceholder;

            if (state.Index.NamedFormatterOptionsStart == PositionUndefined)
            {
                if (parentPlaceholder != null)
                {
                    parentPlaceholder.FormatterNameStartIndex = state.Index.NamedFormatterStart;
                    parentPlaceholder.FormatterNameLength = state.Index.Current - state.Index.NamedFormatterStart;
                }
            }
            else
            {
                if (parentPlaceholder != null)
                {
                    parentPlaceholder.FormatterNameStartIndex = state.Index.NamedFormatterStart;
                    parentPlaceholder.FormatterNameLength = state.Index.NamedFormatterOptionsStart - state.Index.NamedFormatterStart;

                    // Save the formatter options with CharLiteralEscapeChar removed
                    parentPlaceholder.FormatterOptionsStartIndex = state.Index.NamedFormatterOptionsStart + 1;
                    parentPlaceholder.FormatterOptionsLength = state.Index.NamedFormatterOptionsEnd - (state.Index.NamedFormatterOptionsStart + 1);
                }
            }

            // Set start index to start of formatter option arguments,
            // with {0:default:N2} the start index is on the second colon
            state.ResultFormat.StartIndex = state.Index.LastEnd;

            state.Index.NamedFormatterStart = PositionUndefined;
        }

        return true;
    }

    /// <summary>
    /// Adds a <see cref="Selector"/> to the current <see cref="Placeholder"/>
    /// because the current character ':' or '}' indicates the end of a selector.
    /// </summary>
    /// <param name="currentPlaceholder"></param>
    /// <param name="state"></param>
    /// <param name="parsingErrors"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLastSelector(ref Placeholder currentPlaceholder, ParserState state, ParsingErrors parsingErrors)
    {
        if (state.Index.Current != state.Index.LastEnd ||
            currentPlaceholder.Selectors.Count > 0 && currentPlaceholder.Selectors[currentPlaceholder.Selectors.Count - 1].Length > 0 &&
            state.Index.Current - state.Index.Operator == 1 &&
            (state.InputFormat[state.Index.Operator] == ParserSettings.ListIndexEndChar ||
             state.InputFormat[state.Index.Operator] == ParserSettings.NullableOperator))
            currentPlaceholder.AddSelector(SelectorPool.Instance.Get().Initialize(Settings, currentPlaceholder, state.InputFormat, state.Index.LastEnd, state.Index.Current, state.Index.Operator, state.Index.Selector));
        else if (state.Index.Operator != state.Index.Current)
            parsingErrors.AddIssue(state.ResultFormat,
                $"'0x{Convert.ToInt32(state.InputFormat[state.Index.Operator]):X}': " +
                _parsingErrorText[ParsingError.TrailingOperatorsInSelector],
                state.Index.Operator, state.Index.Current);
        state.Index.LastEnd = state.Index.SafeAdd(state.Index.Current, 1);
    }

    /// <summary>
    /// Parses all option characters.
    /// This short-circuits the Parser.ParseFormat main loop.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseFormatOptions(ParserState state)
    {
        state.Index.NamedFormatterOptionsStart = state.Index.Current;

        var nextChar = state.InputFormat[state.Index.SafeAdd(state.Index.Current, 1)];
        // Handle empty options()
        if (_formatOptionsTerminatorChars.Contains(nextChar)) return;

        while (++state.Index.Current < state.Index.ObjectLength)
        {
            nextChar = state.InputFormat[state.Index.SafeAdd(state.Index.Current, 1)];
            // Skip escaped terminating characters
            if (state.InputFormat[state.Index.Current] == _parserSettings.CharLiteralEscapeChar &&
                (_formatOptionsTerminatorChars.Contains(nextChar) ||
                 EscapedLiteral.TryGetChar(nextChar, out _, true, false)))
            {
                state.Index.Current = state.Index.SafeAdd(state.Index.Current, 1);
                if (_formatOptionsTerminatorChars.Contains(
                        state.InputFormat[state.Index.SafeAdd(state.Index.Current, 1)]))
                {
                    return;
                }

                continue;
            }

            // End of parsing options, when the NEXT character is terminating,
            // because this character will be handled in the Parser.ParseFormat main loop.
            if (_formatOptionsTerminatorChars.Contains(state.InputFormat[state.Index.Current + 1]))
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
    private static void ParseHtmlTags(ParserState state)
    {
        // The tags we will be parsing with this method
        var scriptTagName = "script".AsSpan();
        var styleTagName = "style".AsSpan();

        // The first position is the start of an HTML tag
        // Move forward to the tag name
        state.Index.Current++;

        // Is it a script tag starting with <script
        var currentTagName = ReadOnlySpan<char>.Empty;
        if (state.InputFormat.AsSpan(state.Index.Current).StartsWith(scriptTagName, StringComparison.InvariantCultureIgnoreCase))
        {
            currentTagName = scriptTagName;
            state.Index.Current += currentTagName.Length;
        }

        // Is it a style tag starting with <style
        if (state.InputFormat.AsSpan(state.Index.Current).StartsWith(styleTagName, StringComparison.InvariantCultureIgnoreCase))
        {
            currentTagName = styleTagName;
            state.Index.Current += currentTagName.Length;
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
            if (state.Index.Current >= state.InputFormat.Length) return;

            #region ** Quoted characters (inside of single quotes or double quotes) **

            // text inside quotes (e.g.: const variable = "</script>"; could mix-up the parser
            switch (isQuoting)
            {
                case false when state.InputFormat[state.Index.Current] == '\'' || state.InputFormat[state.Index.Current] == '\"':
                    isQuoting = !isQuoting;
                    endQuoteChar = state.InputFormat[state.Index.Current];  // start and end quoting char must be equal
                    state.Index.Current++;
                    continue;
                case true when state.InputFormat[state.Index.Current] == endQuoteChar:
                    isQuoting = !isQuoting;
                    state.Index.Current++;
                    continue;
                case true:
                    state.Index.Current++;
                    continue;
            }

            #endregion

            #region ** Parse script and style tags **

            // Is it a self-closing tag like <script/>
            if (IsSelfClosingTag(state, currentTagName))
            {
                state.Index.Current++;
                return;
            }

            // Is it the begin of </script> or </style>?
            if (IsBeginOfClosingTag(state, currentTagName))
            {
                state.Index.Current = state.Index.SafeAdd(state.Index.Current, 2 + currentTagName.Length);  // move behind tag name
                if (state.Index.Current < state.InputFormat.Length && state.InputFormat[state.Index.Current] == '>')   // closing char
                    return;
            }

            if (state.InputFormat.Length > state.Index.Current)
            {
                state.Index.Current++;
                continue;
            }

            #endregion

            // We get here, when a script or style tag is not closed

            return;
        }
    }

    private static bool IsSelfClosingTag(ParserState state, ReadOnlySpan<char> currentTagName)
    {
        return state.InputFormat[state.Index.Current] == '/' && state.InputFormat[state.Index.Current + 1] == '>' &&
            state.InputFormat
                .AsSpan(state.Index.Current - 1 - currentTagName.Length)
                .StartsWith(currentTagName, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool IsBeginOfClosingTag(ParserState state, ReadOnlySpan<char> currentTagName)
    {
        return state.InputFormat[state.Index.Current] == '<'
               && state.InputFormat[state.Index.Current + 1] == '/'
               && currentTagName != ReadOnlySpan<char>.Empty
               && state.InputFormat
                   .AsSpan(state.Index.Current + 2)
                   .StartsWith(currentTagName, StringComparison.InvariantCultureIgnoreCase);
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
                    if (currentResult.Items[i] is Placeholder ph && parsingErrors.Issues.Exists(errItem => errItem.Index >= currentResult.Items[i].StartIndex && errItem.Index <= currentResult.Items[i].EndIndex))
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
                    if (currentResult.Items[i] is Placeholder ph && parsingErrors.Issues.Exists(errItem => errItem.Index >= currentResult.Items[i].StartIndex && errItem.Index <= currentResult.Items[i].EndIndex))
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
