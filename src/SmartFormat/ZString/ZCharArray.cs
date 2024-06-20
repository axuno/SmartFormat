﻿//
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
/// Note that failing to dispose this struct after use will result in a memory leak.
/// <para/>
/// <see cref="ZCharArray"/> is thread-safe.
/// </summary>
public struct ZCharArray : IDisposable
{
    // ArrayPool is thread-safe
    private static readonly ArrayPool<char>
        Pool = ArrayPool<char>.Create(MaxBufferCapacity, 50);

    private static readonly object Locker = new();

    private char[]? _bufferArray;
    private int _currentLength;

    /// <summary>
    /// The default capacity of the array.
    /// </summary>
    public const int DefaultBufferCapacity = 1_000_000;

    /// <summary>
    /// The maximum capacity of the array.
    /// </summary>
    public const int MaxBufferCapacity = 10_000_000;

    /// <summary>
    /// The maximum size of the stack-allocated buffer.
    /// </summary>
    public const int StackAllocCharBufferSizeLimit = 256;

    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with a length of <see cref="DefaultBufferCapacity"/>>.
    /// </summary>
    public ZCharArray() : this(DefaultBufferCapacity)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with the specified length.
    /// </summary>
    /// <param name="length">The length of the array.</param>
    public ZCharArray(int length)
    {
        lock (Locker)
        {
            _bufferArray = Pool.Rent(length);
            _currentLength = 0;
        }
    }

    /// <summary>
    /// Gets the <see cref="ReadOnlySpan{T}"/> of the array.
    /// </summary>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> of the array.</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public ReadOnlySpan<char> GetSpan()
    {
        lock(Locker)
        {
            ThrowIfDisposed();
            return _bufferArray.AsSpan(0, _currentLength);
        }
    }

    /// <summary>
    /// Gets the <see cref="Span{T}"/> of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    internal Span<char> Span
    {
        get
        {
            lock (Locker)
            {
                ThrowIfDisposed();
                return _bufferArray.AsSpan(0, _currentLength);
            }
        }
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public int Length
    {
        get
        {
            lock (Locker)
            {
                ThrowIfDisposed();
                return _currentLength;
            }
        }
    }

    /// <summary>
    /// Gets the capacity of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public int Capacity
    {
        get
        {
            lock (Locker)
            {
                ThrowIfDisposed();
                return _bufferArray!.Length;
            }
        }
    }

    /// <summary>
    /// Sets the current length of the array to zero.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Reset()
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            _currentLength = 0;
        }
    }

    /// <summary>
    /// Grows the array to the specified length,
    /// copying the existing elements if necessary.
    /// </summary>
    /// <param name="length">The new length of the array.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    private void Grow(int length)
    {
        var newArray = Pool.Rent(length);
        Array.Copy(_bufferArray!, newArray, Math.Min(_bufferArray!.Length, length));
        Pool.Return(_bufferArray);
        _bufferArray = newArray;
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(Span<char> data)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            GrowBufferIfNeeded(data.Length);
            data.CopyTo(_bufferArray!.AsSpan(_currentLength, data.Length));
            _currentLength += data.Length;
        }
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(ReadOnlySpan<char> data)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            GrowBufferIfNeeded(data.Length);
            data.CopyTo(_bufferArray!.AsSpan(_currentLength, data.Length));
            _currentLength += data.Length;
        }
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(string data)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            GrowBufferIfNeeded(data.Length);
            data.AsSpan().CopyTo(_bufferArray!.AsSpan(_currentLength, data.Length));
            _currentLength += data.Length;
        }
    }

    /// <summary>
    /// Writes the specified char to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="c">The char to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(char c)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            GrowBufferIfNeeded(1);
            _bufferArray![_currentLength++] = c;
        }
    }

    /// <summary>
    /// Writes the specified character to the array a specified number of times.
    /// </summary>
    /// <param name="c">The char to write.</param>
    /// <param name="count">The number of repetitions.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(char c, int count)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            GrowBufferIfNeeded(count);

            for (var i = 0; i < count; i++)
            {
                _bufferArray![_currentLength++] = c;
            }
        }
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Writes the <see cref="ISpanFormattable"/> data to the array.
    /// For more information, see <see cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider)"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="format"></param>
    /// <param name="provider"></param>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(ISpanFormattable data, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        lock (Locker)
        {
            ThrowIfDisposed();

            Span<char> stackBuffer = stackalloc char[StackAllocCharBufferSizeLimit];
            if (data.TryFormat(stackBuffer, out var written, format, provider))
            {
                GrowBufferIfNeeded(written);
                stackBuffer.Slice(0, written).CopyTo(_bufferArray!.AsSpan(_currentLength));
                _currentLength += written;
                return;
            }
        }

        throw new FormatException("The data could not be formatted.");
    }
#endif

    /// <summary>
    /// Writes the <see cref="IFormattable"/> data to the array.
    /// For more information, see <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="format"></param>
    /// <param name="provider"></param>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(IFormattable data, string format, IFormatProvider provider)
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            var formatted = data.ToString(format, provider).AsSpan();
            GrowBufferIfNeeded(formatted.Length);
            Write(formatted);
        }
    }

    private void GrowBufferIfNeeded(int dataLength)
    {
        var requiredLength = _currentLength + dataLength;
        if (requiredLength <= Capacity) return;

        Grow(requiredLength * 2); // Does nothing if the buffer is already large enough
    }

    /// <summary>
    /// Returns <see langword="true"/> if the array has been disposed.
    /// </summary>
    public bool IsDisposed => _bufferArray is null;

    private void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(ZCharArray));
    }

    /// <summary>
    /// Returns the string representation of the array.
    /// </summary>
    /// <returns>The string representation of the array.</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public override string ToString()
    {
        lock (Locker)
        {
            ThrowIfDisposed();
            return new string(_bufferArray!, 0, _currentLength);
        }
    }

    /// <summary>
    /// Disposes the array, returning it to the <see cref="ArrayPool{T}"/>.
    /// </summary>
    public void Dispose()
    {
        lock (Locker)
        {
            if (IsDisposed) return;

            Pool.Return(_bufferArray!);
            _bufferArray = null;
        }
    }
}
