using System;
using System.Collections.Generic;



namespace StringFormatEx
{
    internal class PlaceholderInfo 
    {
        public int placeholderStart;
        public int placeholderLength;
        public int selectorStart;
        public int selectorLength;
        public IEnumerable<string> selectors;
        public int formatStart;
        public int formatLength;
        public bool hasNested;
    }
}
