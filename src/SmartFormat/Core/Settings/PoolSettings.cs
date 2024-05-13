// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

namespace SmartFormat.Core.Settings;

/// <summary>
/// Contains global static settings for all object pools.
/// </summary>
public struct PoolSettings
{
    /// <summary>
    /// Gets or sets whether object pools will tackle created and returned objects.
    /// If <see langword="false"/>, the pools are still used for creating new instances, but without tracking.
    /// <para>The setting is respected by all subclasses of <see cref="Pooling.SpecializedPools.SpecializedPoolAbstract{T}"/>.</para>
    /// Default is <see langword="true"/>.<br/>
    /// This setting will have immediate effect at any time.
    /// </summary>
    public static bool IsPoolingEnabled { get; set; } = true;

    /// <summary>
    /// Defines, whether trying to return an object, that has already been
    /// returned to the pool should throw.
    /// <para>This setting will have immediate effect at any time.</para>
    /// </summary>
    /// <remarks>
    /// Should be <see langword="true"/> when debugging.
    /// Enabling has a performance drawback.
    /// </remarks>
    public static bool CheckReturnedObjectsExistInPool { get; set; }
#if DEBUG
        = true;
#else
        = false;
#endif
}
