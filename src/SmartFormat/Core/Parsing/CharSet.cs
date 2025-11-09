// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartFormat.Core.Parsing;

/// <summary>
/// Represents a set of characters that supports efficient storage and lookup
/// for both ASCII and non-ASCII characters.
/// </summary>
/// <remarks>
/// The <see cref="CharSet"/> class is optimized for handling ASCII characters using a bitmap
/// representation, while non-ASCII characters are stored in a separate collection.
/// <para/>
/// The class provides methods to add characters individually or in bulk, remove characters, check for containment, and enumerate all
/// characters in the set. ASCII characters are enumerated first in numerical order, followed by non-ASCII characters in
/// no guaranteed order.
/// <para/>
/// This class is not thread-safe.
/// </remarks>
internal class CharSet : IEnumerable<char>
{
    private const int ASCII_LIMIT = 128;
    private const int BITS_PER_UINT = 32;
    private const int BITMAP_LENGTH = ASCII_LIMIT / BITS_PER_UINT;

    private readonly uint[] _asciiBitmap = new uint[BITMAP_LENGTH];
    private readonly HashSet<char> _nonAsciiChars = [];

    /// <summary>
    /// Gets or sets a value indicating whether the list is
    /// an allowlist (<see langword="true"/>, default) or a blocklist (<see langword="false"/>).
    /// </summary>
    public bool IsAllowList { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharSet"/> class that is empty.
    /// </summary>
    public CharSet()
    {}

    /// <summary>
    /// Initializes a new instance of the <see cref="CharSet"/> class that contains the characters
    /// from the specified read-only span.
    /// </summary>
    /// <param name="characters">The read-only span containing characters to add to the set.</param>
    public CharSet(ReadOnlySpan<char> characters)
    {
        AddRange(characters);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharSet"/> class that contains the characters
    /// from the specified collection.
    /// </summary>
    /// <param name="characters">The collection of characters to add to the set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="characters"/> is null.</exception>
    public CharSet(IEnumerable<char> characters)
    {
        AddRange(characters);
    }

    /// <summary>
    /// Adds all characters from the specified read-only span to the current set.
    /// Only adds characters that aren't already present in the set.
    /// </summary>
    /// <param name="characters">The read-only span containing characters to add.</param>
    public void AddRange(ReadOnlySpan<char> characters)
    {
        foreach (var ch in characters)
            Add(ch);
    }

    /// <summary>
    /// Adds all characters from the specified collection to the current set.
    /// Only adds characters that aren't already present in the set.
    /// </summary>
    /// <param name="characters">The collection of characters to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="characters"/> is null.</exception>
    public void AddRange(IEnumerable<char> characters)
    {
        foreach (var ch in characters)
            Add(ch);
    }

    /// <summary>
    /// Adds the specified character to the current set.
    /// Only adds a character that isn't already present in the set.
    /// </summary>
    /// <param name="c">The character to add.</param>
    public void Add(char c)
    {
        if (c < ASCII_LIMIT)
            _asciiBitmap[c / BITS_PER_UINT] |= 1u << c % BITS_PER_UINT;
        else
            _nonAsciiChars.Add(c);
    }

    /// <summary>
    /// Removes the specified character from the current set.
    /// </summary>
    /// <param name="c">The character to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the character was successfully found and removed; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(char c)
    {
        if (c < ASCII_LIMIT)
        {
            ref var bitmap = ref _asciiBitmap[c / BITS_PER_UINT];
            var mask = 1u << c % BITS_PER_UINT;

            if ((bitmap & mask) == 0) return false;

            bitmap &= ~mask;
            return true;
        }

        return _nonAsciiChars.Remove(c);
    }

    /// <summary>
    /// Determines whether the current set contains the specified character.
    /// </summary>
    /// <param name="c">The character to locate in the set.</param>
    /// <returns>
    /// <see langword="true"/> if the set contains the specified character; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(char c)
    {
        if (c < ASCII_LIMIT)
            return (_asciiBitmap[c / BITS_PER_UINT] & 1u << c % BITS_PER_UINT) != 0;

        return _nonAsciiChars.Contains(c);
    }

    /// <summary>
    /// Removes all characters from the current set.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_asciiBitmap, 0, _asciiBitmap.Length);
        _nonAsciiChars.Clear();
    }

    /// <summary>
    /// Gets the number of characters contained in the set.
    /// </summary>
    /// <value>The number of characters in the set.</value>
    public int Count
    {
        get
        {
            var count = 0;

            // Count ASCII characters using bit population count
            foreach (var segment in _asciiBitmap)
                count += BitCount(segment);

            return count + _nonAsciiChars.Count;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the characters in the set.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the characters in the set.</returns>
    /// <remarks>
    /// The enumeration returns ASCII characters first (in numerical order), followed by non-ASCII characters
    /// (in no guaranteed order).
    /// </remarks>
    public IEnumerable<char> GetCharacters()
    {
        for (var i = 0; i < ASCII_LIMIT; i++)
            if ((_asciiBitmap[i / BITS_PER_UINT] & 1u << i % BITS_PER_UINT) != 0)
                yield return (char) i;

        foreach (var c in _nonAsciiChars)
            yield return c;
    }

    /// <summary>
    /// Helper method to count set bits in an uint (Hamming weight)
    /// </summary>
    /// <param name="value">The unsigned integer value to count bits in.</param>
    /// <returns>The number of bits set to 1 in the specified value.</returns>
    private static int BitCount(uint value)
    {
        // SWAR (SIMD Within A Register) technique for counting the number
        // of set bits (1s) in a 32-bit unsigned integer.

        // Count bits in pairs.
        // Subtracts each pair of bits from itself shifted right by one, masked to isolate alternating bits.
        value -= value >> 1 & 0x55555555;
        // Count bits in 4-bit groups. Adds adjacent 2-bit counts to form 4-bit counts.
        value = (value & 0x33333333) + (value >> 2 & 0x33333333);
        // Aggregate all 4-bit counts into a single total.
        return (int) ((value + (value >> 4) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }

    /// <inheritdoc/>
    public IEnumerator<char> GetEnumerator()
    {
        foreach (var ch in GetCharacters()) yield return ch;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
