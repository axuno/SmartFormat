// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SpecializedPools
{
    /// <summary>
    /// Generic object pool implementation for <see cref="IList{T}"/>s.
    /// </summary>
    internal sealed class ListPool<T> : CollectionPool<List<T>, T>
    {
        private static readonly Lazy<ListPool<T>> Lazy = new(() => new ListPool<T>(),
            SmartSettings.IsThreadSafeMode
                ? LazyThreadSafetyMode.PublicationOnly
                : LazyThreadSafetyMode.None);
        
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <remarks>
        /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
        /// </remarks>
        private ListPool()
        {
            // Use initialization of base class
        }

        /// <summary>
        /// Gets a singleton instance of the pool.
        /// </summary>
        public static new ListPool<T> Instance =>
            Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
    }
}