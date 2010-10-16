using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Output
{
    public interface IOutput
    {
        void Write(string text);
        void Write(string text, int startIndex, int length);
        void Write(LiteralText item);
    }
}
