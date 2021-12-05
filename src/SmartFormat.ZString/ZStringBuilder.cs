using System;
using System.Collections.Generic;
using Cysharp.Text;

namespace SmartFormat.ZString
{
    /// <summary>
    /// A 1:1 wrapper around <see cref="Utf16ValueStringBuilder"/>,
    /// so that we don't have to reference Cysharp.Text classes directly.
    /// </summary>
    /// <remarks>
    /// We cannot add/get <see cref="ZStringBuilder"/> into/from a list or stack,
    /// because it contains a value type <see langword="struct"/> <see cref="Utf16ValueStringBuilder"/>.
    /// </remarks>
    public class ZStringBuilder : IDisposable
    {
        /**********************************
        The only required additional method is:
        public void Append(ZStringBuilder value) => _vsb.Append(value._vsb);
        **********************************/

        private Utf16ValueStringBuilder _vsb;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="disposeImmediately">
        /// If true uses thread-static buffer that is faster but must return immediately.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public ZStringBuilder(bool disposeImmediately)
        {
            _vsb =  new Utf16ValueStringBuilder(disposeImmediately);
        }

        /// <summary>Length of written buffer.</summary>
        public int Length => _vsb.Length;

        /// <summary>Get the written buffer data.</summary>
        public ReadOnlySpan<char> AsSpan() => _vsb.AsSpan();

        /// <summary>Get the written buffer data.</summary>
        public ReadOnlyMemory<char> AsMemory() => _vsb.AsMemory();

        /// <summary>Get the written buffer data.</summary>
        public ArraySegment<char> AsArraySegment() => _vsb.AsArraySegment();

        /// <summary>
        /// Return the inner buffer to pool.
        /// </summary>
        public void Dispose() => _vsb.Dispose();

        public void Clear() => _vsb.Clear();
        public void TryGrow(int sizeHint) => _vsb.TryGrow(sizeHint);
        public void Grow(int sizeHint) => _vsb.Grow(sizeHint);

