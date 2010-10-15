using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SmartFormat.Core.Output
{
    public class TextWriterOutput : IOutput
    {
        public TextWriterOutput(TextWriter output)
        {
            Output = output;
        }
        public TextWriter Output { get; private set; }

        public void Write(string text)
        {
            Output.Write(text);
        }

        public void Write(string text, int startIndex, int length)
        {
            Output.Write(text.Substring(startIndex, length));
        }

        public void Write(Parsing.LiteralText item)
        {
            Output.Write(item.baseString.Substring(item.startIndex, item.endIndex - item.startIndex));
        }
    }
}
