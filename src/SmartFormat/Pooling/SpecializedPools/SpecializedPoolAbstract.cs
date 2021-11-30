using System;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.ObjectPools;

namespace SmartFormat.Pooling.SpecializedPools
{
    /// <summary>
    /// The abstract base class for specialized pools.
    /// </summary>
    /// <typeparam name="T">The <see langword="type"/> of the smart pool.</typeparam>
    internal abstract class SpecializedPoolAbstract<T> : IDisposable where T : class
    {
        private bool _isThreadSafeMode = SmartSettings.IsThreadSafeMode;

        /// <summary>
        /// The static <see cref="ObjectPool{T}"/> instance.
        /// </summary>
        internal ObjectPool<T> Pool { get; set; }

        /// <summary>
        /// The policy for the pool. Policy must be defined before initializing the pool.
        /// </summary>
        protected readonly PoolPolicy<T> Policy = new();

        /// <summary>
        /// CTOR.
        /// </summary>
        protected SpecializedPoolAbstract()
        {
            Pool = LazyCreateObjectPool();
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="ObjectPool{T}"/> and
        /// creates a new one, applying the current <see cref="PoolSettings"/> and <see cref="PoolPolicy{T}"/>.
        /// </summary>
        /// <param name="isThreadSafeMode">If <see langword="null"/>, the <see cref="SmartSettings.IsThreadSafeMode"/> will be used.</param>
        internal void Reset(bool? isThreadSafeMode)
        {
            _isThreadSafeMode = isThreadSafeMode ?? SmartSettings.IsThreadSafeMode;
            PoolRegistry.Remove(this);
            Pool.Dispose();
            Pool = LazyCreateObjectPool();
        }

        private ObjectPool<T> LazyCreateObjectPool()
        {
            return _isThreadSafeMode
                ? new Lazy<ObjectPoolConcurrent<T>>(() => new ObjectPoolConcurrent<T>(Policy),
                    System.Threading.LazyThreadSafetyMode.PublicationOnly).Value
                : new Lazy<ObjectPoolSingleThread<T>>(() => new ObjectPoolSingleThread<T>(Policy),
                    System.Threading.LazyThreadSafetyMode.None).Value;
        }

        /// <summary>
        /// Gets a <see typeparamref="T"/> instance from the object pool.
        /// </summary>
        /// <returns>A <see typeparamref="T"/> instance from the object pool.</returns>
        public virtual T Get()
        {
            if (!PoolSettings.IsPoolingEnabled) return Policy.FunctionOnCreate();
            return Pool.Get();
        }

        /// <summary>
        /// Get a new <see cref="PooledObject{T}"/> which can be used to
        /// return the instance back to the pool when the <see cref="PooledObject{T}"/> is disposed.
        /// </summary>
        /// <param name="instance">Output new typed object.</param>
        /// <returns>
        /// A <see cref="PooledObject{T}"/>
        /// </returns>
        public virtual PooledObject<T> Get(out T instance)
        {
            instance = Get();
            return new PooledObject<T>(instance, Pool);
        }

        /// <summary>
        /// The default method to return an instance to the pool.
        /// The method can be overriden in a derived class.
        /// </summary>
        /// <param name="toReturn"></param>
        public virtual void Return(T toReturn)
        {
            if (!PoolSettings.IsPoolingEnabled) return;
            Pool.Return(toReturn);
        }

        /// <summary>
        /// Releases all pooled objects so they can be garbage collected.
        /// Pooled items will be destroyed before they will be released to garbage collection.
        /// </summary>
        public virtual void Clear()
        {
            Pool.Clear();
        }

        /// <summary>
        /// Disposes the resources by calling the <see cref="Clear"/> method.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reset(null);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}