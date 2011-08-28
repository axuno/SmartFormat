using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    [ExtensionPriority(ExtensionPriority.Highest)]
    public class ListExtension : IFormatter, ISource 
    {

        public ListExtension(SmartFormatter formatter)
        {
            formatter.Parser.AddOperators("[]()");
        }

        /// <summary>
        /// This allows an integer to be used as a selector to index an array (or list).
        /// 
        /// This is better described using an example:
        /// CustomFormat("{Dates.2.Year}", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "9999"
        /// The ".2" selector is used to reference Dates(2).
        /// </summary>
        public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            // See if we're trying to access a specific index:
            int itemIndex;
            var currentList = current as IList;
            if (selector.SelectorIndex > 0 && currentList != null && int.TryParse(selector.Text, out itemIndex) && itemIndex < currentList.Count)
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
        /// format [| spacer [| last spacer ]]
        /// 
        /// The format will be used for each item in the collection, the spacer will be between all items, and the last spacer will replace the spacer for the last item only.
        /// 
        /// Example:
        /// CustomFormat("{Dates:D|; |; and }", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "January 1, 2000; December 31, 2999; and September 9, 9999"
        /// In this example, format = "D", spacer = "; ", and last spacer = "; and "
        /// 
        /// 
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
            // This method needs the Highest priority so that it comes before the ConditionalExtension

            // Check if this Format has the correct syntax:
            // (It must have a nested placeholder, and must have >= 2 parameters
            //if (format == null || !format.HasNested) return;
            // Split our parameters:
            //var parameters = format.Split("|", 3);
            //if (parameters.Count == 1) return;


            // This extension requires at least IEnumerable
            var enumerable = current as IEnumerable;
            if (enumerable == null) return;
            // Ignore Strings, because they're IEnumerable.
            // This issue might actually need a solution 
            // for other objects that are IEnumerable.
            if (current is string) return;






            // Let's retrieve all items from the enumerable:
            IList items = current as IList;
            if (items == null)
            {
                var allItems = new List<object>();
                foreach (var item in enumerable)
                {
                    allItems.Add(item);
                }
                items = allItems.ToArray();
            }

            // Create an item formatter:
            Format itemFormat = null;
            string spacer = null;
            string lastSpacer = null;
            if (format != null)
            {
                var parameters = format.Split("|", 3);
                itemFormat = parameters[0];
                spacer = (parameters.Count >= 2) ? parameters[1].Text : "";
                lastSpacer = (parameters.Count >= 3) ? parameters[2].Text : null;
            }
            if (itemFormat == null)
            {
                // The format is not nested,
                // so we will treat it as an itemFormat:
                // Create an empty placeholder:
                var newItemFormat = new Format("");
                newItemFormat.HasNested = true;
                var placeholder = new Placeholder(newItemFormat, 0, formatDetails.Placeholder.NestedDepth);
                placeholder.Format = null;
                newItemFormat.Items.Add(placeholder);
                itemFormat = newItemFormat;
            }
            else if (!itemFormat.HasNested)
            {
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

            int oldCollectionIndex = CollectionIndex; // In case we have nested arrays, we might need to restore the CollectionIndex

            CollectionIndex = -1;
            foreach (object item in items) {
                CollectionIndex += 1; // Keep track of the index

                // If it isn't the first item, then write the spacer:
                if (spacer != null && CollectionIndex != 0) {
                    // Write either the spacer or lastSpacer:
                    if (lastSpacer == null || CollectionIndex < items.Count - 1) {
                        output.Write(spacer, formatDetails);
                    }
                    else 
                    {
                        output.Write(lastSpacer, formatDetails);
                    }
                }

                // Output the nested format for this item:
                formatDetails.Formatter.Format(output, itemFormat, item, formatDetails);
            }

            CollectionIndex = oldCollectionIndex; // Restore the CollectionIndex

            handled = true;
        }

    }
}