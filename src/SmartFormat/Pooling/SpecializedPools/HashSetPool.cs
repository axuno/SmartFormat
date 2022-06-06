// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SpecializedPools;

/// <summary>
/// Generic object pool implementation for <see cref="HashSet{T}"/>s.
/// </summary>
internal sealed class HashSetPool<T> : CollectionPool<HashSet<T>, T>
{
    private static readonly Lazy<HashSetPool<T>> Lazy = new(() => new HashSetPool<T>(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private HashSetPool()
    {
        // Use initialization of base class
    }

    /// <summary>
    /// Gets a singleton instance of the pool.
    /// </summary>
    public static new HashSetPool<T> Instance =>
        Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
}