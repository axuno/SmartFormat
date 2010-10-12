using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Parsing;

namespace StringFormatEx.Core.Output
{
    public interface IOutput
    {
        void Write(string text);
        void Write(string text, int startIndex, int length);
        //void Write(Format format);
        //void Write(Format format, int startIndex, int length);
        void Write(FormatItem item);
    }
}
