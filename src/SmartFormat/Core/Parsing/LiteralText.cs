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

        // The buffer is only for 1 character - each escaped char goes into its own LiteralText
        return SmartSettings.Parser.ConvertCharacterStringLiterals &&
               BaseString.AsSpan(StartIndex)[0] == SmartSettings.Parser.CharLiteralEscapeChar
            ? EscapedLiteral.UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar,
                BaseString.AsSpan(StartIndex, Length),
                false, new char[1])
            : BaseString.AsSpan(StartIndex, Length);
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