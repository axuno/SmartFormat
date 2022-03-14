// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;

namespace SmartFormat.Pooling.ObjectPools
{
    /// <summary>
    /// A <see cref="PooledObject{T}"/> wraps a reference to an instance that will be returned
    /// to the pool when the <see cref="PooledObject{T}"/> is disposed.
    /// The purpose is to automate the return of references so that they do not need to be returned manually.
    /// <example>
    /// <para>A <see cref="PooledObject{T}"/> can be used like so:</para>
    /// <code>
    /// MyClass myInstance;
    /// using(myPool.Get(out myInstance)) // When leaving the scope myInstance will be returned to the pool.
    /// {
    ///     // Do something with myInstance
    /// }
    /// </code>
    /// </example>
    /// </summary>
    internal readonly struct PooledObject<T> : IDisposable where T : class
    {
        private readonly T _value;
        private readonly IObjectPool<T> _pool;

        internal PooledObject(T value, IObjectPool<T> pool)
        {
            _value = value;
            _pool = pool;
        }

        void IDisposable.Dispose() => _pool.Return(_value);
    }
}