        /// <summary>Appends the default line terminator to the end of this instance.</summary>
        public void AppendLine() => _vsb.AppendLine();

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(char value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(string value) => _vsb.AppendLine(value);

        public void AppendLine(ReadOnlySpan<char> value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine<T>(T value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Byte value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Byte value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.DateTime value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.DateTime value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.DateTimeOffset value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.DateTimeOffset value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Decimal value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Decimal value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Double value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Double value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int16 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int16 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int32 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int32 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int64 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Int64 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.SByte value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.SByte value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Single value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Single value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.TimeSpan value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.TimeSpan value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt16 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt16 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt32 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt32 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt64 value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.UInt64 value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Guid value) => _vsb.AppendLine(value);

        /// <summary>Appends the string representation of a specified value with numeric format strings followed by the default line terminator to the end of this instance.</summary>
        public void AppendLine(System.Guid value, string format) => _vsb.AppendLine(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(char value) => _vsb.Append(value);

        public void Append(char value, int repeatCount) => _vsb.Append(value, repeatCount);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(string value) => _vsb.Append(value);

        /// <summary>Appends a contiguous region of arbitrary memory to this instance.</summary>
        public void Append(ReadOnlySpan<char> value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append<T>(T value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(ZStringBuilder value) => _vsb.Append(value._vsb);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Byte value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Byte value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.DateTime value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.DateTime value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.DateTimeOffset value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.DateTimeOffset value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Decimal value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Decimal value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Double value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Double value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Int16 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Int16 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Int32 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Int32 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Int64 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Int64 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.SByte value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.SByte value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Single value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Single value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.TimeSpan value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.TimeSpan value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.UInt16 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.UInt16 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.UInt32 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.UInt32 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.UInt64 value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.UInt64 value, string format) => _vsb.Append(value, format);

        /// <summary>Appends the string representation of a specified value to this instance.</summary>
        public void Append(System.Guid value) => _vsb.Append(value);

        /// <summary>Appends the string representation of a specified value to this instance with numeric format strings.</summary>
        public void Append(System.Guid value, string format) => _vsb.Append(value, format);

        /// <summary>
        /// Inserts a string 0 or more times into this builder at the specified position.
        /// </summary>
        /// <param name="index">The index to insert in this builder.</param>
        /// <param name="value">The string to insert.</param>
        /// <param name="count">The number of times to insert the string.</param>
        public void Insert(int index, string value, int count) => _vsb.Insert(index, value, count);

        public void Insert(int index, string value) => _vsb.Insert(index, value);
        public void Insert(int index, ReadOnlySpan<char> value, int count) => _vsb.Insert(index, value, count);

        /// <summary>
        /// Replaces all instances of one character with another in this builder.
        /// </summary>
        /// <param name="oldChar">The character to replace.</param>
        /// <param name="newChar">The character to replace <paramref name="oldChar"/> with.</param>
        public void Replace(char oldChar, char newChar) => _vsb.Replace(oldChar, newChar);

        /// <summary>
        /// Replaces all instances of one character with another in this builder.
        /// </summary>
        /// <param name="oldChar">The character to replace.</param>
        /// <param name="newChar">The character to replace <paramref name="oldChar"/> with.</param>
        /// <param name="startIndex">The index to start in this builder.</param>
        /// <param name="count">The number of characters to read in this builder.</param>
        public void Replace(char oldChar, char newChar, int startIndex, int count) =>
            _vsb.Replace(oldChar, newChar, startIndex, count);

        /// <summary>
        /// Replaces all instances of one string with another in this builder.
        /// </summary>
        /// <param name="oldValue">The string to replace.</param>
        /// <param name="newValue">The string to replace <paramref name="oldValue"/> with.</param>
        /// <remarks>
        /// If <paramref name="newValue"/> is <c>null</c>, instances of <paramref name="oldValue"/>
        /// are removed from this builder.
        /// </remarks>
        public void Replace(string oldValue, string newValue) => _vsb.Replace(oldValue, newValue);

        public void Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue) => _vsb.Replace(oldValue, newValue);

        /// <summary>
        /// Replaces all instances of one string with another in part of this builder.
        /// </summary>
        /// <param name="oldValue">The string to replace.</param>
        /// <param name="newValue">The string to replace <paramref name="oldValue"/> with.</param>
        /// <param name="startIndex">The index to start in this builder.</param>
        /// <param name="count">The number of characters to read in this builder.</param>
        /// <remarks>
        /// If <paramref name="newValue"/> is <c>null</c>, instances of <paramref name="oldValue"/>
        /// are removed from this builder.
        /// </remarks>
        public void Replace(string oldValue, string newValue, int startIndex, int count) =>
            _vsb.Replace(oldValue, newValue, startIndex, count);

        public void Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, int startIndex, int count) =>
            _vsb.Replace(oldValue, newValue, startIndex, count);

        /// <summary>
        /// Replaces the contents of a single position within the builder.
        /// </summary>
        /// <param name="newChar">The character to use at the position.</param>
        /// <param name="replaceIndex">The index to replace.</param>
        public void ReplaceAt(char newChar, int replaceIndex) => _vsb.ReplaceAt(newChar, replaceIndex);

        /// <summary>
        /// Removes a range of characters from this builder.
        /// </summary>
        /// <remarks>
        /// This method does not reduce the capacity of this builder.
        /// </remarks>
        public void Remove(int startIndex, int length) => _vsb.Remove(startIndex, length);

        /// <summary>Copy inner buffer to the destination span.</summary>
        public bool TryCopyTo(Span<char> destination, out int charsWritten) => _vsb.TryCopyTo(destination, out charsWritten);

        /// <summary>
        /// Converts the value of this instance to a system.String.
        /// </summary>
        /// <remarks>
        /// <see cref="Utf16ValueStringBuilder"/> creates the string from the buffer.
        /// Using <i>string.Create</i> here does not bring better results.
        /// </remarks>
        public override string ToString() => _vsb.ToString();

        /// <summary>IBufferWriter.GetMemory.</summary>
        public Memory<char> GetMemory(int sizeHint) => _vsb.GetMemory(sizeHint);

        /// <summary>IBufferWriter.GetSpan.</summary>
        public Span<char> GetSpan(int sizeHint) => _vsb.GetSpan(sizeHint);

        /// <summary>IBufferWriter.Advance.</summary>
        public void Advance(int count) => _vsb.Advance(count);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1>(string format, T1 arg1) => _vsb.AppendFormat(format, arg1);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2>(string format, T1 arg1, T2 arg2) => _vsb.AppendFormat(format, arg1, arg2);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3) =>
            _vsb.AppendFormat(format, arg1, arg2, arg3);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
            _vsb.AppendFormat(format, arg1, arg2, arg3, arg4);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
            _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
            T6 arg6) => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
            T6 arg6, T7 arg7) => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
            T6 arg6, T7 arg7, T8 arg8) => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11, arg12);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11, arg12, arg13);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)  => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);

        /// <summary>Appends the string returned by processing a composite format string, each format item is replaced by the string representation of arguments.</summary>
        public void AppendFormat<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12,
            T13 arg13, T14 arg14, T15 arg15, T16 arg16) => _vsb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6,
            arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);

        /// <summary>
        /// Concatenates the string representations of the elements in the provided array of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public void AppendJoin<T>(char separator, params T[] values) => _vsb.AppendJoin(separator, values);

        public void AppendJoin<T>(char separator, List<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(char separator, ReadOnlySpan<T> values) => _vsb.AppendJoin(separator, values);

        /// <summary>
        /// Concatenates and appends the members of a collection, using the specified char separator between each member.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public void AppendJoin<T>(char separator, IEnumerable<T> values) => _vsb.AppendJoin(separator, values);

        public void AppendJoin<T>(char separator, ICollection<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(char separator, IList<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(char separator, IReadOnlyList<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(char separator, IReadOnlyCollection<T> values) => _vsb.AppendJoin(separator, values);

        /// <summary>
        /// Concatenates the string representations of the elements in the provided array of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public void AppendJoin<T>(string separator, params T[] values) => _vsb.AppendJoin(separator, values);

        public void AppendJoin<T>(string separator, List<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(string separator, ReadOnlySpan<T> values) => _vsb.AppendJoin(separator, values);

        /// <summary>
        /// Concatenates and appends the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public void AppendJoin<T>(string separator, IEnumerable<T> values) => _vsb.AppendJoin(separator, values);

        public void AppendJoin<T>(string separator, ICollection<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(string separator, IList<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(string separator, IReadOnlyList<T> values) => _vsb.AppendJoin(separator, values);
        public void AppendJoin<T>(string separator, IReadOnlyCollection<T> values) => _vsb.AppendJoin(separator, values);

        void IDisposable.Dispose()
        {
            _vsb.Dispose();
        }
    }
}