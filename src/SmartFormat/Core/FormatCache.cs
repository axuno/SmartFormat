using System.Collections.Generic;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core
{
    /// <summary>
    /// Caches information about a format operation 
    /// so that repeat calls can be optimized to run faster.
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
        /// This can be used by extensions that want to cache data,
        /// such as reflection information.
        /// </summary>
        public Dictionary<string, object> CachedObjects { get; private set; }
    }
}
