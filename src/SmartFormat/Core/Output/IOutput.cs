using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;

namespace SmartFormat.Core.Output
{
    public interface IOutput
    {
        void Write(string text, FormatDetails formatDetails);
        void Write(string text, int startIndex, int length, FormatDetails formatDetails);
        void Write(LiteralText item, FormatDetails formatDetails);
    }
}
