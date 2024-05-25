// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.ObjectPools;

/// <summary>
/// Generic, thread-safe object pool implementation.
/// </summary>
/// <typeparam name="T"><see langword="type"/> of the object pool elements.</typeparam>
internal class ObjectPoolConcurrent<T> : ObjectPool<T> where T : class
{
    private readonly ConcurrentStack<T> _stack;
    private int _countAll;

    ///<inheritdoc/>
    public ObjectPoolConcurrent(PoolPolicy<T> poolPolicy) : base(poolPolicy)
    {
        _stack = new ConcurrentStack<T>();
    }

    ///<inheritdoc/>
    public override bool IsThreadSafeMode => true;

    ///<inheritdoc/>
    public override int CountAll => _countAll;

    ///<inheritdoc/>
    public override IReadOnlyList<T> PoolItems => _stack.ToList().AsReadOnly();

    ///<inheritdoc/>
    public override int CountInactive  => _stack.Count;

    ///<inheritdoc/>
    public override T Get()
    {
        // Always just create a new instance, if pooling is disabled
        if (!PoolSettings.IsPoolingEnabled) return PoolPolicy.FunctionOnCreate();

        if (!_stack.TryPop(out var element))
        {
            element = PoolPolicy.FunctionOnCreate();
            Interlocked.Increment(ref _countAll);
            return element;
        }

        PoolPolicy.ActionOnGet?.Invoke(element);
        return element;
    }

    ///<inheritdoc/>
    public override void Return(T element)
    {
        // Never put an instance to the stack, if pooling is disabled
        if (!PoolSettings.IsPoolingEnabled) return;
            
        // This is a safe, but expensive check
        if (PoolSettings.CheckReturnedObjectsExistInPool && !_stack.IsEmpty && _stack.Contains(element))
            throw new PoolingException(
                $"Trying to return an object of type '{element.GetType()}', that has already been returned to the pool.", GetType());

        PoolPolicy.ActionOnReturn?.Invoke(element);

        if (CountInactive < PoolPolicy.MaximumPoolSize)
            _stack.Push(element);
        else
            PoolPolicy.ActionOnDestroy?.Invoke(element);
    }

    ///<inheritdoc/>
    public override void Clear()
    {
        if (PoolPolicy.ActionOnDestroy != null)
            foreach (var item in _stack)
                PoolPolicy.ActionOnDestroy(item);

        _stack.Clear();
        _countAll = 0;
    }
}
