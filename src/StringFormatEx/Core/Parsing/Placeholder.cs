using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Parsing
{
    public sealed class Placeholder : FormatItem
    {
        public Placeholder(Format parent, int startIndex) : base(parent, startIndex)
        {
            this.parent = parent;
            this.Selectors = new List<string>();
        }

        public readonly Format parent;
        public List<string> Selectors {get; private set;}
        public Format Format { get; set; }
    }
}
