// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools;

/// <summary>
/// The abstract base class for smart pools.
/// </summary>
/// <typeparam name="T">The <see langword="type"/> of the smart pool.</typeparam>
internal abstract class SmartPoolAbstract<T> : SpecializedPoolAbstract<T> where T : class
{
    /// <summary>
    /// Gets a not yet initialized <see typeparamref ="T"/> instance from the object pool.
    /// </summary>
    /// <returns>A not yet initialized <see typeparamref ="T"/> instance from the object pool.</returns>
    public override T Get()
    {
        return base.Get();
    }

    /// <summary>
    /// Gets a <see cref="PooledObject{T}"/> with a not yet initialized <see paramref="T"/> instance from the object pool.
    /// </summary>
    /// <returns>A <see cref="PooledObject{T}"/> with a not yet initialized <see paramref="T"/> instance from the object pool.</returns>
    public override PooledObject<T> Get(out T instance)
    {
        return base.Get(out instance);
    }
}