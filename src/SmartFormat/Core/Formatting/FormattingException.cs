using System;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Formatting
{
    /// <summary>
    /// An exception caused while attempting to output the format.
    /// </summary>
    public class FormattingException : Exception
    {
        public FormattingException(FormatItem errorItem, Exception formatException, int index)
        {
            this.Format = errorItem.baseString;
            this.ErrorItem = errorItem;
            this.Issue = formatException.Message;
            this.Index = index;
        }
        public FormattingException(FormatItem errorItem, string issue, int index)
        {
            this.Format = errorItem.baseString;
            this.ErrorItem = errorItem;
            this.Issue = issue;
            this.Index = index;
        }

        public string Format { get; private set; }
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
