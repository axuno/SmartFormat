// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Settings;

/// <summary>
/// Class for <see cref="Parser"/> settings.
/// Properties should be considered as 'init-only' like implemented in C# 9.
/// Any changes after passing settings as argument to CTORs may not have effect. 
/// </summary>
public class ParserSettings
{
    private readonly List<char> _customSelectorChars = [];
    private readonly List<char> _customOperatorChars = [];
    private FilterType _selectorCharFilter = FilterType.Allowlist;

    private const string StandardAllowlist = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

    /// <summary>
    /// Gets or sets the <see cref="ParseErrorAction" /> to use for the <see cref="Parser" />.
    /// The default is <see cref="ParseErrorAction.ThrowError"/>.
    /// </summary>
    public ParseErrorAction ErrorAction { get; set; } = ParseErrorAction.ThrowError;

    /// <summary>
    /// Gets a read-only list of the custom selector characters, which were set with <see cref="AddCustomSelectorChars"/>.
    /// </summary>
    internal List<char> CustomSelectorChars => _customSelectorChars;

    /// <summary>
    /// Gets a list of the custom operator characters, which were set with <see cref="AddCustomOperatorChars"/>.
    /// Contiguous operator characters are parsed as one operator (e.g. '?.').
    /// </summary>
    internal List<char> CustomOperatorChars => _customOperatorChars;

    /// <summary>
    /// When <see cref="FilterType.Allowlist"/> (default) is set, an allowlist of selector characters is used.
    /// The allowlist contains alphanumeric characters (upper and lower case), plus '_' and '-'.
    /// On top, any custom selector characters added with <see cref="AddCustomSelectorChars"/> are included.
    /// <para/>
    /// When <see cref="FilterType.Blocklist"/>, all Unicode characters are allowed in a selector,
    /// except control characters (ASCII 0-31 and 127). Excluded control characters can be added back
    /// using <see cref="AddCustomSelectorChars"/>.
    /// <para/>
    /// Changing this setting clears any custom operator characters added with <see cref="AddCustomOperatorChars"/>.
    /// </summary>
    public FilterType SelectorCharFilter
    {
        get
        {
            return _selectorCharFilter;
        }
        set
        {
            _selectorCharFilter = value;
            _customOperatorChars.Clear();
        }
    }

    /// <summary>
    /// The list of characters for a selector.
    /// This can be an allowlist, which contains explicitly allowed characters,
    /// or a blocklist, when all Unicode characters are allowed, except those from the blocklist.
    /// </summary>
    internal CharSet GetSelectorChars() => SelectorCharFilter == FilterType.Allowlist ? CreateAllowlist() : CreateBlocklist();

    private CharSet CreateBlocklist()
    {
        var chars = new CharSet {
            CharLiteralEscapeChar // avoid confusion with escape sequences
        };
        chars.IsAllowList = false;
        chars.AddRange(SelectorDelimitingChars.AsSpan());
        chars.AddRange(OperatorChars.AsSpan()); // no overlaps
        chars.AddRange(_customOperatorChars); // no overlaps
        // Hard to visualize and debug, disallow by default - can be added back as custom selector chars
        chars.AddRange(ControlChars());

        // Remove characters used as custom selector chars from the blocklist
        foreach (var c in _customSelectorChars) chars.Remove(c);
        return chars;
    }

