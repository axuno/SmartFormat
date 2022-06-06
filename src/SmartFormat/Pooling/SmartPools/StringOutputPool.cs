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
/// The object pool for <see cref="StringOutput"/>.
/// </summary>
internal sealed class StringOutputPool : SmartPoolAbstract<StringOutput>
{
    private static readonly Lazy<StringOutputPool> Lazy = new(() => new StringOutputPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private StringOutputPool()
    {
        Policy.FunctionOnCreate = () => new StringOutput();
        Policy.ActionOnReturn = so => so.Clear();
    }

    /// <summary>
    /// Gets a singleton instance of the pool.
    /// </summary>
    public static StringOutputPool Instance =>
        Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
}