using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core
{
    /// <summary>
    /// Caches information about a format operation 
    /// so that repeat calls can run faster.
    /// </summary>
    public class FormatCache
    {
        public FormatCache(Format format)
        {
            this.Format = format;
        }
        public Format Format { get; private set; }
    }
}
