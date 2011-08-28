using System;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core
{
    public class FormatException : Exception
    {
        public FormatException(FormatItem errorItem, Exception formatException, int index)
        {
            this.Format = errorItem.baseString;
            this.FormatSoFar = null;
            this.ErrorItem = errorItem;
            this.Issue = formatException.Message;
            this.Index = index;
        }
        public FormatException(FormatItem errorItem, string issue, int index)
        {
            this.Format = errorItem.baseString;
            this.FormatSoFar = null;
            this.ErrorItem = errorItem;
            this.Issue = issue;
            this.Index = index;
        }

        public FormatException(string format, int index, string issue, Format formatSoFar)
        {
            this.Format = format;
            this.FormatSoFar = formatSoFar;
            this.ErrorItem = null;
            this.Index = index;
            this.Issue = issue;
        }

        public string Format { get; private set; }
        public Format FormatSoFar { get; private set; }
        public FormatItem ErrorItem { get; private set; }
        public string Issue { get; private set; }
        public int Index { get; private set; }

        public override string Message
        {
            get
            {
                return string.Format("Error parsing format string: {0} at {1}\n{2}\n{3}",
                                     Issue,
                                     Index,
                                     Format,
                                     new String('-', Index) + "^");
            }
        }
    }
}
