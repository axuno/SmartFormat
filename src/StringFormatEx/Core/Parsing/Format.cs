using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    public sealed class Format : FormatItem
    {

        #region: Constructors :

        public Format(string baseString) : base(baseString, 0, baseString.Length)
        {
            this.parent = null;
            Items = new List<FormatItem>();
        }
        public Format(Placeholder parent, int startIndex) : base(parent, startIndex)
        {
            this.parent = parent;
            Items = new List<FormatItem>();
        }

        #endregion

        public readonly Placeholder parent;
        public List<FormatItem> Items { get; private set; }
        public bool HasNested { get; set; }

        #region: IndexOf :
        /// <summary>
        /// Searches the literal text for the search string.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        public int IndexOf(string search)
        {
            return IndexOf(search, this.startIndex);
        }
        /// <summary>
        /// Searches the literal text for the search string.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="startIndex"></param>
        public int IndexOf(string search, int startIndex)
        {
            foreach (var item in this.Items)
            {
                if (item.endIndex < startIndex) continue;
                var literalItem = item as LiteralText;
                if (literalItem == null) continue;

                if (startIndex < literalItem.startIndex) startIndex = literalItem.startIndex;
                var literalIndex = literalItem.baseString.IndexOf(search, startIndex, literalItem.endIndex - startIndex);
                if (literalIndex != -1) return literalIndex;
            }
            return -1;
        }

        #endregion

        #region: Substring :

        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int startIndex)
        {
            return Substring(startIndex, this.endIndex);
        }
        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int startIndex, int endIndex)
        {
            // Validate the arguments:
            if (startIndex < this.startIndex || startIndex > this.endIndex) // || endIndex > this.endIndex)
                throw new ArgumentOutOfRangeException("startIndex");
            if (endIndex > this.endIndex)
                throw new ArgumentOutOfRangeException("endIndex");

            var substring = new Format(this.baseString) { startIndex = startIndex, endIndex = endIndex };
            foreach (var item in this.Items)
            {
                if (item.endIndex <= startIndex)
                    continue; // Skip first items
                if (endIndex <= item.startIndex)
                    break; // Done

                var newItem = item;
                if (item is LiteralText) // See if we need to slice the LiteralText:
                {
                    if (startIndex > item.startIndex || item.endIndex > endIndex)
                    {
                        newItem = new LiteralText(substring) {
                            startIndex = Math.Max(startIndex, item.startIndex),
                            endIndex = Math.Min(endIndex, item.endIndex)
                        };
                    }
                } else {
                    // item is a placeholder -- we can't split a placeholder though.
                    substring.HasNested = true;
                }
                substring.Items.Add(newItem);
            }

            return substring;
        }

        #endregion

        #region: ToString :

        /// <summary>
        /// Retrieves the literal text contained in this format.
        /// Excludes escaped chars, and does not include the text
        /// of placeholders
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            var sb = new StringBuilder();
            foreach (var item in this.Items)
            {
                var literalItem = item as LiteralText;
                if (literalItem != null)
                {
                    sb.Append(literalItem.baseString, literalItem.startIndex, literalItem.endIndex - literalItem.startIndex);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reconstructs the format string, but doesn't include escaped chars
        /// and tries to reconstruct placeholders.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder(endIndex - startIndex);
            foreach (var item in Items)
            {
                result.Append(item.ToString());
            }
            return result.ToString();
        }

        #endregion

        public Format[] Split(string search)
        {
            return Split(search, -1);
        }

        public Format[] Split(string search, int maxCount)
        {
            var results = new List<Format>();
            var startIndex = this.startIndex;
            while (startIndex != -1)
            {
                var nextIndex = this.IndexOf(search, startIndex);
                if (nextIndex == -1 || maxCount == 0)
                {
                    results.Add(this.Substring(startIndex));
                    break;
                }
                else
                {
                    results.Add(this.Substring(startIndex, nextIndex));
                }
                startIndex = nextIndex + search.Length;
                maxCount--;
            }
            return results.ToArray();
        }
    }
}
