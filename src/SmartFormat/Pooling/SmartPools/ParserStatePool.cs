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
/// The object pool for <see cref="ParserState"/>.
/// </summary>
internal sealed class ParserStatePool : SmartPoolAbstract<ParserState>
{
    private static readonly Lazy<ParserStatePool> Lazy = new(() => new ParserStatePool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private ParserStatePool()
    {
        Policy.FunctionOnCreate = () => new ParserState();
        Policy.ActionOnReturn = state => state.Clear();
    }

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static ParserStatePool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
