// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Threading;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools;

/// <summary>
/// The object pool for <see cref="FormatDetails"/>.
/// </summary>
internal sealed class FormatDetailsPool : SmartPoolAbstract<FormatDetails>
{
    private static readonly Lazy<FormatDetailsPool> Lazy = new(() => new FormatDetailsPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private FormatDetailsPool()
    {
        Policy.FunctionOnCreate = () => new FormatDetails();
        Policy.ActionOnReturn = fd => fd.Clear();
    }

    /// <inheritdoc/>
    public override void Return(FormatDetails toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.FormatDetails))
            throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static FormatDetailsPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
