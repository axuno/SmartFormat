// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Threading;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools;

/// <summary>
/// The object pool for <see cref="Placeholder"/>.
/// </summary>
internal sealed class PlaceholderPool : SmartPoolAbstract<Placeholder>
{
    private static readonly Lazy<PlaceholderPool> Lazy = new(() => new PlaceholderPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private PlaceholderPool()
    {
        Policy.FunctionOnCreate = () => new Placeholder();
        Policy.ActionOnReturn = ph => ph.ReturnToPool();
    }

    /// <inheritdoc/>
    public override void Return(Placeholder toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.Placeholder)) throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static PlaceholderPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
