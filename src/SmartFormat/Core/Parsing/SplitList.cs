// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Contains the results of a <see cref="Format"/> Split operation.
/// This allows deferred splitting of items.
/// </summary>
internal class SplitList : IList<Format>
{
    #region: Create, initialize, return to pool :

    private Format _format = InitializationObject.Format;
    private List<int> _splits = InitializationObject.IntegerList;
    private readonly List<Format?> _splitFormatCache = new();
        
    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public SplitList()
    {
        // Inserted for clarity and documentation
    }

    /// <summary>
    /// Initializes the instance of <see cref="SplitList"/>.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="splits"></param>
    /// <returns>This <see cref="Format"/> instance.</returns>
    public SplitList Initialize(Format format, List<int> splits)
    {
        _format = format;
        _splits = splits;

        // Pre-size the cache to match
        // If we have N splits, we have N+1 resulting formats (see Count property)
        for (var i = 0; i < Count; ++i)
            _splitFormatCache.Add(null);

        return this;
    }

    #endregion

    #region: Supported IList :

    ///<inheritdoc/>
    public Format this[int index]
    {
        get
        {
            // Can't use ArgumentOutOfRangeException.ThrowIfGreaterThan < net5.0
            if (index > _splits.Count) throw new ArgumentOutOfRangeException(nameof(index)); //NOSONAR

            if (_splits.Count == 0) return _format;

            // The cache was initialized with nulls, but not yet filled
            if (_splitFormatCache.Exists(c => c is null))
                throw new InvalidOperationException("SplitList cache was not filled.");

            // Return the result from the cache
            // which was created in method Format.Split(...)
            return _splitFormatCache[index]!;
        }
        set => throw new NotSupportedException();
    }

    internal void CreateSplitCache()
    {
        if (_splits.Count == 0) return;

        // If we have N splits, we have N+1 resulting formats
        for (var index = 0; index <= _splits.Count; index++)
        {
            Format f;
            if (index == 0)
            {
                f = _format.Substring(0, _splits[0]);
                _splitFormatCache[index] = f;
                continue;
            }

            if (index == _splits.Count)
            {
                f = _format.Substring(_splits[index - 1] + 1);
                _splitFormatCache[index] = f;
                continue;
            }

            // The format between the splits
            var startIndex = _splits[index - 1] + 1;
            f = _format.Substring(startIndex, _splits[index] - startIndex);
            _splitFormatCache[index] = f;
        }
    }

    /// <summary>
    /// Clears the <see cref="SplitList"/> item.
    /// <para>This method gets called by <see cref="SplitListPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.</para>
    /// </summary>
    public void Clear()
    {
        // Format and Splits were Initialize(...) arguments, we can safely reassign
        _format = InitializationObject.Format;
        _splits = InitializationObject.IntegerList;

        // Return the Formats we created to the pool
        for (var i = 0; i < _splitFormatCache.Count; i++)
        {
            if (_splitFormatCache[i] != null)
                FormatPool.Instance.Return(_splitFormatCache[i]!);
        }

        _splitFormatCache.Clear();
    }

    ///<inheritdoc/>
    public void CopyTo(Format[] array, int arrayIndex)
    {
        var length = _splits.Count + 1;
        for (var i = 0; i < length; i++) array[arrayIndex + i] = this[i];
    }

    ///<inheritdoc/>
    public int Count => _splits.Count + 1;

    ///<inheritdoc/>
    public bool IsReadOnly => true;

    ///<inheritdoc/>
    public IEnumerator<Format> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region: NotSupported IList Interface :

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public int IndexOf(Format item)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public void Insert(int index, Format item)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public void RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public void Add(Format item)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public bool Contains(Format item)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    public bool Remove(Format item)
    {
        throw new NotSupportedException();
    }

    #endregion
}
