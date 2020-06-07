using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

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
    public class ListFormatter : IFormatter, ISource
    {
        private readonly SmartSettings _smartSettings;

        public string[] Names { get; set; } = {"list", "l", ""};

        public ListFormatter(SmartFormatter formatter)
        {
            formatter.Parser.AddOperators("[]()");
            _smartSettings = formatter.Settings;
        }

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

            // See if we're trying to access a specific index:
            int itemIndex;
            var currentList = current as IList;
            var isAbsolute = selectorInfo.SelectorIndex == 0 && selectorInfo.SelectorOperator.Length == 0;
            if (!isAbsolute && currentList != null && int.TryParse(selector, out itemIndex) &&
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
                if (currentList != null && 0 <= CollectionIndex && CollectionIndex < currentList.Count)
                {
                    selectorInfo.Result = currentList[CollectionIndex];
                    return true;
                }
            }

            return false;
        }

        // This does not work, because CollectionIndex will be initialized only once 
        // NOT once per thread.
        // [ThreadStatic] private static int CollectionIndex = -1;
        // same with: private static ThreadLocal<int> CollectionIndex2 = new ThreadLocal<int>(() => -1);
        // Good example: https://msdn.microsoft.com/en-us/library/dn906268(v=vs.110).aspx
        /// <remarks>
        /// Wrap, so that CollectionIndex can be used without code changes.
        /// </remarks>
        private static readonly AsyncLocal<int?> _collectionIndex = new AsyncLocal<int?>();

        /// <remarks>
        /// System.Runtime.Remoting.Messaging and CallContext.Logical[Get|Set]Data 
        /// not supported by .Net Core. Instead .Net Core provides AsyncLocal&lt;T&gt;
        /// Good examples are: https://msdn.microsoft.com/en-us/library/dn906268(v=vs.110).aspx
        /// and https://github.com/StephenCleary/AsyncLocal/blob/master/src/UnitTests/UnitTests.cs
        /// </remarks>
        private static int CollectionIndex
        {
            get { return _collectionIndex.Value ?? -1; }
            set { _collectionIndex.Value = value; }
        }    

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // This method needs the Highest priority so that it comes before the PluralLocalizationExtension and ConditionalExtension

            // This extension requires at least IEnumerable
            var enumerable = current as IEnumerable;
            if (enumerable == null) return false;
            // Ignore Strings, because they're IEnumerable.
            // This issue might actually need a solution
            // for other objects that are IEnumerable.
            if (current is string) return false;
            // If the object is IFormattable, ignore it
            if (current is IFormattable) return false;

            // This extension requires a | to specify the spacer:
            if (format == null) return false;
            var parameters = format.Split('|', 4);
            if (parameters.Count < 2) return false;

            // Grab all formatting options:
            // They must be in one of these formats:
            // itemFormat|spacer
            // itemFormat|spacer|lastSpacer
            // itemFormat|spacer|lastSpacer|twoSpacer
            var itemFormat = parameters[0];
            var spacer = parameters.Count >= 2 ? parameters[1].GetLiteralText() : "";
            var lastSpacer = parameters.Count >= 3 ? parameters[2].GetLiteralText() : spacer;
            var twoSpacer = parameters.Count >= 4 ? parameters[3].GetLiteralText() : lastSpacer;

            if (!itemFormat.HasNested)
            {
                // The format is not nested,
                // so we will treat it as an itemFormat:
                var newItemFormat = new Format(_smartSettings, itemFormat.baseString)
                {
                    startIndex = itemFormat.startIndex,
                    endIndex = itemFormat.endIndex,
                    HasNested = true
                };
                var newPlaceholder = new Placeholder(_smartSettings, newItemFormat, itemFormat.startIndex, 0)
                {
                    Format = itemFormat,
                    endIndex = itemFormat.endIndex
                };
                newItemFormat.Items.Add(newPlaceholder);
                itemFormat = newItemFormat;
            }

            // Let's buffer all items from the enumerable (to ensure the Count without double-enumeration):
            var items = current as ICollection;
            if (items == null)
            {
                var allItems = new List<object>();
                foreach (var item in enumerable) allItems.Add(item);
                items = allItems;
            }

            var oldCollectionIndex =
                CollectionIndex; // In case we have nested arrays, we might need to restore the CollectionIndex
            CollectionIndex = -1;
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
                    formattingInfo.Write(spacer);
                }
                else if (CollectionIndex == 1)
                {
                    formattingInfo.Write(twoSpacer);
                }
                else
                {
                    formattingInfo.Write(lastSpacer);
                }

                // Output the nested format for this item:
                formattingInfo.Write(itemFormat, item);
            }

            CollectionIndex = oldCollectionIndex; // Restore the CollectionIndex

            return true;
        }
    }
}