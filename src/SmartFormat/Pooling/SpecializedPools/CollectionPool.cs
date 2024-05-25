// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SpecializedPools;

/// <summary>
/// Generic object pool implementation for <see cref="ICollection{T}"/>s.
/// </summary>
/// <typeparam name="TCollection"></typeparam> name=""/>
/// <typeparam name="TItem"></typeparam> name=""/>
internal class CollectionPool<TCollection, TItem> : SpecializedPoolAbstract<TCollection> where TCollection : class, ICollection<TItem>, new()
{
    private static readonly Lazy<CollectionPool<TCollection, TItem>> Lazy =
        new(() => new CollectionPool<TCollection, TItem>(),
            SmartSettings.IsThreadSafeMode
                ? LazyThreadSafetyMode.PublicationOnly
                : LazyThreadSafetyMode.None);

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    protected CollectionPool()
    {
        Policy.FunctionOnCreate = () => new TCollection();
        Policy.ActionOnReturn = coll => coll.Clear();
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static CollectionPool<TCollection, TItem> Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
