using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;

namespace SmartFormat.Core.Output
{
    public class TextWriterOutput : IOutput
    {
        public TextWriterOutput(TextWriter output)
        {
            Output = output;
        }
        public TextWriter Output { get; private set; }

        public void Write(string text, FormatDetails formatDetails)
        {
            Output.Write(text);
        }

        public void Write(string text, int startIndex, int length, FormatDetails formatDetails)
        {
            Output.Write(text.Substring(startIndex, length));
        }

        public void Write(LiteralText item, FormatDetails formatDetails)
        {
            Output.Write(item.baseString.Substring(item.startIndex, item.endIndex - item.startIndex));
        }
    }
}
