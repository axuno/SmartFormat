using System.IO;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a TextWriter so that it can be used for output.
    /// </summary>
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
    }
}
