// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

namespace SmartFormat.Core.Settings;

/// <summary>
/// Determines the filter type for allowed or disallowed characters.
/// </summary>
public enum SelectorFilterType    
{
    /// <summary>
    /// Use a list of characters that are allowed. The default characters are<br/>
    /// alphanumeric characters (upper and lower case), plus '_' and '-'.<br/>
    /// </summary>
    Alphanumeric,

    /// <summary>
    /// All Unicode characters are allowed in a selector, except 68 non-visual characters:
    /// Control Characters (U+0000–U+001F, U+007F), Format Characters (Category: Cf),
    /// Directional Formatting (Category: Cf), Invisible Separator, Common Combining Marks (Category: Mn),
    /// Whitespace Characters (non-glyph spacing).
    /// <para/>
    /// {}[]()\.? are characters with special functions that are never allowed.
    /// </summary>
    VisualUnicodeChars
}
