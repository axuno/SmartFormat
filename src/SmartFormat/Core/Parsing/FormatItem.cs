// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Base class that represents a substring
/// of text from a parsed format string.
/// </summary>
public abstract class FormatItem
{
    private string? _toStringCache;

    /// <summary>
    /// Gets the base format string.
    /// </summary>
    public string BaseString { get; protected set; } = string.Empty;
        
    /// <summary>
    /// The end index is pointing to ONE POSITION AFTER the last character of item.
    /// </summary>
    /// <example>
    /// Format string: {0}{1}ABC
    /// Index:         012345678
    /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
    /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
    /// </example>
    public int EndIndex { get; set; }
        
    /// <summary>
    /// The start index is pointing to the first character of item.
    /// </summary>
    /// <example>
    /// Format string: {0}{1}ABC
    /// Index:         012345678
    /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
    /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
    /// </example>
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets the result of <see cref="EndIndex"/> minus <see cref="StartIndex"/>.
    /// </summary>
    public int Length => EndIndex - StartIndex;

    /// <summary>
    /// The settings for formatter and parser.
    /// </summary>
    public SmartSettings SmartSettings { get; protected set; } = InitializationObject.SmartSettings;

    /// <summary>
    /// The parent <see cref="FormatItem"/> of this instance, <see langword="null"/> if not parent exists.
    /// </summary>
    public FormatItem? ParentFormatItem { get; private set; }

    /// <summary>
    /// Initializes the <see cref="FormatItem"/> or the derived class.
    /// </summary>
    /// <param name="smartSettings"></param>
    /// <param name="parent">The parent <see cref="FormatItem"/> or <see langword="null"/>.</param>
    /// <param name="baseString">The base format string.</param>
    /// <param name="startIndex">The start index of the <see cref="FormatItem"/> within the base format string.</param>
    /// <param name="endIndex">The end index of the <see cref="FormatItem"/> within the base format string.</param>
    protected virtual void Initialize(SmartSettings smartSettings, FormatItem? parent, string baseString, int startIndex, int endIndex)
    {
        ParentFormatItem = parent;
        SmartSettings = smartSettings;
        BaseString = baseString;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    /// <summary>
    /// Clears the <see cref="FormatItem"/> or the derived class.
    /// </summary>
    public virtual void Clear()
    {
        _toStringCache = null;
        BaseString = string.Empty;
        EndIndex = 0;
        StartIndex = 0;
        SmartSettings = InitializationObject.SmartSettings;
        ParentFormatItem = null;
    }

    /// <summary>
    /// Retrieves the raw text that this item represents.
    /// </summary>
    public string RawText => ToString();

    /// <summary>
    /// Gets the string representation of this <see cref="FormatItem"/>.
    /// </summary>
    /// <returns>The string representation of this <see cref="FormatItem"/></returns>
    public override string ToString() => _toStringCache ??= AsSpan().ToString();

    /// <summary>
    /// Gets the <see cref="ReadOnlySpan{T}"/> representation of this <see cref="FormatItem"/>.
    /// </summary>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> representation of this <see cref="FormatItem"/></returns>
    public virtual ReadOnlySpan<char> AsSpan() => EndIndex <= StartIndex
        ? BaseString.AsSpan(StartIndex)
        : BaseString.AsSpan(StartIndex, Length);
}