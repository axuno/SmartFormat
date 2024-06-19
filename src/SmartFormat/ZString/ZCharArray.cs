//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;

namespace SmartFormat.ZString;

/// <summary>
/// A lightweight container that rents a char array as a buffer from an <see cref="ArrayPool{T}"/> and returns it when disposed.
/// It simplifies passing around the buffer without intermediate memory allocations.
/// <para/>
/// <b>Note that failing to dispose this struct after use will result in a memory leak.</b>
/// </summary>
public struct ZCharArray : IDisposable
{
    // The *default* max array length of ArrayPool<char>.Shared is 1,048,576.
    private readonly ArrayPool<char> _pool = ArrayPool<char>.Create(10_000_000, 50);
    private char[] _array;
    private int _currentLength = 0;

    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with a length of 1024.
    /// </summary>
    public ZCharArray()
    {
        _array = _pool.Rent(1024);
    }

    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with the specified length.
    /// </summary>
    /// <param name="length"></param>
    internal ZCharArray(int length)
    {
        _array = _pool.Rent(length);
    }

    /// <summary>
    /// Gets the <see cref="ReadOnlySpan{T}"/> of the array.
    /// </summary>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> of the array.</returns>
    public ReadOnlySpan<char> GetSpan() => _array.AsSpan(0, _currentLength);

    /// <summary>
    /// Gets the <see cref="Span{T}"/> of the array.
    /// </summary>
    internal Span<char> Span => _array.AsSpan(0, _currentLength);

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length => _currentLength;

    /// <summary>
    /// Gets the capacity of the array.
    /// </summary>
    public int Capacity => _array.Length;

    /// <summary>
    /// Resizes the array to the specified length, copying the existing elements if necessary.
    /// </summary>
    /// <param name="length"></param>
    internal void Resize(int length)
    {
        var newArray = _pool.Rent(length);
        if (_array != null)
        {
            Array.Copy(_array, newArray, Math.Min(_array.Length, length));
            _pool.Return(_array);
        }
        _array = newArray;
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data"></param>
    internal void Write(Span<char> data)
    {
        GrowIfNeeded(data.Length);
        data.CopyTo(_array.AsSpan(_currentLength, data.Length));
        _currentLength += data.Length;
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data"></param>
    internal void Write(ReadOnlySpan<char> data)
    {
        GrowIfNeeded(data.Length);
        data.CopyTo(_array.AsSpan(_currentLength, data.Length));
        _currentLength += data.Length;
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data"></param>
    internal void Write(string data)
    {
        GrowIfNeeded(data.Length);
        data.AsSpan().CopyTo(_array.AsSpan(_currentLength, data.Length));
        _currentLength += data.Length;
    }

    /// <summary>
    /// Writes the specified char to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="c"></param>
    internal void Write(char c)
    {
        GrowIfNeeded(1);
        _array[_currentLength++] = c;
    }

    /// <summary>
    /// Writes the specified char to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="count">The number of repetitions.</param>
    internal void Write(char c, int count)
    {
        GrowIfNeeded(count);

        for (var i = 0; i < count; i++)
        {
            _array[_currentLength++] = c;
        }
    }

    private void GrowIfNeeded(int dataLength)
    {
        var requiredLength = _currentLength + dataLength;
        if (requiredLength > _array.Length)
        {
            Resize(requiredLength * 2);
        }
    }

    /// <summary>
    /// Returns the string representation of the array.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return new string(_array, 0, _currentLength);
    }

    /// <summary>
    /// Disposes the array, returning it to the <see cref="ArrayPool{T}"/>.
    /// </summary>
    public void Dispose()
    {
        if (_array != null)
        {
            _pool.Return(_array);
        }
    }
}
