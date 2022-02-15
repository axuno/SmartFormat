// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace SmartFormat.Pooling.ObjectPools
{
    internal interface IObjectPool<T> : IDisposable, IPoolCounters where T : class
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyList{T}"/> of the unused items in the object pool.
        /// </summary>
        IReadOnlyList<T> PoolItems { get; }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <returns>
        /// An initialized object from the pool.
        /// </returns>
        T Get();

        /// <summary>
        /// Get a new <see cref="PooledObject{T}"/> which can be used to
        /// return the instance back to the pool when the <see cref="PooledObject{T}"/> is disposed.
        /// </summary>
        /// <param name="instance">Output new typed object.</param>
        /// <returns>
        /// A <see cref="PooledObject{T}"/>
        /// </returns>
        PooledObject<T> Get(out T instance);

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="element">Object to return to the pool.</param>
        void Return(T element);

        /// <summary>
        /// Releases all pooled objects so they can be garbage collected.
        /// Pooled items will be destroyed before they will be released to garbage collection.
        /// </summary>
        void Clear();
    }
}