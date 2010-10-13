using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Parsing
{
    public sealed class LiteralText : FormatItem
    {
        public LiteralText(Format parent, int startIndex) : base(parent, startIndex)
        { }
        public LiteralText(Format parent) : base(parent, parent.startIndex)
        { }

        public string Text
        {
            get
            {
                return this.baseString.Substring(startIndex, endIndex - startIndex);
            }
        }
    }
}
