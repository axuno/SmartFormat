// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Represents a parsed format string.
/// Contains a list of <see cref="FormatItem" />s,
/// including <see cref="LiteralText" />s and <see cref="Placeholder" />s.
/// <para>Note: <see cref="Format"/> is <see cref="IDisposable"/>.</para>
/// </summary>
public sealed class Format : FormatItem, IDisposable
{
    private string? _toStringCache;
    private string? _literalTextCache;

    #region: Create, initialize, return to pool :

    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public Format()
    {
        // Inserted for clarity and documentation
    }

    /// <summary>
    /// Initializes the <see cref="Format"/> instance.
    /// </summary>
    /// <param name="smartSettings"></param>
    /// <param name="baseString"></param>
    /// <returns>This <see cref="Format"/> instance.</returns>
    public Format Initialize(SmartSettings smartSettings, string baseString)
    {
        base.Initialize(smartSettings, null, baseString, 0, baseString.Length);
        ParentPlaceholder = null;
        return this;
    }

    /// <summary>
    /// Initializes the instance of <see cref="Format"/>.
    /// </summary>
    /// <param name="smartSettings"></param>
    /// <param name="parent">The parent <see cref="Placeholder"/>.</param>
    /// <param name="startIndex">The start index within the format base string.</param>
    /// <returns>This <see cref="Format"/> instance.</returns>
    public Format Initialize(SmartSettings smartSettings, Placeholder parent, int startIndex)
    {
        base.Initialize(smartSettings, parent, parent.BaseString, startIndex, parent.EndIndex);
        ParentPlaceholder = parent;
        return this;
    }

    /// <summary>
    /// Initializes the instance of <see cref="Format"/>.
    /// </summary>
    /// <param name="smartSettings"></param>
    /// <param name="baseString">The base format string-</param>
    /// <param name="startIndex">The start index within the format base string.</param>
    /// <param name="endIndex">The end index within the format base string.</param>
    /// <returns>This <see cref="Format"/> instance.</returns>
    public Format Initialize(SmartSettings smartSettings, string baseString, int startIndex, int endIndex)
    {
        base.Initialize(smartSettings, null, baseString, startIndex, endIndex);
        ParentPlaceholder = null;
        return this;
    }

    /// <summary>
    /// Initializes the instance of <see cref="Format"/>.
    /// </summary>
    /// <param name="smartSettings"></param>
    /// <param name="baseString">The base format string-</param>
    /// <param name="startIndex">The start index within the format base string.</param>
    /// <param name="endIndex">The end index within the format base string.</param>
    /// <param name="hasNested"><see langword="true"/> if the nested formats exist.</param>
    /// <returns>This <see cref="Format"/> instance.</returns>
    public Format Initialize(SmartSettings smartSettings, string baseString, int startIndex, int endIndex, bool hasNested)
    {
        base.Initialize(smartSettings, null, baseString, startIndex, endIndex);
        ParentPlaceholder = null;
        HasNested = hasNested;

        return this;
    }

    /// <summary>
    /// Return items we own to the object pools.
    /// This method gets called by <see cref="FormatPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.
    /// </summary>
    public void ReturnToPool()
    {
        // Clear the format
        Clear();

        ParentPlaceholder = null;
        HasNested = false;

        // Return and clear FormatItems we own
        foreach (var item in Items)
        {
            if (ReferenceEquals(this, item.ParentFormatItem))
                ReturnFormatItemToPool(item);
        }
        Items.Clear();

        // Return and clear the list of SplitLists
        foreach (var splitList in _listOfSplitLists)
        {
            SplitListPool.Instance.Return(splitList);
        }
        _listOfSplitLists.Clear();
        // Items of _splitCache are returned via _listOfSplitLists
        _splitCache = null;
            
        _toStringCache = null;
        _literalTextCache = null;
    }

    #endregion

    #region: Fields and Properties :

    /// <summary>
    /// Gets the parent <see cref="Placeholder"/>.
    /// </summary>
    public Placeholder? ParentPlaceholder { get; internal set; }

    /// <summary>
    /// Gets the <see cref="List{T}"/> of <see cref="FormatItem"/>s.
    /// </summary>
    public List<FormatItem> Items { get; } = new();
        
    /// <summary>
    /// Returns <see langword="true"/>, if the <see cref="Format"/> is nested.
    /// </summary>
    public bool HasNested { get; internal set; }

    #endregion

    #region: Special Optimized Functions :

    #region: Substring :

    /// <summary>
    /// Gets a substring of the current <see cref="Format"/>.
    /// </summary>
    /// <param name="start">The start index of the substring.</param>
    /// <returns>The substring of the current <see cref="Format"/>.</returns>
    public Format Substring(int start)
    {
        return Substring(start, Length - start);
    }

    /// <summary>
    /// Gets a substring of the current <see cref="Format"/>.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns>The substring of the current <see cref="Format"/>.</returns>
    public Format Substring(int start, int length)
    {
        start = StartIndex + start;
        var end = start + length;
        ValidateArguments(start, length);
            
        // If startIndex and endIndex already match this item, we're done:
        if (start == StartIndex && end == EndIndex) return this;

        var substring = FormatPool.Instance.Get().Initialize(SmartSettings, BaseString, start, end);
        foreach (var item in Items)
        {
            if (item.EndIndex <= start)
                continue; // Skip first items
            if (end <= item.StartIndex)
                break; // Done

            var newItem = item;
            if (item is LiteralText)
            {
                // See if we need to slice the LiteralText
                if (start > item.StartIndex || item.EndIndex > end)
                    newItem = LiteralTextPool.Instance.Get().Initialize(substring.SmartSettings, substring,
                        substring.BaseString, Math.Max(start, item.StartIndex), Math.Min(end, item.EndIndex));
            }
            else
            {
                // item is a placeholder -- we can't split a placeholder though.
                substring.HasNested = true;
            }

            substring.Items.Add(newItem);
        }

        return substring;
    }

