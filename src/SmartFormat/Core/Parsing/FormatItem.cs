using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a substring of text.
    /// </summary>
    public abstract class FormatItem
    {
        public FormatItem(FormatItem parent, int startIndex) : this(parent.baseString, startIndex, parent.baseString.Length)
        { }
        public FormatItem(string baseString, int startIndex, int endIndex)
        {
            this.baseString = baseString;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        /// <summary>
        /// Retrieves the substring that this item represents.
        /// </summary>
        public string Text
        {
            get
            {
                return this.baseString.Substring(startIndex, endIndex - startIndex);
            }
        }


        public readonly string baseString;
        public int startIndex;
        public int endIndex;

        public override string ToString()
        {
            if (endIndex <= startIndex)
                return string.Format("Empty ({0})", baseString.Substring(startIndex));
            return string.Format("{0}", baseString.Substring(startIndex, endIndex - startIndex));
        }
    }
}
