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
/// The object pool for <see cref="Selector"/>.
/// </summary>
internal sealed class SelectorPool : SmartPoolAbstract<Selector>
{
    private static readonly Lazy<SelectorPool> Lazy = new(() => new SelectorPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private SelectorPool()
    {
        Policy.FunctionOnCreate = () => new Selector();
        Policy.ActionOnReturn = selector => selector.Clear();
    }

    /// <inheritdoc/>
    public override void Return(Selector toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.Selector)) throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets a singleton instance of the pool.
    /// </summary>
    public static SelectorPool Instance =>
        Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
}