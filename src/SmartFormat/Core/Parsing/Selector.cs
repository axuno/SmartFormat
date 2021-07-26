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
        /// <param name="placeholder">The <see cref="Placeholder"/> the <see cref="Selector"/> belongs to.</param>
        /// <param name="operatorStartIndex"></param>
        /// <param name="selectorIndex"></param>
        public Selector(Placeholder placeholder, int startIndex, int endIndex, int operatorStartIndex,
            int selectorIndex)
            : base(placeholder.SmartSettings, placeholder.BaseString, startIndex, endIndex)
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