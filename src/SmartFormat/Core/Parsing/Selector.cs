//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a single selector
    /// in the text in a <see cref="Placeholder" />
    /// that comes before the colon.
    /// </summary>
    public class Selector : FormatItem
    {
        /// <summary>
        /// Keeps track of where the "operators" started for this item.
        /// </summary>
        internal readonly int operatorStart;

        public Selector(SmartSettings smartSettings, string baseString, int startIndex, int endIndex, int operatorStart,
            int selectorIndex)
            : base(smartSettings, baseString, startIndex, endIndex)
        {
            SelectorIndex = selectorIndex;
            this.operatorStart = operatorStart;
        }

        /// <summary>
        /// The index of the selector in a multi-part selector.
        /// Example: {Person.Birthday.Year} has 3 seletors,
        /// and Year has a SelectorIndex of 2.
        /// </summary>
        public int SelectorIndex { get; }

        /// <summary>
        /// The operator that came before the selector; typically "."
        /// </summary>
        public string Operator => baseString.Substring(operatorStart, startIndex - operatorStart);
    }
}