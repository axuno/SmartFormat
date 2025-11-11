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
    private SelectorFilterType _selectorCharFilter = SelectorFilterType.Alphanumeric;

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
    /// When <see cref="SelectorFilterType.Alphanumeric"/> (default) is set, an allowlist of selector characters is used.
    /// The allowlist contains alphanumeric characters (upper and lower case), plus '_' and '-'.
    /// On top, any custom selector characters added with <see cref="AddCustomSelectorChars"/> are included.
    /// <para/>
    /// When <see cref="SelectorFilterType.VisualUnicodeChars"/> is set, all Unicode characters are allowed in a selector,
    /// except 68 non-visual characters: Control Characters (U+0000–U+001F, U+007F), Format Characters (Category: Cf),
    /// Directional Formatting (Category: Cf), Invisible Separator, Common Combining Marks (Category: Mn),
    /// Whitespace Characters (non-glyph spacing).<br/>
    /// Excluded characters can be added back using <see cref="AddCustomSelectorChars"/>.
    /// <para/>
    /// {}[]()\.? are characters with special functions that are never allowed.
    /// <para/>
    /// Changing this setting clears any custom operator characters added with <see cref="AddCustomOperatorChars"/>.
    /// </summary>
    public SelectorFilterType SelectorCharFilter
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
    internal CharSet GetSelectorChars() => SelectorCharFilter == SelectorFilterType.Alphanumeric ? CreateAllowlist() : CreateBlocklist();

    private CharSet CreateBlocklist()
    {
        var chars = new CharSet {
            CharLiteralEscapeChar // avoid confusion with escape sequences
        };
        chars.IsAllowList = false;
        chars.AddRange(SelectorDelimitingChars.AsSpan());
        chars.AddRange(OperatorChars.AsSpan()); // no overlaps
        chars.AddRange(_customOperatorChars); // no overlaps
        chars.AddRange(NonVisualUnicodeCharacters.AsSpan());

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
    /// All 68 non-visual Unicode characters that are typically not used in selectors.
    /// </summary>
    internal static readonly char[] NonVisualUnicodeCharacters =
    [
        // Control Characters (U+0000–U+001F, U+007F)
        '\u0000', // NULL – string terminator
        '\u0001', // START OF HEADING – protocol control
        '\u0002', // START OF TEXT – protocol control
        '\u0003', // END OF TEXT – protocol control
        '\u0004', // END OF TRANSMISSION – protocol control
        '\u0005', // ENQUIRY – request for response
        '\u0006', // ACKNOWLEDGE – positive response
        '\u0007', // BELL – triggers alert
        '\u0008', // BACKSPACE – moves cursor back
        '\u0009', // CHARACTER TABULATION – horizontal tab
        '\u000A', // LINE FEED – line break
        '\u000B', // LINE TABULATION – vertical tab
        '\u000C', // FORM FEED – page break
        '\u000D', // CARRIAGE RETURN – return to line start
        '\u000E', // SHIFT OUT – alternate character set
        '\u000F', // SHIFT IN – return to standard set
        '\u0010', // DATA LINK ESCAPE – protocol framing
        '\u0011', // DEVICE CONTROL 1 – device-specific
        '\u0012', // DEVICE CONTROL 2 – device-specific
        '\u0013', // DEVICE CONTROL 3 – device-specific
        '\u0014', // DEVICE CONTROL 4 – device-specific
        '\u0015', // NEGATIVE ACKNOWLEDGE – error signal
        '\u0016', // SYNCHRONOUS IDLE – timing control
        '\u0017', // END OF TRANSMISSION BLOCK – block end
        '\u0018', // CANCEL – cancel transmission
        '\u0019', // END OF MEDIUM – physical medium end
        '\u001A', // SUBSTITUTE – invalid character
        '\u001B', // ESCAPE – escape sequence initiator
        '\u001C', // FILE SEPARATOR – data structuring
        '\u001D', // GROUP SEPARATOR – data structuring
        '\u001E', // RECORD SEPARATOR – data structuring
        '\u001F', // UNIT SEPARATOR – data structuring
        '\u007F', // DELETE – erase character

        // Format Characters (Category: Cf)
        '\u200B', // ZERO WIDTH SPACE – invisible space
        '\u200C', // ZERO WIDTH NON-JOINER – prevents ligature
        '\u200D', // ZERO WIDTH JOINER – forces ligature
        '\u2060', // WORD JOINER – prevents line break
        '\uFEFF', // ZERO WIDTH NO-BREAK SPACE – BOM or NBSP

        // Directional Formatting (Category: Cf)
        '\u202A', // LEFT-TO-RIGHT EMBEDDING – sets LTR context
        '\u202B', // RIGHT-TO-LEFT EMBEDDING – sets RTL context
        '\u202C', // POP DIRECTIONAL FORMATTING – ends override
        '\u202D', // LEFT-TO-RIGHT OVERRIDE – forces LTR rendering
        '\u202E', // RIGHT-TO-LEFT OVERRIDE – forces RTL rendering
        '\u2066', // LEFT-TO-RIGHT ISOLATE – isolates LTR segment
        '\u2067', // RIGHT-TO-LEFT ISOLATE – isolates RTL segment
        '\u2068', // FIRST STRONG ISOLATE – isolates with inferred direction
        '\u2069', // POP DIRECTIONAL ISOLATE – ends isolate

        // Invisible Separator
        '\u2063', // INVISIBLE SEPARATOR – semantic boundary marker

        // Common Combining Marks (Category: Mn)
        '\u0300', // COMBINING GRAVE ACCENT – diacritic (invisible alone)
        '\u0301', // COMBINING ACUTE ACCENT – diacritic (invisible alone)
        '\u0302', // COMBINING CIRCUMFLEX ACCENT – diacritic (invisible alone)
        '\u0308', // COMBINING DIAERESIS – diacritic (invisible alone)

        // Whitespace Characters (non-glyph spacing)
        '\u00A0', // NO-BREAK SPACE – non-breaking space
        '\u1680', // OGHAM SPACE MARK – special spacing
        '\u2000', // EN QUAD – fixed-width space
        '\u2001', // EM QUAD – fixed-width space
        '\u2002', // EN SPACE – fixed-width space
        '\u2003', // EM SPACE – fixed-width space
        '\u2004', // THREE-PER-EM SPACE – narrow space
        '\u2005', // FOUR-PER-EM SPACE – narrow space
        '\u2006', // SIX-PER-EM SPACE – narrow space
        '\u2007', // FIGURE SPACE – aligns digits
        '\u2008', // PUNCTUATION SPACE – aligns punctuation
        '\u2009', // THIN SPACE – narrow space
        '\u200A', // HAIR SPACE – ultra-thin space
        '\u202F', // NARROW NO-BREAK SPACE – narrow NBSP
        '\u205F', // MEDIUM MATHEMATICAL SPACE – math spacing
        '\u3000' // IDEOGRAPHIC SPACE – full-width CJK space
    ];

    /// <summary>
    /// Add a list of allowable selector characters on top of the default selector characters.
    /// <para/>
    /// When <see cref="SelectorCharFilter"/> is <see langword="true"/> (default), an allowlist of selector characters is used.
    /// The allowlist contains alphanumeric characters (upper and lower case), plus '_' and '-'.
    /// On top, any custom selector characters added with <see cref="AddCustomSelectorChars"/> are included.
    /// <para/>
    /// When <see cref="SelectorCharFilter"/> is <see langword="false"/>, all Unicode characters are allowed in a selector,
    /// except 68 non-visual characters. Excluded characters can be added back using <see cref="AddCustomSelectorChars"/>.
    /// <para/>
    /// Operator chars and selector chars must be different.
    /// </summary>
    public void AddCustomSelectorChars(IList<char> characters)
    {
        foreach (var c in characters)
        {
            // Explicitly disallow certain characters
            if (SelectorDelimitingChars.Contains(c) || c == CharLiteralEscapeChar
                || OperatorChars.Contains(c) || CustomOperatorChars.Contains(c))
                throw new ArgumentException($"Cannot add '{c}' as a custom selector character. It is disallowed or in use as an operator character.");

            if (NonVisualUnicodeCharacters.Contains(c))
                _customSelectorChars.Add(c);

            if (SelectorCharFilter == SelectorFilterType.Alphanumeric && !(StandardAllowlist.Contains(c) || _customSelectorChars.Contains(c))) _customSelectorChars.Add(c);
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
