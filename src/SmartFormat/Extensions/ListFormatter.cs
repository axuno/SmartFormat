//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// If the source value is an array (or supports ICollection),
    /// then each item will be custom formatted.
    /// Syntax:
    /// #1: "format|spacer"
    /// #2: "format|spacer|last spacer"
    /// #3: "format|spacer|last spacer|two spacer"
    /// The format will be used for each item in the collection, the spacer will be between all items, and the last spacer
    /// will replace the spacer for the last item only.
    /// Example:
    /// CustomFormat("{Dates:D|; |; and }", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "January 1, 2000; December 31, 2999;
    /// and September 9, 9999"
    /// In this example, format = "D", spacer = "; ", and last spacer = "; and "
    /// Advanced:
    /// Composite Formatting is allowed in the format by using nested braces.
    /// If a nested item is detected, Composite formatting will be used.
    /// Example:
    /// CustomFormat("{Sizes:{Width}x{Height}|, }", {new Size(4,3), new Size(16,9)}) = "4x3, 16x9"
    /// In this example, format = "{Width}x{Height}".  Notice the nested braces.
    /// </summary>
    /// <remarks>
    /// The <see cref="ListFormatter"/> PluralLocalizationExtension and ConditionalExtension
    /// </remarks>
    public class ListFormatter : IFormatter, ISource, IInitializer
    {
        // Will be overridden during Initialize()
        private SmartSettings _smartSettings = new();
        private bool _isInitialized = false;

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"list", "l", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "list";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used to split the option text literals.
        /// </summary>
        public char SplitChar { get; set; } = '|';

        /// <summary>
        /// This allows an integer to be used as a selector to index an array (or list).
        /// This is better described using an example:
        /// CustomFormat("{Dates.2.Year}", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "9999"
        /// The ".2" selector is used to reference Dates[2].
        /// </summary>
        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var current = selectorInfo.CurrentValue;
            var selector = selectorInfo.SelectorText;

            if (current is not IList currentList) return false;

            // See if we're trying to access a specific index:
            var isAbsolute = selectorInfo.SelectorIndex == 0 && selectorInfo.SelectorOperator.Length == 0;
            if (!isAbsolute && int.TryParse(selector, out var itemIndex) &&
                itemIndex < currentList.Count)
            {
                // The current is a List, and the selector is a number;
                // let's return the List item:
                // Example: {People[2].Name}
                //           ^List  ^itemIndex
                selectorInfo.Result = currentList[itemIndex];
                return true;
            }

            // We want to see if there is an "Index" property that was supplied.
            if (selector.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                // Looking for "{Index}"
                if (selectorInfo.SelectorIndex == 0)
                {
                    selectorInfo.Result = CollectionIndex;
                    return true;
                }

                // Looking for 2 lists to sync: "{List1: {List2[Index]} }"
                if (0 <= CollectionIndex && CollectionIndex < currentList.Count)
                {
                    selectorInfo.Result = currentList[CollectionIndex];
                    return true;
                }
            }

            return false;
        }

        // Note: CollectionIndex must be initialized ONLY ONCE, NOT once per thread (aka ThreadStatic).
        // Note: Multi threading support generates some garbage
        private static readonly AsyncLocal<int?> _collectionIndexThreadSafe = new();
        private static int _collectionIndexSingleThread = -1;

        /// <summary>
        /// Gets, whether the <see cref="ListFormatter"/> is in thread safe mode.
        /// </summary>
        internal static bool IsThreadSafeMode { get; set; } = SmartSettings.IsThreadSafeMode;

        /// <summary>
        /// Gets or sets the collection index.
        /// </summary>
        /// <remarks>
        /// In thread safe mode, <see cref="CollectionIndex"/> wraps an <see cref="AsyncLocal{T}"/>,
        /// while in single thread mode, a static <see langword="int"/> ist wrapped.
        /// </remarks>
        private static int CollectionIndex
        {
            get => IsThreadSafeMode
                ? _collectionIndexThreadSafe.Value ?? -1
                : _collectionIndexSingleThread;

            set
            {
                if (IsThreadSafeMode)
                    _collectionIndexThreadSafe.Value = value;
                else _collectionIndexSingleThread = value;
            }
        }

        /// <summary>
        /// Writes the given <see cref="IFormattingInfo"/> to the <see cref="Core.Output.IOutput"/>
        /// if it can be processed by the formatter.
        /// </summary>
        /// <param name="formattingInfo">The <see cref="IFormattingInfo"/> to process.</param>
        /// <returns>Returns <see langword="true"/> if processing was possible, else <see langword="false"/>.</returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // If the ListFormatter is called explicitly, null with nullable must be handled here
            if (current is null && HasNullableOperator(formattingInfo))
            {
                formattingInfo.Write(string.Empty);
                return true;
            }

            // See if the format string contains un-nested "|":
            using var splitListPooledObject = SplitListPool.Instance.Get(out var splitList);
            var parameters = (SplitList) (format is not null ? format.Split(SplitChar, 4) : splitList);
            
            // Check whether arguments can be handled by this formatter
            if (format is null || parameters.Count < 2 || current is not IEnumerable currentAsEnumerable 
                || currentAsEnumerable is string or IFormattable)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                {
                    parameters.Clear();
                    return false;
                }

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires an IEnumerable argument and at least 2 format parameters.");
            }

            // Grab all formatting options:
            // They must be in one of these formats:
            // itemFormat|spacer
            // itemFormat|spacer|lastSpacer
            // itemFormat|spacer|lastSpacer|twoSpacer
            var itemFormat = parameters[0];
            var spacer = parameters.Count >= 2 ? parameters[1].GetLiteralText() : string.Empty;
            var lastSpacer = parameters.Count >= 3 ? parameters[2].GetLiteralText() : spacer;
            var twoSpacer = parameters.Count >= 4 ? parameters[3].GetLiteralText() : lastSpacer;
            
            if (!itemFormat.HasNested)
            {
                // The format is not nested,
                // so we will treat it as an ItemFormat:
                var newItemFormat = FormatPool.Instance.Get().Initialize(_smartSettings, itemFormat.BaseString,
                    itemFormat.StartIndex, itemFormat.EndIndex, true);

                var newPlaceholder = PlaceholderPool.Instance.Get().Initialize(newItemFormat, itemFormat.StartIndex, 0);
                newPlaceholder.Format = itemFormat;
                newPlaceholder.EndIndex = itemFormat.EndIndex;
                // inherit alignment
                newPlaceholder.Alignment = formattingInfo.Alignment;
                
                newItemFormat.Items.Add(newPlaceholder);
                itemFormat = newItemFormat;
            }

            // Let's buffer all items from the enumerable (to ensure the Count without double-enumeration):
            List<object>? itemsAsList = null;
            if (currentAsEnumerable is not ICollection items)
            {
                itemsAsList = ListPool<object>.Instance.Get();
                foreach (var item in currentAsEnumerable)
                    itemsAsList.Add(item);

                items = itemsAsList;
            }

            // In case we have nested arrays, we might need to restore the CollectionIndex
            var savedCollectionIndex = CollectionIndex; 
            CollectionIndex = -1;

            // Do not inherit alignment for the spacers - use new FormattingInfo
            var spacerFormattingInfo = FormattingInfoPool.Instance.Get()
                .Initialize(null, formattingInfo.FormatDetails, format, null);
            spacerFormattingInfo.Alignment = 0;

            foreach (var item in items)
            {
                CollectionIndex += 1; // Keep track of the index
                
                // Determine which spacer to write:
                if (spacer == null || CollectionIndex == 0)
                {
                    // Don't write the spacer.
                }
                else if (CollectionIndex < items.Count - 1)
                {
                    spacerFormattingInfo.Write(spacer);
                }
                else if (CollectionIndex == 1)
                {
                    spacerFormattingInfo.Write(twoSpacer);
                }
                else
                {
                    spacerFormattingInfo.Write(lastSpacer);
                }

                // Output the nested format for this item:
                formattingInfo.FormatAsChild(itemFormat, item);
            }

            CollectionIndex = savedCollectionIndex; // Restore the CollectionIndex

            FormattingInfoPool.Instance.Return(spacerFormattingInfo);

            if (itemsAsList != null)
                ListPool<object>.Instance.Return(itemsAsList);

            parameters.Clear();

            return true;
        }

        /// <summary>
        /// Checks if any of the <see cref="Placeholder"/>'s <see cref="Placeholder.Selectors"/> has nullable <c>?</c> as their first operator.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>
        /// <see langword="true"/>, any of the <see cref="Placeholder"/>'s <see cref="Placeholder.Selectors"/> has nullable <c>?</c> as their first operator.
        /// </returns>
        /// <remarks>
        /// The nullable operator '?' can be followed by a dot (like '?.') or a square brace (like '?[')
        /// </remarks>
        private bool HasNullableOperator(IFormattingInfo formattingInfo)
        {
            return formattingInfo.Placeholder != null &&
                   formattingInfo.Placeholder.Selectors.Any(s =>
                       s.OperatorLength > 0 && s.BaseString[s.OperatorStartIndex] == _smartSettings.Parser.NullableOperator);
        }

        ///<inheritdoc />
        public void Initialize(SmartFormatter smartFormatter)
        {
            if (_isInitialized) return;
            _smartSettings = smartFormatter.Settings;
            _isInitialized = true;
        }
    }
}
