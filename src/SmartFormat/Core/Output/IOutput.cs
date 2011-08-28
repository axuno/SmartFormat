using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Output
{
    public interface IOutput
    {
        void Write(string text, FormatDetails formatDetails);
        void Write(string text, int startIndex, int length, FormatDetails formatDetails);
        void Write(LiteralText item, FormatDetails formatDetails);
    }
}
