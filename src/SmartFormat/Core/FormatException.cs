using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core
{
    public class FormatException : Exception
    {
        public FormatException(FormatItem errorItem, Exception formatException, int index)
            : base(string.Format("Error evaluating format string: {0} at {1}\n{2}\n{3}", formatException.Message, index, errorItem.baseString, new String('-', index) + "^"), formatException)
        {
            this.Format = errorItem.baseString;
            this.FormatSoFar = null;
            this.ErrorItem = errorItem;
            this.Issue = formatException.Message;
            this.Index = index;
        }
        public FormatException(FormatItem errorItem, string issue, int index)
            : base(string.Format("Error evaluating format string: {0} at {1}\n{2}\n{3}", issue, index, errorItem.baseString, new String('-', index) + "^"))
        {
            this.Format = errorItem.baseString;
            this.FormatSoFar = null;
            this.ErrorItem = errorItem;
            this.Issue = issue;
            this.Index = index;
        }

        public FormatException(string format, int index, string issue, Format formatSoFar)
            : base(string.Format("Error parsing format string: {0} at {1}\n{2}\n{3}", issue, index, format, new String('-', index) + "^"))
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
    }
}
