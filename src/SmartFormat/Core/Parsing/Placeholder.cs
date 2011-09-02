using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents the placeholder in a parsed format string,
    /// including the <see cref="Selector"/>s 
    /// and the item <see cref="Format"/>.
    /// </summary>
    public class Placeholder : FormatItem
    {
        public Placeholder(Format parent, int startIndex, int nestedDepth) : base(parent, startIndex)
        {
            this.parent = parent;
            this.Selectors = new List<Selector>();
            this.NestedDepth = nestedDepth;
        }

        public readonly Format parent;
        public List<Selector> Selectors { get; private set; }
        public Format Format { get; set; }
        public int Alignment { get; set; }
        public int NestedDepth { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder(endIndex - startIndex);
            result.Append('{');
            foreach (var s in Selectors)
            {
                result.Append(s.baseString, s.operatorStart, s.endIndex - s.operatorStart);
            }
            if (Alignment != 0)
            {
                result.Append(',');
                result.Append(Alignment);
            }
            if (Format != null)
            {
                result.Append(':');
                result.Append(Format.ToString());
            }
            result.Append('}');
            return result.ToString();
        }
    }
}
