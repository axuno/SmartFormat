using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx.Plugins
{
    public class ArrayPlugin : IStringFormatterPlugin
    {

        public IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions()
        {
            return new EventHandler<ExtendSourceEventArgs>[] 
                { this.GetArraySource, this.GetArrayIndex };
        }

        public IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions()
        {
            return new EventHandler<ExtendFormatEventArgs>[] 
                { this.DoArrayFormatting };
        }


        /// <summary>
        /// This allows an integer to be used as a selector to index an array (or list).
        /// Doesn't support ICollection because ICollection doesn't implement indexed items.
        /// 
        /// This is better described using an example:
        /// CustomFormat("{Dates.2.Year}", {#1/1/2000#, #12/31/2999#, #9/9/9999#}) = "9999"
        /// The ".2" selector is used to reference Dates(2).
        /// </summary>
        private void GetArraySource(object sender, ExtendSourceEventArgs e)
        {
            ICustomSourceInfo info = e.SourceInfo;
            int itemIndex;
            if (info.Current is IList && int.TryParse(info.Selector, out itemIndex) && itemIndex < ((IList)info.Current).Count) {
                info.Current = ((IList)info.Current)[itemIndex];
            }
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private int CollectionIndex = -1;



        /// <summary>
        /// Allows you to use "{Index}" as a valid selector.
        /// </summary>
        /// <param name="info"></param>
        [CustomFormatPriority(CustomFormatPriorities.Lowest)]
        private void GetArrayIndex(object sender, ExtendSourceEventArgs e) // Handles ExtendCustomSource
        {
            ICustomSourceInfo info = e.SourceInfo;
            // This event has the Lowest priority, which means that it only fires if reflection fails.
            // We want to see if there is an "Index" property that was supplied.
            if (info.Selector.Equals("index", StringComparison.OrdinalIgnoreCase)) {
                if (info.Current is IList) {
                    // This might occur if we have 2 lists that we are trying to sync.
                    IList list = info.Current as IList;
                    if (0 <= CollectionIndex & CollectionIndex < list.Count) {
                        info.Current = list[CollectionIndex];
                    }
                    else {
                        // Not a valid index!!!  Cannot synchronize these lists.
                    }
                } 
                else {
                    // We want the Index to be inserted:
                    info.Current = CollectionIndex;
                }
            }
        }


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
        [CustomFormatPriority(CustomFormatPriorities.Highest)]
        private void DoArrayFormatting(object sender, ExtendFormatEventArgs e)
        {
            CustomFormatInfo info = e.FormatInfo;
            // This method needs the Highest priority so that it comes before Strings.Format.Conditional.vb
            ICollection items = info.Current as ICollection;

            if (items == null) {
                return;
            }


            // The SplitString function is in the file Strings.Format.Conditional.vb:
            string[] split = Core.ParsingServices.SplitNested(info.Format, '|', 3);
            string format = split[0];
            string spacer = null, lastSpacer = null;

            if (split.Length >= 2) {
                spacer = split[1];
            } 
            else {
                spacer = " "; // At least put a space between items by default
            }


            if (split.Length >= 3) {
                lastSpacer = split[2];
            }

            if (!info.HasNested) {
                format = "{:" + format + "}";
            }



            int itemCount = -1;
            if (lastSpacer != null) {
                itemCount = items.Count;
            }

            int oldCollectionIndex = CollectionIndex;
            // In case we have nested arrays, we might need to restore the CollectionIndex
            CollectionIndex = -1;
            foreach (object item in items) {
                CollectionIndex += 1;
                // Keep track of the index

                // If it isn't the first item, then write the spacer:
                if (CollectionIndex > 0) {
                    // Write either the spacer or lastSpacer:
                    if (itemCount == -1 || CollectionIndex < itemCount - 1) {
                        info.Write(spacer);
                    } 
                    else {
                        info.Write(lastSpacer);
                    }
                }

                // Write the format for this item:
                info.Current = item;
                info.SetFormat(format, info.HasNested);
                info.CustomFormatNested();
            }
            CollectionIndex = oldCollectionIndex;
            // Restore the CollectionIndex

        }

    }
}