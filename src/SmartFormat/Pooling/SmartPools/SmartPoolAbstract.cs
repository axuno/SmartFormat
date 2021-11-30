//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools
{
    /// <summary>
    /// The abstract base class for specialized pools.
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
            return Pool.Get();
        }

        /// <summary>
        /// Gets a <see cref="PooledObject{T}"/> with a not yet initialized <see paramref="T"/> instance from the object pool.
        /// </summary>
        /// <returns>A <see cref="PooledObject{T}"/> with a not yet initialized <see paramref="T"/> instance from the object pool.</returns>
        public override PooledObject<T> Get(out T instance)
        {
            instance = Get();
            return new PooledObject<T>(instance, Pool);
        }
    }
}
