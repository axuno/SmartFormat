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
/// The object pool for <see cref="ParsingErrors"/>.
/// </summary>
internal sealed class ParsingErrorsPool : SmartPoolAbstract<ParsingErrors>
{
    private static readonly Lazy<ParsingErrorsPool> Lazy = new(() => new ParsingErrorsPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private ParsingErrorsPool()
    {
        Policy.FunctionOnCreate = () => new ParsingErrors();
        Policy.ActionOnReturn = pe => pe.Clear();
    }

    /// <inheritdoc/>
    public override void Return(ParsingErrors toReturn)
    {
        if (ReferenceEquals(toReturn, InitializationObject.ParsingErrors)) throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
        base.Return(toReturn);
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static ParsingErrorsPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
