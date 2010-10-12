using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Parsing;
using StringFormatEx.Core.Output;

namespace StringFormatEx.Core.Plugins
{
    public interface IFormatterPlugin
    {
        void Format(object arg, Format format, ref bool handled, IOutput output);
    }
    public delegate void FormatDelegate(object arg, Format format, ref bool handled, IOutput output);
}
