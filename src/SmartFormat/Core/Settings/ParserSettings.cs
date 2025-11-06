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

    /// <summary>
    /// Gets or sets the <see cref="ParseErrorAction" /> to use for the <see cref="Parser" />.
    /// The default is <see cref="ParseErrorAction.ThrowError"/>.
    /// </summary>
    public ParseErrorAction ErrorAction { get; set; } = ParseErrorAction.ThrowError;

    /// <summary>
    /// Gets a read-only list of the custom selector characters, which were set with <see cref="AddCustomSelectorChars"/>.
    /// </summary>
    internal List<char> CustomSelectorChars() => _customSelectorChars;

    /// <summary>
    /// The list of characters which are delimiting a selector.
    /// </summary>
    internal HashSet<char> SelectorDelimitingChars() =>
    [
        FormatterNameSeparator,
        PlaceholderBeginChar, PlaceholderEndChar,
        FormatterOptionsBeginChar, FormatterOptionsEndChar
    ];

    /// <summary>
    /// Gets the set of control characters (ASCII 0-31 and 127).
    /// </summary>
    internal IEnumerable<char> ControlChars()
    {
        for (var i = 0; i <= 31; i++) yield return (char) i;
        yield return (char) 127; // delete character
    }

    /// <summary>
    /// The list of characters which are disallowed in a selector.
    /// </summary>
    internal HashSet<char> DisallowedSelectorChars()
    {
        var chars = new HashSet<char> {
            CharLiteralEscapeChar // avoid confusion with escape sequences
        };
        chars.UnionWith(SelectorDelimitingChars());
        chars.UnionWith(OperatorChars()); // no overlaps
        chars.UnionWith(CustomOperatorChars()); // no overlaps
        // Hard to visualize and debug, disallow by default - can be added back as custom selector chars
        chars.UnionWith(ControlChars());

        // Remove characters used as custom selector chars.
        // Note: Using chars.ExceptWith(_customOperatorChars) would not remove char 0.
        foreach (var c in _customSelectorChars) chars.Remove(c); 
        return chars;
    }

    /// <summary>
    /// Gets a list of the custom operator characters, which were set with <see cref="AddCustomOperatorChars"/>.
    /// Contiguous operator characters are parsed as one operator (e.g. '?.').
    /// </summary>
    internal List<char> CustomOperatorChars() => _customOperatorChars;

    /// <summary>
    /// Add a list of allowable selector characters on top of the default selector characters.
    /// This can be useful to add control characters (ASCII 0-31 and 127) that are excluded by default.
    /// Operator chars and selector chars must be different.
    /// </summary>
    public void AddCustomSelectorChars(IList<char> characters)
    {
        var delimitingChars = SelectorDelimitingChars();
        var controlChars = ControlChars().ToList();
        var operatorChars = OperatorChars();
        var customOperatorChars = CustomOperatorChars();

        foreach (var c in characters)
        {
            // Explicitly disallow certain characters
            if (delimitingChars.Contains(c) || c == CharLiteralEscapeChar
                || operatorChars.Contains(c) || customOperatorChars.Contains(c))
                throw new ArgumentException($"Cannot add '{c}' as a custom selector character. It is disallowed or in use as an operator character.");

            if (controlChars.Contains(c))
                _customSelectorChars.Add(c);
        }
    }

    /// <summary>
    /// Add a list of allowable operator characters on top of the standard <see cref="OperatorChars"/> setting.
    /// Operator chars and selector chars must be different.
    ///  </summary>
    public void AddCustomOperatorChars(IList<char> characters)
    {
        var selectorDelimitingChars = SelectorDelimitingChars();
        var customSelectorChars = CustomSelectorChars();
        var operatorChars = OperatorChars();
        var customOperatorChars = CustomOperatorChars();

        foreach (var c in characters)
        {
            if (selectorDelimitingChars.Contains(c) || customSelectorChars.Contains(c))
                throw new ArgumentException($"Cannot add '{c}' as a custom operator character. It is disallowed or in use as a selector.");

            if (!operatorChars.Contains(c) && !customOperatorChars.Contains(c))
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
    internal char FormatterNameSeparator => ':';

    /// <summary>
    /// The standard operator characters.
    /// Contiguous operator characters are parsed as one operator (e.g. '?.').
    /// </summary>
    internal List<char> OperatorChars() =>
    [
        SelectorOperator, NullableOperator, AlignmentOperator, ListIndexBeginChar, ListIndexEndChar
    ];

    /// <summary>
    /// The character which separates the selector for alignment. <c>E.g.: Smart.Format("Name: {name,10}")</c>
    /// </summary>
    internal char AlignmentOperator => ',';

    /// <summary>
    /// The character which separates two or more selectors <c>E.g.: "First.Second.Third"</c>
    /// </summary>
    internal char SelectorOperator => '.';

    /// <summary>
    /// The character which flags the selector as <see langword="nullable"/>.
    /// The character after <see cref="NullableOperator"/> must be the <see cref="SelectorOperator"/>.
    /// <c>E.g.: "First?.Second"</c>
    /// </summary>
    internal char NullableOperator => '?';

    /// <summary>
    /// Gets the character indicating the start of a <see cref="Placeholder"/>.
    /// </summary>
    internal char PlaceholderBeginChar => '{';

    /// <summary>
    /// Gets the character indicating the end of a <see cref="Placeholder"/>.
    /// </summary>
    internal char PlaceholderEndChar => '}';

    /// <summary>
    /// Gets the character indicating the beginning of formatter options.
    /// </summary>
    internal char FormatterOptionsBeginChar => '(';

    /// <summary>
    /// Gets the character indicating the end of formatter options.
    /// </summary>
    internal char FormatterOptionsEndChar => ')';

    /// <summary>
    /// Gets the character indicating the beginning of a list index, like in '{Numbers[0]}'
    /// </summary>
    internal char ListIndexBeginChar => '[';

    /// <summary>
    /// Gets the character indicating the end of a list index, like in '{Numbers[0]}'
    /// </summary>
    internal char ListIndexEndChar => ']';

    /// <summary>
    /// Characters which terminate parsing of format options.
    /// To use them as options, they must be escaped (preceded) by the <see cref="CharLiteralEscapeChar"/>.
    /// </summary>
    internal List<char> FormatOptionsTerminatorChars() =>
    [
        FormatterNameSeparator, FormatterOptionsBeginChar, FormatterOptionsEndChar, PlaceholderBeginChar,
        PlaceholderEndChar
    ];
}
