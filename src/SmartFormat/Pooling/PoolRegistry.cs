// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;

namespace SmartFormat.Pooling;

/// <summary>
/// Registry for all object pools.
/// </summary>
internal static class PoolRegistry
{
    public static readonly ConcurrentDictionary<Type, object> Items = new();

    /// <summary>
    /// Gets the instance of the pool that already exists in the registry,
    /// or adds new pool to the registry and returns it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pool"></param>
    /// <returns>The instance of the pool that already exists, or that was newly added.</returns>
    public static T GetOrAdd<T>(T pool) where T: class
    {
        return (T) Items.GetOrAdd(typeof(T), pool);
    }

    /// <summary>
    /// Tries to remove the pool from the registry.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pool"></param>
    public static void TryRemove<T>(T pool) where T: class
    {
        Items.TryRemove(typeof(T), out _);
    }

    /// <summary>
    /// Gets the pool of type <typeparamref name="T"/> from the registry, or <see langword="null"/> if not found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The pool of type <typeparamref name="T"/> from the registry, or <see langword="null"/> if not found.</returns>
    public static T? Get<T>() where T: class
    {
        return (T?)Items[typeof(T)];
    }
}
