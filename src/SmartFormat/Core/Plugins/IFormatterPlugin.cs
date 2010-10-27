using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core.Plugins
{
    public interface IFormatter
    {
        /// <summary>
        /// Takes the current object and writes it to the output, using the specified format.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="format"></param>
        /// <param name="handled"></param>
        /// <param name="output"></param>
        /// <param name="formatDetails"></param>
        void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails);
    }
}
