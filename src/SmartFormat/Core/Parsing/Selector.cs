//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a single selector in a <see cref="Placeholder" />.
    /// E.g.: {selector0.selector1?.selector2}, while "." and "?." and "?[]" are operators.
    /// </summary>
    public class Selector : FormatItem
    {
        /// <summary>
        /// The start index of the <see cref="Operator"/> inside of a <see cref="Selector"/>.
        /// </summary>
        internal readonly int OperatorStartIndex;

        /// <summary>
        /// Gets the length of the operator.
        /// </summary>
        internal int OperatorLength => StartIndex - OperatorStartIndex;

        /// <summary>
        /// Creates a new <see cref="Selector"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="baseString">The input format string</param>
        /// <param name="startIndex">The start index of the selector inside the <see cref="FormatItem.BaseString"/></param>
        /// <param name="endIndex">The end index of the selector inside the <see cref="FormatItem.BaseString"/></param>
        /// <param name="operatorStartIndex"></param>
        /// <param name="selectorIndex"></param>
        public Selector(SmartSettings settings, string baseString, int startIndex, int endIndex, int operatorStartIndex,
            int selectorIndex)
            : base(settings, baseString, startIndex, endIndex)
        {
            SelectorIndex = selectorIndex;
            OperatorStartIndex = operatorStartIndex;
        }

        /// <summary>
        /// The index of the selector in a multi-part selector.
        /// Example: {Person.Birthday.Year} has 3 selectors,
        /// and Year has a SelectorIndex of 2.
        /// </summary>
        public int SelectorIndex { get; }

        /// <summary>
        /// Gets the operator characters.
        /// </summary>
        /// <example>
        /// The operator that came between selectors is typically ("." or "?.")
        /// </example>
        public string Operator => BaseString.Substring(OperatorStartIndex, OperatorLength);
    }
}