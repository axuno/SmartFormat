using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    public class LiteralText : FormatItem
    {
        public LiteralText(Format parent, int startIndex) : base(parent, startIndex)
        { }
        public LiteralText(Format parent) : base(parent, parent.startIndex)
        { }

        public override string ToString()
        {
            return this.baseString.Substring(startIndex, endIndex - startIndex);
        }
    }
}
