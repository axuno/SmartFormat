using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Parsing
{
    public class Format : FormatItem
    {

        #region: Constructors :

        public Format(string baseString) : base(baseString)
        {
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

        #region: Watched Characters :

        /// <summary>
        /// Allows our parsing engine to find special un-nested characters.
        /// </summary>
        private readonly Dictionary<char, List<int>> watchedChars = new Dictionary<char, List<int>>();
        /// <summary>
        /// Adds the watched character to the list.
        /// </summary>
        public void AddWatchedCharacter(char c, int index)
        {
            List<int> indexes;
            if (!watchedChars.TryGetValue(c, out indexes))
            {
                indexes = new List<int>();
                watchedChars.Add(c, indexes);
            }
            indexes.Add(index);
        }
        /// <summary>
        /// Retrieves a list of all occurrances of un-nested watched characters,
        /// as defined by our parsing engine.
        /// </summary>
        public List<int> GetWatchedCharacters(char c)
        {
            return watchedChars[c];
        }

        #endregion

        public string Text
        {
            get
            {
                return baseString.Substring(startIndex, endIndex - startIndex);
            }
        }

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            Reconstruct(this, sb);
            return sb.ToString();
        }
        private static void Reconstruct(Format f, StringBuilder result)
        {
            foreach (var item in f.Items)
            {
                if (item is LiteralText)
                {
                    var l = item as LiteralText;
                    result.Append(l.baseString, l.startIndex, l.endIndex - l.startIndex);
                }
                if (item is Placeholder)
                {
                    var p = item as Placeholder;
                    result.Append("{");
                    result.Append(string.Join(".", p.Selectors.ToArray()));
                    if (p.Format != null)
                    {
                        result.Append(":");
                        Reconstruct(p.Format, result);
                    }
                    result.Append("}");
                }
            }
        }

        #endregion

    }
}
