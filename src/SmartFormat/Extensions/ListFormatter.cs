using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    public class ListFormatter : IFormatter, ISource 
    {

        public ListFormatter(SmartFormatter formatter)
        {
            formatter.Parser.AddOperators("[]()");
        }

        /// <summary>
        /// This allows an integer to be used as a selector to index an array (or list).
        /// 
        /// This is better described using an example:
        /// CustomFormat("{Dates.2.Year}", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "9999"
        /// The ".2" selector is used to reference Dates[2].
        /// </summary>
        public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            // See if we're trying to access a specific index:
            int itemIndex;
            var currentList = current as IList;
    		var isAbsolute = (selector.SelectorIndex == 0 && selector.Operator.Length == 0);
			if (!isAbsolute && currentList != null && int.TryParse(selector.Text, out itemIndex) && itemIndex < currentList.Count)
            {
                // The current is a List, and the selector is a number;
                // let's return the List item:
                // Example: {People[2].Name}
                //           ^List  ^itemIndex
                result = currentList[itemIndex];
                handled = true;
            }


            // We want to see if there is an "Index" property that was supplied.
            if (selector.Text.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                // Looking for "{Index}"
                if (selector.SelectorIndex == 0)
                {
                    result = CollectionIndex;
                    handled = true;
                    return;
                }

                // Looking for 2 lists to sync: "{List1: {List2[Index]} }"
                if (currentList != null && 0 <= CollectionIndex && CollectionIndex < currentList.Count)
                {
                    result = currentList[CollectionIndex];
                    handled = true;
                }
            }
        }


        [ThreadStatic]
        private int CollectionIndex = -1;
        /// <summary>
        /// If the source value is an array (or supports ICollection), 
        /// then each item will be custom formatted.
        /// 
        /// 
        /// Syntax: 
        /// #1: "format|spacer"
        /// #2: "format|spacer|last spacer"
        /// #3: "format|spacer|last spacer|two spacer"
        /// 
        /// The format will be used for each item in the collection, the spacer will be between all items, and the last spacer will replace the spacer for the last item only.
        /// 
        /// Example:
        /// CustomFormat("{Dates:D|; |; and }", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "January 1, 2000; December 31, 2999; and September 9, 9999"
        /// In this example, format = "D", spacer = "; ", and last spacer = "; and "
        /// 
        /// 
        /// 
        /// Advanced:
        /// Composite Formatting is allowed in the format by using nested braces.
        /// If a nested item is detected, Composite formatting will be used.
        ///
        /// Example:
        /// CustomFormat("{Sizes:{Width}x{Height}|, }", {new Size(4,3), new Size(16,9)}) = "4x3, 16x9"
        /// In this example, format = "{Width}x{Height}".  Notice the nested braces.
        /// 
        /// </summary>
        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            // This method needs the Highest priority so that it comes before the PluralLocalizationExtension and ConditionalExtension

            // This extension requires at least IEnumerable
            var enumerable = current as IEnumerable;
            if (enumerable == null) return;
            // Ignore Strings, because they're IEnumerable.
            // This issue might actually need a solution 
            // for other objects that are IEnumerable.
            if (current is string) return;
            // If the object is IFormattable, ignore it
            if (current is IFormattable) return;

            // This extension requires a | to specify the spacer:
            if (format == null) return;
            var parameters = format.Split("|", 4);
            if (parameters.Count < 2) return;
            
            // Grab all formatting options:
            // They must be in one of these formats:
            // itemFormat|spacer
            // itemFormat|spacer|lastSpacer
            // itemFormat|spacer|lastSpacer|twoSpacer
            var itemFormat = parameters[0];
            var spacer = (parameters.Count >= 2) ? parameters[1].Text : "";
            var lastSpacer = (parameters.Count >= 3) ? parameters[2].Text : spacer;
            var twoSpacer = (parameters.Count >= 4) ? parameters[3].Text : lastSpacer;

            if (!itemFormat.HasNested)
            {
                // The format is not nested,
                // so we will treat it as an itemFormat:
                var newItemFormat = new Format(itemFormat.baseString);
                newItemFormat.startIndex = itemFormat.startIndex;
                newItemFormat.endIndex = itemFormat.endIndex;
                newItemFormat.HasNested = true;
                var newPlaceholder = new Placeholder(newItemFormat, itemFormat.startIndex, formatDetails.Placeholder.NestedDepth);
                newPlaceholder.Format = itemFormat;
                newPlaceholder.endIndex = itemFormat.endIndex;
                newItemFormat.Items.Add(newPlaceholder);
                itemFormat = newItemFormat;
            }

            // Let's buffer all items from the enumerable (to ensure the Count without double-enumeration):
            ICollection items = current as ICollection;
            if (items == null)
            {
                var allItems = new List<object>();
                foreach (var item in enumerable)
                {
                    allItems.Add(item);
                }
                items = allItems;
            }

            int oldCollectionIndex = CollectionIndex; // In case we have nested arrays, we might need to restore the CollectionIndex
            CollectionIndex = -1;
            foreach (object item in items) {
                CollectionIndex += 1; // Keep track of the index

                // Determine which spacer to write:
                if (spacer == null || CollectionIndex == 0)
                {
                    // Don't write the spacer.
                }
                else if (CollectionIndex < items.Count - 1) {
                    output.Write(spacer, formatDetails);
                }
                else if (CollectionIndex == 1)
                {
                    output.Write(twoSpacer, formatDetails);
                }
                else
                {
                    output.Write(lastSpacer, formatDetails);
                }

                // Output the nested format for this item:
                formatDetails.Formatter.Format(output, itemFormat, item, formatDetails);
            }

            CollectionIndex = oldCollectionIndex; // Restore the CollectionIndex

            handled = true;
        }

    }
}