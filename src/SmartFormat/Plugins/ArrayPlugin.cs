using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SmartFormat.Core;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;

namespace SmartFormat.Plugins
{
    [PluginPriority(PluginPriority.Highest)]
    public class ArrayPlugin : IFormatter, ISource 
    {

        public ArrayPlugin(SmartFormatter formatter)
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
        public void EvaluateSelector(SmartFormatter formatter, object[] args, object current, Selector selector, ref bool handled, ref object result)
        {
            int itemIndex;
            var currentList = current as IList;
            if (selector.SelectorIndex > 0 && currentList != null && int.TryParse(selector.Text, out itemIndex) && itemIndex < currentList.Count)
            {
                // The current is a List, and the selector is a number;
                // let's return the List item:
                // Example: {People.2.Name}
                //           ^List  ^itemIndex
                result = currentList[itemIndex];
                handled = true;
            }
        //}

        ///// <summary>
        ///// Allows you to use "{Index}" as a valid selector.
        ///// </summary>
        //public void EvaluateSelector2()
        //{
            // This event has the Lowest priority, which means that it only fires if reflection fails.

            // We want to see if there is an "Index" property that was supplied.
            if (selector.Text.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                if (selector.SelectorIndex == 0)
                {
                    result = CollectionIndex;
                    handled = true;
                    return;
                }

                //var currentList = current as IList;
                if (currentList != null)
                {
                    // This might occur if we have 2 lists that we are trying to sync.
                    if (0 <= CollectionIndex & CollectionIndex < currentList.Count)
                    {
                        result = currentList[CollectionIndex];
                        handled = true;
                    }
                    else
                    {
                        // Not a valid index!!!  Cannot synchronize these lists.
                    }
                }
                else
                {
                    // We want the Index to be inserted:
                    result = CollectionIndex;
                    handled = true;
                }
            }
        }



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
        /// Composite Formatting is allowed in the format by using nested brackets.
        /// If a nested item is detected, Composite formatting will be used.
        ///
        /// Example:
        /// CustomFormat("{Sizes:{Width}x{Height}|, }", {new Size(4,3), new Size(16,9)}) = "4x3, 16x9"
        /// In this example, format = "{Width}x{Height}".  Notice the nested brackets.
        /// 
        /// </summary>
        public void EvaluateFormat(SmartFormatter formatter, object[] args, object current, Format format, ref bool handled, IOutput output)
        {
            // This method needs the Highest priority so that it comes before the ConditionalPlugin

            // Check if this Format has the correct syntax:
            // (It must have a nested placeholder, and must have >= 2 parameters
            if (format == null || !format.HasNested) return;
            // Split our parameters:
            var parameters = format.Split("|", 3);
            //if (parameters.Count == 1) return;


            // This plugin requires at least IEnumerable
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

            Format itemFormat = null;
            string spacer = null;
            string lastSpacer = null;
            if (format != null)
            {
                itemFormat = parameters[0];
                spacer = (parameters.Count >= 2) ? parameters[1].Text : "";
                lastSpacer = (parameters.Count >= 3) ? parameters[2].Text : null;
            }

            int oldCollectionIndex = CollectionIndex; // In case we have nested arrays, we might need to restore the CollectionIndex

            CollectionIndex = -1;
            foreach (object item in items) {
                CollectionIndex += 1; // Keep track of the index

                // If it isn't the first item, then write the spacer:
                if (CollectionIndex != 0) {
                    // Write either the spacer or lastSpacer:
                    if (lastSpacer == null || CollectionIndex < items.Count - 1) {
                        output.Write(spacer);
                    }
                    else 
                    {
                        output.Write(lastSpacer);
                    }
                }

                // Output the nested format for this item:
                formatter.Format(output, itemFormat, args, item);

            }

            CollectionIndex = oldCollectionIndex; // Restore the CollectionIndex

            handled = true;
        }

    }
}