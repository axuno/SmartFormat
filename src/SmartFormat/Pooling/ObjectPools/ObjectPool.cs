// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.ObjectPools;

/// <summary>
/// The abstract base class for object pool implementations.
/// </summary>
/// <typeparam name="T"><see langword="type"/> of the object pool elements.</typeparam>
internal abstract class ObjectPool<T> : IObjectPool<T> where T : class
{
    /// <summary>
    /// Creates a new object pool.
    /// </summary>
    protected ObjectPool(PoolPolicy<T> poolPolicy)
    {
        PoolPolicy = poolPolicy;
    }

    /// <summary>
    /// Gets whether the underlying object pool is thread safe.
    /// </summary>
    public abstract bool IsThreadSafeMode { get; }

    /// <summary>
    /// Indicates whether object pooling is enabled (<see langword="true"/>).
    /// <para>Default is taken from <see cref="PoolSettings.IsPoolingEnabled"/></para>
    /// </summary>
    public bool IsPoolingEnabled
    {
        get => PoolSettings.IsPoolingEnabled;
        internal set => PoolSettings.IsPoolingEnabled = value;
    }

    /// <summary>
    /// The configuration of how an <see cref="IObjectPool{U}"/> works.
    /// </summary>
    protected PoolPolicy<T> PoolPolicy { get; }

    #region : Public Members

    /// <summary>
    /// The total number of active and inactive objects.
    /// </summary>
    public abstract int CountAll { get; }

    /// <summary>
    /// Gets a <see cref="IReadOnlyList{T}"/> of the unused items in the object pool.
    /// </summary>
    public abstract IReadOnlyList<T> PoolItems { get; }

    /// <summary>
    /// Number of objects that have been created by the pool but are currently in use and have not yet been returned.
    /// </summary>
    public int CountActive => CountAll - CountInactive;

    /// <summary>
    /// Number of objects that are currently available in the pool.
    /// </summary>
    public abstract int CountInactive { get; }

    /// <summary>
    /// Get an object from the pool.
    /// </summary>
    /// <returns>
    /// An initialized object from the pool.
    /// </returns>
    public abstract T Get();

    /// <summary>
    /// Get a new <see cref="PooledObject{T}"/> which can be used to
    /// return the instance back to the pool when the <see cref="PooledObject{T}"/> is disposed.
    /// </summary>
    /// <param name="instance">Output new typed object.</param>
    /// <returns>
    /// A <see cref="PooledObject{T}"/>
    /// </returns>
    public PooledObject<T> Get(out T instance)
    {
        instance = Get();
        return new PooledObject<T>(instance, this);
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="element">Object to return to the pool.</param>
    public abstract void Return(T element);
        
    /// <summary>
    /// Releases all pooled objects so they can be garbage collected.
    /// Pooled items will be destroyed before they will be released to garbage collection.
    /// <see cref="CountAll"/>, <see cref="CountActive"/> and <see cref="CountInactive"/> are set to zero.
    /// </summary>
    /// <remarks>
    /// The method should be called from <see cref="Dispose(bool)"/>.
    /// </remarks>
    public abstract void Clear();

    /// <summary>
    /// Disposes the resources by calling the <see cref="Clear"/> method.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
