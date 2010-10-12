using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Parsing;

namespace StringFormatEx.Core
{
    public class FormatException : Exception
    {
        public FormatException(string format, int index, string issue, Format formatSoFar) 
            : base(string.Format("Error parsing format string: {0} at {1}\n{2}", issue, index, format))
        {
            this.Format = format;
            this.Index = index;
            this.Issue = issue;
            this.FormatSoFar = formatSoFar;
        }
        public FormatException(string format, int index, string issue)
            : base(string.Format("Error evaluating format string: {0} at {1}\n{2}", issue, index, format))
        {
            this.Format = format;
            this.Index = index;
            this.Issue = issue;
        }

        public string Format { get; private set; }
        public int Index { get; private set; }
        public string Issue { get; private set; }
        public Format FormatSoFar { get; private set; }
    }
}
