using System;

namespace SmartFormat.Pooling.ObjectPools
{
    /// <summary>
    /// Defines the configuration of how an <see cref="IObjectPool{T}"/> works.
    /// </summary>
    /// <typeparam name="T">The type of item being pooled.</typeparam>
    internal class PoolPolicy<T> where T : class
    {
        private uint _maximumPoolSize = 10000;

        /// <summary>
        /// Determines the maximum number of items allowed in the pool, which must not be zero.
        /// </summary>
        /// <remarks>
        /// <para>This restricts the number of instances stored in the pool at any given time.
        /// It does not represent the maximum number of items that may be generated or exist in memory at any given time.
        /// If the pool is empty and a new item is requested, a new instance will be created,
        /// even if pool was previously full and all it's instances have been taken already.</para>
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public uint MaximumPoolSize
        {
            get => _maximumPoolSize;
            set => _maximumPoolSize = value > 0
                ? value
                : throw new PoolingException($"Policy for {nameof(MaximumPoolSize)} size must be greater than 0", typeof(T));
        }

        /// <summary>
        /// The initial capacity the item store will be created with. May not be used in <see cref="IObjectPool{T}"/> implementations.
        /// Default is 10.
        /// </summary>
        public int InitialPoolSize { get; set; } = 10;

        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        /// <remarks>
        /// <para>Should return a new, clean item, ready for use by the caller. Takes a single argument being a reference to the pool that was asked for the object, useful if you're creating <see cref="PooledObject{T}"/> instances.</para>
        /// <para>May not be <see langword="null"/>. If <see langword="null"/> when provided to an <see cref="IObjectPool{T}"/> instance, an <see cref="ArgumentNullException"/> will be thrown.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<T> FunctionOnCreate { get; set; } = () =>
            throw new PoolingException($"'{nameof(FunctionOnCreate)}' is not set in {nameof(PoolPolicy<T>)}.", typeof(T));

        /// <summary>
        /// Called when the element cannot be returned to the pool because this would exceed the maximal pool size.
        /// </summary>
        public Action<T>? ActionOnDestroy { get; set; }

        /// <summary>
        /// Called when an item is being taken from the pool.
        /// Should return an initialized, clean item, ready for use by the caller.
        /// </summary>
        public Action<T>? ActionOnGet { get; set; }

        /// <summary>
        /// Called when an item is being returned to the pool.
        /// This could be used to clean up or disable the instance.
        /// </summary>
        public Action<T>? ActionOnReturn { get; set; }
    }
}