    private CharSet CreateAllowlist()
    {
        var chars = new CharSet {IsAllowList = true};
        chars.AddRange(StandardAllowlist.AsSpan());
        // Add characters used as custom selector chars to the allowlist
        chars.AddRange(_customSelectorChars);
        return chars;
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
    /// <para>Experimental.</para>
    /// Gets or sets, whether the input format should be interpreted as HTML.
    /// If <see langword="true"/>, the <see cref="Parser"/> will parse all content
    /// inside &lt;script&gt; and &lt;style&gt; tags as <see cref="LiteralText"/>. All other tags may contain <see cref="Placeholder"/>s.
    /// This is because &lt;script&gt; and &lt;style&gt; tags may contain curly or square braces, that interfere with <c>SmartFormat</c>.
    /// Default is <see langword="false"/>.
    /// <para>
    /// Best results can only be expected with clean HTML: balanced opening and closing tags, single and double quotes.
    /// Also, do not use angle brackets, single and double quotes in script or style comments.
    /// <c>SmartFormat</c> is not a fully-fledged HTML parser. If this is required, use <c>AngleSharp</c> or <c>HtmlAgilityPack</c>.
    /// </para>
    /// </summary>
    public bool ParseInputAsHtml { get; set; } = false;

    /// <summary>
    /// The character literal escape character for <see cref="PlaceholderBeginChar"/> and <see cref="PlaceholderEndChar"/>,
    /// but also others like for \t (TAB), \n (NEW LINE), \\ (BACKSLASH) and others defined in <see cref="EscapedLiteral"/>.
    /// </summary>
    internal char CharLiteralEscapeChar { get; set; } = '\\';

    /// <summary>
    /// The character which separates the formatter name (if any exists) from other parts of the placeholder.
    /// E.g.: {Variable:FormatterName:argument} or {Variable:FormatterName}
    /// </summary>
    internal const char FormatterNameSeparator = ':';

    /// <summary>
    /// The character which separates the selector for alignment. <c>E.g.: Smart.Format("Name: {name,10}")</c>
    /// </summary>
    internal const char AlignmentOperator = ',';

    /// <summary>
    /// The character which separates two or more selectors <c>E.g.: "First.Second.Third"</c>
    /// </summary>
    internal const char SelectorOperator = '.';

    /// <summary>
    /// The character which flags the selector as <see langword="nullable"/>.
    /// The character after <see cref="NullableOperator"/> must be the <see cref="SelectorOperator"/>.
    /// <c>E.g.: "First?.Second"</c>
    /// </summary>
    internal const char NullableOperator = '?';

    /// <summary>
    /// Gets the character indicating the start of a <see cref="Placeholder"/>.
    /// </summary>
    internal const char PlaceholderBeginChar = '{';

    /// <summary>
    /// Gets the character indicating the end of a <see cref="Placeholder"/>.
    /// </summary>
    internal const char PlaceholderEndChar = '}';

    /// <summary>
    /// Gets the character indicating the beginning of formatter options.
    /// </summary>
    internal const char FormatterOptionsBeginChar = '(';

    /// <summary>
    /// Gets the character indicating the end of formatter options.
    /// </summary>
    internal const char FormatterOptionsEndChar = ')';

    /// <summary>
    /// Gets the character indicating the beginning of a list index, like in '{Numbers[0]}'
    /// </summary>
    internal const char ListIndexBeginChar = '[';

    /// <summary>
    /// Gets the character indicating the end of a list index, like in '{Numbers[0]}'
    /// </summary>
    internal const char ListIndexEndChar = ']';

    /// <summary>
    /// Characters which terminate parsing of format options.
    /// To use them as options, they must be escaped (preceded) by the <see cref="CharLiteralEscapeChar"/>.
    /// </summary>
    internal static readonly char[] FormatOptionsTerminatorChars =
    [
        FormatterNameSeparator, FormatterOptionsBeginChar, FormatterOptionsEndChar, PlaceholderBeginChar,
        PlaceholderEndChar
    ];

    /// <summary>
    /// The standard operator characters.
    /// Contiguous operator characters are parsed as one operator (e.g. '?.').
    /// </summary>
    internal static readonly char[] OperatorChars =
    [
        SelectorOperator, NullableOperator, AlignmentOperator, ListIndexBeginChar, ListIndexEndChar
    ];

    /// <summary>
    /// The list of characters which are delimiting a selector.
    /// </summary>
    internal static readonly char[] SelectorDelimitingChars =
    [
        FormatterNameSeparator,
        PlaceholderBeginChar, PlaceholderEndChar,
        FormatterOptionsBeginChar, FormatterOptionsEndChar
    ];

    /// <summary>
    /// Gets the set of control characters (ASCII 0-31 and 127).
    /// </summary>
    internal static IEnumerable<char> ControlChars()
    {
        for (var i = 0; i <= 31; i++) yield return (char) i;
        yield return (char) 127; // delete character
    }

    /// <summary>
    /// Add a list of allowable selector characters on top of the default selector characters.
    /// <para/>
    /// When <see cref="SelectorCharFilter"/> is <see langword="true"/> (default), an allowlist of selector characters is used.
    /// The allowlist contains alphanumeric characters (upper and lower case), plus '_' and '-'.
    /// On top, any custom selector characters added with <see cref="AddCustomSelectorChars"/> are included.
    /// <para/>
    /// When <see cref="SelectorCharFilter"/> is <see langword="false"/>, all Unicode characters are allowed in a selector,
    /// except control characters (ASCII 0-31 and 127). Excluded control characters can be added back
    /// using <see cref="AddCustomSelectorChars"/>.
    /// <para/>
    /// Operator chars and selector chars must be different.
    /// </summary>
    public void AddCustomSelectorChars(IList<char> characters)
    {
        var controlChars = ControlChars().ToList();

        foreach (var c in characters)
        {
            // Explicitly disallow certain characters
            if (SelectorDelimitingChars.Contains(c) || c == CharLiteralEscapeChar
                || OperatorChars.Contains(c) || CustomOperatorChars.Contains(c))
                throw new ArgumentException($"Cannot add '{c}' as a custom selector character. It is disallowed or in use as an operator character.");

            if (controlChars.Contains(c))
                _customSelectorChars.Add(c);

            if (SelectorCharFilter == FilterType.Allowlist && !(StandardAllowlist.Contains(c) || _customSelectorChars.Contains(c))) _customSelectorChars.Add(c);
        }
    }

    /// <summary>
    /// Add a list of allowable operator characters on top of the standard <see cref="OperatorChars"/> setting.
    /// Operator chars and selector chars must be different.
    ///  </summary>
    public void AddCustomOperatorChars(IList<char> characters)
    {
        foreach (var c in characters)
        {
            if (SelectorDelimitingChars.Contains(c) || CustomSelectorChars.Contains(c))
                throw new ArgumentException($"Cannot add '{c}' as a custom operator character. It is disallowed or in use as a selector.");

            if (!OperatorChars.Contains(c) && !_customOperatorChars.Contains(c))
                _customOperatorChars.Add(c);
        }
    }
}
