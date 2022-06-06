// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SpecializedPools;

/// <summary>
/// Generic object pool implementation for <see cref="IDictionary{TKey,TValue}"/>s.
/// </summary>
internal sealed class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> where TKey: notnull
{
    private static readonly Lazy<DictionaryPool<TKey, TValue>> Lazy = new(() => new DictionaryPool<TKey, TValue>(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private DictionaryPool()
    {
        // Use initialization of base class
    }

    /// <summary>
    /// Gets a singleton instance of the pool.
    /// </summary>
    public static new DictionaryPool<TKey, TValue> Instance =>
        Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
}