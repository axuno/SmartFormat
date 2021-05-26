//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a parsed format string.
    /// Contains a list of <see cref="FormatItem" />s,
    /// including <see cref="LiteralText" />s
    /// and <see cref="Placeholder" />s.
    /// </summary>
    public class Format : FormatItem
    {
        #region: Constructors :

        public Format(SmartSettings smartSettings, string baseString) : base(smartSettings, baseString, 0,
            baseString.Length)
        {
            Parent = null;
            Items = new List<FormatItem>();
        }

        public Format(SmartSettings smartSettings, Placeholder parent, int startIndex) : base(smartSettings, parent.BaseString, startIndex, parent.EndIndex)
        {
            Parent = parent;
            Items = new List<FormatItem>();
        }

        public Format(SmartSettings smartSettings, string baseString, int startIndex, int endIndex) : base(smartSettings, baseString, startIndex, endIndex)
        {
            Parent = null;
            Items = new List<FormatItem>();
        }

        #endregion

        #region: Fields and Properties :

        /// <summary>
        /// Gets the parent <see cref="Placeholder"/>.
        /// </summary>
        [Obsolete("Use property 'Parent' instead.")]
        public Placeholder? parent => Parent;

        /// <summary>
        /// Gets the parent <see cref="Placeholder"/>.
        /// </summary>
        public Placeholder? Parent { get; }

        public List<FormatItem> Items { get; }

        public bool HasNested { get; set; }

        #endregion

        #region: Special Optimized Functions :

        #region: Substring :

        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int start)
        {
            return Substring(start, Length - start);
        }

        /// <summary>Returns a substring of the current Format.</summary>
        public Format Substring(int start, int length)
        {
            start = StartIndex + start;
            var end = start + length;
            // Validate the arguments:
            if (start < StartIndex || start > EndIndex) // || endIndex > endIndex)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (end > EndIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            // If startIndex and endIndex already match this item, we're done:
            if (start == StartIndex && end == EndIndex) return this;

            var substring = new Format(SmartSettings, BaseString, start, end);
            foreach (var item in Items)
            {
                if (item.EndIndex <= start)
                    continue; // Skip first items
                if (end <= item.StartIndex)
                    break; // Done

                var newItem = item;
                if (item is LiteralText) // See if we need to slice the LiteralText:
                {
                    if (start > item.StartIndex || item.EndIndex > end)
                        newItem = new LiteralText(SmartSettings, substring, Math.Max(start, item.StartIndex),Math.Min(end, item.EndIndex));
                }
                else
                {
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
        /// Searches the literal text for the search char.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        public int IndexOf(char search)
        {
            return IndexOf(search, 0);
        }

        /// <summary>
        /// Searches the literal text for the search char.
        /// Does not search in nested placeholders.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="start"></param>
        public int IndexOf(char search, int start)
        {
            start = StartIndex + start;
            foreach (var item in Items)
            {
                if (item.EndIndex < start) continue;
                var literalItem = item as LiteralText;
                if (literalItem == null) continue;

                if (start < literalItem.StartIndex) start = literalItem.StartIndex;
                var literalIndex =
                    literalItem.BaseString.IndexOf(search, start, literalItem.EndIndex - start);
                if (literalIndex != -1) return literalIndex - StartIndex;
            }

            return -1;
        }

        #endregion

        #region: FindAll :

        private IList<int> FindAll(char search, int maxCount)
        {
            var results = new List<int>();
            var index = 0; // startIndex;
            while (maxCount != 0)
            {
                index = IndexOf(search, index);
                if (index == -1) break;
                results.Add(index);
                index++;
                maxCount--;
            }

            return results;
        }

        #endregion

        #region: Split :

        private char splitCacheChar;
        private IList<Format>? splitCache;

        public IList<Format> Split(char search)
        {
            if (splitCache == null || splitCacheChar != search)
            {
                splitCacheChar = search;
                splitCache = Split(search, -1);
            }

            return splitCache;
        }

        public IList<Format> Split(char search, int maxCount)
        {
            var splits = FindAll(search, maxCount);
            return new SplitList(this, splits);
        }

        /// <summary>
        /// Contains the results of a Split operation.
        /// This allows deferred splitting of items.
        /// </summary>
        private class SplitList : IList<Format>
        {
            #region Constructor

            private readonly Format _format;
            private readonly IList<int> _splits;

            public SplitList(Format format, IList<int> splits)
            {
                _format = format;
                _splits = splits;
            }

            #endregion

            #region IList

            public Format this[int index]
            {
                get
                {
                    if (index > _splits.Count) throw new ArgumentOutOfRangeException("index");

                    if (_splits.Count == 0) return _format;

                    if (index == 0) return _format.Substring(0, _splits[0]);

                    if (index == _splits.Count) return _format.Substring(_splits[index - 1] + 1);

                    // Return the format between the splits:
                    var startIndex = _splits[index - 1] + 1;
                    return _format.Substring(startIndex, _splits[index] - startIndex);
                }
                set => throw new NotSupportedException();
            }

            public void CopyTo(Format[] array, int arrayIndex)
            {
                var length = _splits.Count + 1;
                for (var i = 0; i < length; i++) array[arrayIndex + i] = this[i];
            }

            public int Count => _splits.Count + 1;

            public bool IsReadOnly => true;

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

            IEnumerator IEnumerable.GetEnumerator()
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
        public string GetLiteralText()
        {
            var sb = new StringBuilder();
            foreach (var item in Items)
            {
                if (item is LiteralText literalItem) sb.Append(literalItem);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reconstructs the format string, but doesn't include escaped chars
        /// and tries to reconstruct placeholders.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder(EndIndex - StartIndex);
            foreach (var item in Items) result.Append(item);
            return result.ToString();
        }

        #endregion
    }
}