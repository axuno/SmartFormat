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
            this.CachedObjects = new Dictionary<string, object>();
        }
        /// <summary>
        /// Caches the parsed format.
        /// </summary>
        public Format Format { get; private set; }
        /// <summary>
        /// Storage for any misc objects.
        /// This can be used by extensions that want to cache data.
        /// </summary>
        public Dictionary<string, object> CachedObjects { get; private set; }
    }
}
