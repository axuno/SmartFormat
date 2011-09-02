using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a parsed format string.
    /// Contains a list of <see cref="FormatItem"/>s, 
    /// including <see cref="LiteralText"/>s 
    /// and <see cref="Placeholder"/>s.
    /// </summary>
    public class Format : FormatItem
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

        #region: Fields and Properties :

        public readonly Placeholder parent;
        public List<FormatItem> Items { get; private set; }
        public bool HasNested { get; set; }

        #endregion

        #region: Special Optimized Functions :

        #region: Substring :

        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int startIndex)
        {
            return Substring(startIndex, this.endIndex - this.startIndex - startIndex);
        }
        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int startIndex, int length)
        {
            startIndex = this.startIndex + startIndex;
            var endIndex = startIndex + length;
            // Validate the arguments:
            if (startIndex < this.startIndex || startIndex > this.endIndex) // || endIndex > this.endIndex)
                throw new ArgumentOutOfRangeException("startIndex");
            if (endIndex > this.endIndex)
                throw new ArgumentOutOfRangeException("length");

            // If startIndex and endIndex already match this item, we're done:
            if (startIndex == this.startIndex && endIndex == this.endIndex)
            {
                return this;
            }

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

        #region: IndexOf :
        /// <summary>
        /// Searches the literal text for the search string.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        public int IndexOf(string search)
        {
            return IndexOf(search, 0);
        }
        /// <summary>
        /// Searches the literal text for the search string.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="index"></param>
        public int IndexOf(string search, int startIndex)
        {
            startIndex = this.startIndex + startIndex;
            foreach (var item in this.Items)
            {
                if (item.endIndex < startIndex) continue;
                var literalItem = item as LiteralText;
                if (literalItem == null) continue;

                if (startIndex < literalItem.startIndex) startIndex = literalItem.startIndex;
                var literalIndex = literalItem.baseString.IndexOf(search, startIndex, literalItem.endIndex - startIndex);
                if (literalIndex != -1) return literalIndex - this.startIndex;
            }
            return -1;
        }

        #endregion

        #region: FindAll :

        public IList<int> FindAll(string search)
        {
            return FindAll(search, -1);
        }

        public IList<int> FindAll(string search, int maxCount)
        {
            var results = new List<int>();
            var index = 0; // this.startIndex;
            while (maxCount != 0)
            {
                index = this.IndexOf(search, index);
                if (index == -1) break;
                results.Add(index);
                index += search.Length;
                maxCount--;
            }
            return results;
        }
        
        #endregion

        #region: Split :

        public IList<Format> Split(string search)
        {
            return Split(search, -1);
        }

        public IList<Format> Split(string search, int maxCount)
        {
            var splits = this.FindAll(search, maxCount);
            return new SplitList(this, splits, search.Length);
        }

        /// <summary>
        /// Contains the results of a Split operation.
        /// This allows deferred splitting of items.
        /// </summary>
        private class SplitList : IList<Format>
        {
            #region Constructor

            private readonly Format format;
            private readonly IList<int> splits;
            private readonly int searchLength;
            public SplitList(Format format, IList<int> splits, int searchLength)
            {
                this.format = format;
                this.splits = splits;
                this.searchLength = searchLength;
            }

            #endregion

            #region IList

            public Format this[int index]
            {
                get
                {
                    if (index > splits.Count) throw new ArgumentOutOfRangeException("index");

                    if (splits.Count == 0)
                    {
                        // No splits, so return the whole format
                        return format;
                    }
                    else if (index == 0)
                    {
                        // Return the format before the first split:
                        return format.Substring(0, splits[0]);
                    }
                    else if (index == splits.Count)
                    {
                        // Return the format after the last split:
                        return format.Substring(splits[index - 1] + searchLength);
                    }
                    else
                    {
                        // Return the format between the splits:
                        var startIndex = splits[index - 1] + searchLength;
                        return format.Substring(startIndex, splits[index] - startIndex);
                    }
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            public void CopyTo(Format[] array, int arrayIndex)
            {
                var length = splits.Count + 1;
                for (int i = 0; i < length; i++)
                {
                    array[arrayIndex + i] = this[i];
                }
            }

            public int Count
            {
                get { return splits.Count + 1; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            #endregion

            #region NotSupported IList Interface

            public int IndexOf(Format item)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, Format item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public void Add(Format item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(Format item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(Format item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<Format> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion

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
    
    }
}
