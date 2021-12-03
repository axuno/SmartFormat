//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a single selector in a <see cref="Placeholder" />.
    /// E.g.: {selector0.selector1?.selector2}, while "." and "?." and "?[]" are operators.
    /// </summary>
    public class Selector : FormatItem
    {
        private string? _operatorCache;

        /// <summary>
        /// The start index of the <see cref="Operator"/> inside of a <see cref="Selector"/>.
        /// </summary>
        internal int OperatorStartIndex { get; private set; }

        /// <summary>
        /// Gets the length of the operator.
        /// </summary>
        internal int OperatorLength => StartIndex - OperatorStartIndex;

        #region: Create, initialize, return to pool :

        /// <summary>
        /// CTOR for object pooling.
        /// Immediately after creating the instance, an overload of 'Initialize' must be called.
        /// </summary>
        public Selector()
        {
            // Inserted for clarity and documentation
        }

        /// <summary>
        /// Initializes this <see cref="Selector"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="parent">The parent <see cref="FormatItem"/>.</param>
        /// <param name="baseString">The input format string</param>
        /// <param name="startIndex">The start index of the selector inside the <see cref="FormatItem.BaseString"/></param>
        /// <param name="endIndex">The end index of the selector inside the <see cref="FormatItem.BaseString"/></param>
        /// <param name="operatorStartIndex"></param>
        /// <param name="selectorIndex"></param>
        public Selector Initialize(SmartSettings settings, FormatItem parent, string baseString, int startIndex, int endIndex, int operatorStartIndex,
            int selectorIndex)
        {
            base.Initialize(settings, parent, baseString, startIndex, endIndex);
            SelectorIndex = selectorIndex;
            OperatorStartIndex = operatorStartIndex;
            return this;
        }

        /// <summary>
        /// Clears the <see cref="Selector"/>.
        /// <para>This method gets called by <see cref="SelectorPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.</para>
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            SelectorIndex = 0;
            OperatorStartIndex = 0;
            _operatorCache = null;
        }

        #endregion

        /// <summary>
        /// The index of the selector in a multi-part selector.
        /// Example: {Person.Birthday.Year} has 3 selectors,
        /// and Year has a SelectorIndex of 2.
        /// </summary>
        public int SelectorIndex { get; private set; }

        /// <summary>
        /// Gets the operator characters.
        /// </summary>
        /// <example>
        /// The operator that came between selectors is typically ("." or "?.")
        /// </example>
        public string Operator => _operatorCache ??= BaseString.Substring(OperatorStartIndex, OperatorLength);
    }
}