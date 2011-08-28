using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Output
{
    public class StringOutput : IOutput
    {
        private readonly StringBuilder output;
        public StringOutput()
        {
            this.output = new StringBuilder();
        }
        public StringOutput(int capacity)
        {
            this.output = new StringBuilder(capacity);
        }
        public StringOutput(StringBuilder output)
        {
            this.output = output;
        }


        public void Write(string text)
        {
            output.Append(text);
        }
        public void Write(string text, FormatDetails formatDetails)
        {
            output.Append(text);
        }
        public void Write(string text, int startIndex, int length, FormatDetails formatDetails)
        {
            output.Append(text, startIndex, length);
        }
        public void Write(LiteralText item, FormatDetails formatDetails)
        {
            output.Append(item.baseString, item.startIndex, item.endIndex - item.startIndex);
        }



        /// <summary>
        /// Returns the results of the StringBuilder.
        /// </summary>
        public override string ToString()
        {
            return output.ToString();
        }
    }
}
