//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a single selector in a <see cref="Placeholder" />
    /// that comes before the colon.
    /// </summary>
    public class Selector : FormatItem
    {
        /// <summary>
        /// The start index of the <see cref="Operator"/> inside of a <see cref="Selector"/>.
        /// </summary>
        internal readonly int OperatorStartIndex;

        public Selector(SmartSettings smartSettings, string baseString, int startIndex, int endIndex, int operatorStartIndex,
            int selectorIndex)
            : base(smartSettings, baseString, startIndex, endIndex)
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
        /// Gets the one of the operator characters as defined in <see cref="SmartSettings.Parser.OperatorChars"/>.
        /// </summary>
        /// <example>
        /// The operator that came between selectors is typically a colon (".")
        /// </example>
        public string Operator => BaseString.Substring(OperatorStartIndex, StartIndex - OperatorStartIndex);
    }
}