// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Threading;
using SmartFormat.Core.Output;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools;

/// <summary>
/// The object pool for <see cref="ZStringOutput"/>.
/// </summary>
internal sealed class ZStringOutputPool : SmartPoolAbstract<ZStringOutput>
{
    private static readonly Lazy<ZStringOutputPool> Lazy = new(() => new ZStringOutputPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private ZStringOutputPool()
    {
        Policy.FunctionOnCreate = () => new ZStringOutput();
        Policy.ActionOnReturn = zso => zso.Output.Clear();
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static ZStringOutputPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
