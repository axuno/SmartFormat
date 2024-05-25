// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Text;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SpecializedPools;

/// <summary>
/// The object pool for <see cref="StringBuilder"/>.
/// </summary>
internal sealed class StringBuilderPool : SpecializedPoolAbstract<StringBuilder>
{
    private static readonly Lazy<StringBuilderPool> Lazy = new(() => new StringBuilderPool(),
        SmartSettings.IsThreadSafeMode
            ? LazyThreadSafetyMode.PublicationOnly
            : LazyThreadSafetyMode.None);
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <remarks>
    /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
    /// </remarks>
    private StringBuilderPool()
    {
        Policy.FunctionOnCreate = () => new StringBuilder(DefaultStringBuilderCapacity);
        Policy.ActionOnReturn = sb =>
        {
            sb.Clear(); // Clear the StringBuilder before setting the new capacity
            sb.Capacity = DefaultStringBuilderCapacity;
        };
    }

    /// <summary>
    /// Gets or sets the <see cref="StringBuilder.Capacity"/>, that is used
    /// when creating new instances, or when returning an instance to the pool.
    /// <para>The default capacity is 1024.</para>
    /// </summary>
    public int DefaultStringBuilderCapacity { get; set; } = 1024;

    /// <summary>
    /// Gets the existing instance of the pool or lazy-creates a new one, which is then added to the registry.
    /// </summary>
    public static StringBuilderPool Instance => PoolRegistry.GetOrAdd(Lazy.Value);
}
