using System.IO;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

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
