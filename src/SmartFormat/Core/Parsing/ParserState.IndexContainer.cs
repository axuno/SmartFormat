// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

namespace SmartFormat.Core.Parsing;

internal sealed partial class ParserState
{
    /// <summary>
    /// The Container for indexes pointing to positions within the input format.
    /// </summary>
    internal sealed record IndexContainer
    {
        internal const int PositionUndefined = -1;

        public IndexContainer()
        {
            Reset();
        }

        /// <summary>
        /// The length of the target object, where indexes will be used.
        /// E.g.: ReadOnlySpan&lt;char&gt;().Length or string.Length
        /// </summary>
        public int ObjectLength;

        /// <summary>
        /// The current index within the input format
        /// </summary>
        public int Current;

        /// <summary>
        /// The index within the input format after an item (like <see cref="Placeholder"/>, <see cref="Selector"/>, <see cref="LiteralText"/> etc.) was added.
        /// </summary>
        public int LastEnd;

        /// <summary>
        /// The start index of the formatter name within the input format.
        /// </summary>
        public int NamedFormatterStart;

        /// <summary>
        /// The start index of the formatter options within the input format.
        /// </summary>
        public int NamedFormatterOptionsStart;

        /// <summary>
        /// The end index of the formatter options within the input format.
        /// </summary>
        public int NamedFormatterOptionsEnd;

        /// <summary>
        /// The index of the operator within the input format.
        /// </summary>
        public int Operator;

        /// <summary>
        /// The current index of the selector <b>across all</b> <see cref="Placeholder"/>s.
        /// </summary>
        public int Selector;

        /// <summary>
        /// Adds a number to the index and returns the sum, but not more than <see cref="ObjectLength"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="add"></param>
        /// <returns>The sum, but not more than <see cref="ObjectLength"/></returns>
        public int SafeAdd(int index, int add)
        {
            // The design is the way, that an end index
            // is always 1 above the last position.
            // Meaning that the maximum of 'FormatItem.EndIndex' equals 'inputFormat.Length'
            index += add;
            System.Diagnostics.Debug.Assert(index >= 0);
            return index < ObjectLength ? index : ObjectLength;
        }

        public void Reset()
        {
            ObjectLength = 0;
            Current = PositionUndefined;
            LastEnd = 0;
            NamedFormatterStart = PositionUndefined;
            NamedFormatterOptionsStart = PositionUndefined;
            NamedFormatterOptionsEnd = PositionUndefined;
            Operator = PositionUndefined;
            Selector = PositionUndefined;
        }
    }
}
