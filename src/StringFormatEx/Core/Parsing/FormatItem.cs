using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Parsing
{
    public abstract class FormatItem
    {
        public FormatItem(FormatItem parent, int startIndex) : this(parent.baseString, startIndex)
        { }

        public FormatItem(string baseString) : this(baseString, 0, baseString.Length)
        { }
        public FormatItem(string baseString, int startIndex) : this(baseString, startIndex, baseString.Length)
        { }
        public FormatItem(string baseString, int startIndex, int endIndex)
        {
            this.baseString = baseString;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
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
