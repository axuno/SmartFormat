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
        /// <summary>
        /// Takes the current object and writes it to the output, using the specified format.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="args"></param>
        /// <param name="current"></param>
        /// <param name="format"></param>
        /// <param name="handled"></param>
        /// <param name="output"></param>
        void EvaluateFormat(SmartFormat formatter, object[] args, object current, Format format, ref bool handled, IOutput output);
    }
}
