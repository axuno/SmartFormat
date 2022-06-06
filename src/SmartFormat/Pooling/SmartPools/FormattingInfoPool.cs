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
/// The object pool for <see cref="FormattingInfo"/>.
/// </summary>
internal sealed class FormattingInfoPool : SmartPoolAbstract<FormattingInfo>
{
    private static readonly Lazy<FormattingInfoPool> Lazy = new(() => new FormattingInfoPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private FormattingInfoPool()
    {
        Policy.FunctionOnCreate = () => new FormattingInfo();
        Policy.ActionOnReturn = fi => fi.ReturnToPool();
    }

    /// <inheritdoc/>
    public override void Return(FormattingInfo toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.FormattingInfo))
            throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets a singleton instance of the pool.
    /// </summary>
    public static FormattingInfoPool Instance =>
        Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
}