    private void ValidateArguments(int start, int length)
    {
        var end = start + length;
        if (start < StartIndex || start > EndIndex)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (end > EndIndex)
            throw new ArgumentOutOfRangeException(nameof(length));
    }

    #endregion

    #region: IndexOf :

    /// <summary>
    /// Searches the literal text for the search char.
    /// Does not search in nested placeholders.
    /// </summary>
    /// <param name="search"></param>
    public int IndexOf(char search)
    {
        return IndexOf(search, 0);
    }

    /// <summary>
    /// Searches the literal text for the search char.
    /// Does not search in nested placeholders.
    /// </summary>
    /// <param name="search"></param>
    /// <param name="start"></param>
    public int IndexOf(char search, int start)
    {
        start = StartIndex + start;
        foreach (var item in Items)
        {
            if (item.EndIndex < start || item is not LiteralText literalItem) continue;

            if (start < literalItem.StartIndex) start = literalItem.StartIndex;
            var literalIndex =
                literalItem.BaseString.IndexOf(search, start, literalItem.EndIndex - start);
            if (literalIndex != -1) return literalIndex - StartIndex;
        }

        return -1;
    }

    #endregion

    #region: FindAll :

    private List<int> FindAll(char search, int maxCount)
    {
        var results = ListPool<int>.Instance.Get();
        var index = 0;
        while (maxCount != 0)
        {
            index = IndexOf(search, index);
            if (index == -1) break;
            results.Add(index);
            index++;
            maxCount--;
        }

        return results;
    }

    #endregion

    #region: Split :

    // set the default
    private char _splitCacheChar = '\0'; 
    // Items of the _splitCache are returned to the pool using _listOfSplitLists
    private IList<Format>? _splitCache;
    private readonly List<SplitList> _listOfSplitLists = new();

    /// <summary>
    /// Splits the <see cref="Format"/> items by the given search character.
    /// </summary>
    /// <param name="search">The search character used to split.</param>
    /// <returns></returns>
    public IList<Format> Split(char search)
    {
        if (_splitCache == null || _splitCacheChar != search)
        {
            _splitCacheChar = search;
            _splitCache = Split(search, -1);
        }

        return _splitCache;
    }
    /// <summary>
    /// Splits the <see cref="Format"/> items by the given search character.
    /// </summary>
    /// <param name="search">e search character used to split.</param>
    /// <param name="maxCount">The maximum number of <see cref="IList"/> of type <see cref="Format"/>.</param>
    /// <returns>An <see cref="IList{T}"/> of <see cref="Format"/>s.</returns>
    public IList<Format> Split(char search, int maxCount)
    {
        var splits = FindAll(search, maxCount);
        var splitList = SplitListPool.Instance.Get().Initialize(this, splits);

        // Keep track of the split lists we create,
        // so that they can be returned to the object pool for later reuse.
        _listOfSplitLists.Add(splitList);
        return splitList;
    }

    #endregion

    #endregion

    #region: ToString :

    /// <summary>
    /// Retrieves the literal text contained in this format.
    /// Excludes escaped chars, and does not include the text
    /// of placeholders.
    /// </summary>
    /// <returns></returns>
    public string GetLiteralText()
    {
        if (_literalTextCache != null) return _literalTextCache;

        using var sb = Utilities.ZStringBuilderExtensions.CreateZStringBuilder(this);
        foreach (var item in Items)
            if (item is LiteralText literalItem) sb.Append(literalItem.AsSpan());

        _literalTextCache = sb.ToString();
        return _literalTextCache;
    }

    /// <summary>
    /// Reconstructs the format string, but doesn't include escaped chars
    /// and tries to reconstruct placeholders.
    /// </summary>
    public override string ToString()
    {
        if (_toStringCache != null) return _toStringCache;

        using var sb = Utilities.ZStringBuilderExtensions.CreateZStringBuilder(this);
        foreach (var item in Items) sb.Append(item.AsSpan());
        _toStringCache = sb.ToString();
        return _toStringCache;
    }

    #endregion

    private static void ReturnFormatItemToPool(FormatItem formatItem)
    {
        switch (formatItem)
        {
            case LiteralText literal:
                LiteralTextPool.Instance.Return(literal);
                break;

            case Format format:
                FormatPool.Instance.Return(format);
                break;

            case Placeholder placeholder:
                PlaceholderPool.Instance.Return(placeholder);
                break;

            case Selector selector:
                SelectorPool.Instance.Return(selector);
                break;

            default:
                throw new ArgumentException($"Unhandled type '{formatItem.GetType()}'", nameof(formatItem));
        }
    }

    #region : Disposable Pattern :

    /// <summary>
    /// Returns this instance to the object pool.
    /// <para>Do not use this instance after calling.</para>
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clearing this instance is done when returning it to the pool
            FormatPool.Instance.Return(this);
        }
    }

    /// <summary>
    /// Returns this instance to the object pool, which also clears all objects it owns.
    /// <para>Do not use this instance after calling <see cref="Dispose()"/></para>
    /// </summary>
    /// <code>
    /// // Example:
    /// var settings = new SmartSettings();
    /// using var formatParsed = new Parser(settings).ParseFormat("inputFormat");
    /// var formatter = new SmartFormatter(settings);
    /// for (var i = 0; i &lt; 10; i++)
    /// {
    ///    var result = formatter.Format(formatParsed, i);    
    /// }
    /// </code>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~Format()
    {
        Dispose(false);
    }

    #endregion
}
