// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Represents the literal text that is found
/// in a parsed format string.
/// </summary>
public class LiteralText : FormatItem
{
    private string? _toStringCache;

    #region: Create, initialize, return to pool :

    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public LiteralText()
    {
        // Inserted for clarity and documentation
    }

    /// <summary>
    /// Initializes the <see cref="LiteralText"/> instance, representing the literal text that is found in a parsed format string.
    /// </summary>
    /// <param name="smartSettings">The <see cref="SmartSettings"/>.</param>
    /// <param name="parent">The parent <see cref="FormatItem"/></param>
    /// <param name="baseString">The reference to the parsed format string.</param>
    /// <param name="startIndex">The start index of the <see cref="LiteralText"/> in the format string.</param>
    /// <param name="endIndex">The end index of the <see cref="LiteralText"/> in the format string.</param>
    /// <returns>The <see cref="LiteralText"/> instance, representing the literal text that is found in a parsed format string.</returns>
    public new LiteralText Initialize(SmartSettings smartSettings, FormatItem parent, string baseString, int startIndex, int endIndex)
    {
        base.Initialize(smartSettings, parent, baseString, startIndex, endIndex);
        return this;
    }

    #endregion

    /// <summary>
    /// Get the string representation of the <see cref="LiteralText"/>, with escaped characters converted.
    /// Note: The <see cref="Parser"/> puts each escaped character of an input string
    /// into its own <see cref="LiteralText"/> item.
    /// </summary>
    /// <returns>The string representation of the <see cref="LiteralText"/>, with escaped characters converted.</returns>
    public override string ToString()
    {
        if (_toStringCache != null) return _toStringCache;
        if (Length == 0) _toStringCache = string.Empty;

        // The buffer is only 1 character
        _toStringCache = AsSpan().ToString();

        return _toStringCache;
    }

    /// <summary>
    /// Get the string representation of the <see cref="LiteralText"/>, with escaped characters converted.
    /// Note: The <see cref="Parser"/> puts each escaped character of an input string
    /// into its own <see cref="LiteralText"/> item.
    /// </summary>
    /// <returns>The string representation of the <see cref="LiteralText"/>, with escaped characters converted.</returns>
    public override ReadOnlySpan<char> AsSpan()
    {
        if (Length == 0) return ReadOnlySpan<char>.Empty;

        var span = BaseString.AsSpan(StartIndex, Length);

        return SmartSettings.Parser.ConvertCharacterStringLiterals switch
        {
            // Convert escaped literals, e.g. \n, \t, or \u2022
            // Each escaped char goes into its own LiteralText object.
            true when span[0] == SmartSettings.Parser.CharLiteralEscapeChar
                => EscapedLiteral.UnEscapeCharLiterals(
                    SmartSettings.Parser.CharLiteralEscapeChar,
                    span,
                    false,
                    true,
                    // Each escaped literal has just 1 character in size.
                    new char[1]),
            // Special case: Escaped escape char, i.e. "\\", when ConvertCharacterStringLiterals is false.
            false when span.Length == 2 && span[0] == span[1] && span[0] == SmartSettings.Parser.CharLiteralEscapeChar
                => span.Slice(1), // simplify instead of calling UnEscapeCharLiterals
            // No conversion
            _ => span
        };
    }

    /// <summary>
    /// Clears the <see cref="LiteralText"/> item.
    /// <para>This method gets called by <see cref="LiteralTextPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.</para>
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _toStringCache = null;
    }
}
