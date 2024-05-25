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
/// The object pool for <see cref="Format"/>.
/// </summary>
internal sealed class FormatPool : SmartPoolAbstract<Format>
{
    private static readonly Lazy<FormatPool> Lazy = new(() => new FormatPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private FormatPool()
    {
        Policy.FunctionOnCreate = () => new Format();
        Policy.ActionOnReturn = fmt => fmt.ReturnToPool();
    }

    /// <inheritdoc/>
    public override void Return(Format toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.Format))
            throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static FormatPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
