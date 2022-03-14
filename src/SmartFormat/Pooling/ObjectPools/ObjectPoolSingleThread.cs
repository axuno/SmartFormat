// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.ObjectPools
{
    /// <summary>
    /// Generic object pool implementation optimized for single thread use cases.
    /// It is not thread-safe, has better performance compared to synchronized implementations.
    /// </summary>
    /// <typeparam name="T"><see langword="type"/> of the object pool elements.</typeparam>
    internal class ObjectPoolSingleThread<T> : ObjectPool<T> where T : class
    {
        private readonly Stack<T> _stack;
        private int _countAll;   

        /// <inheritdoc/>
        public ObjectPoolSingleThread(PoolPolicy<T> poolPolicy) : base(poolPolicy)
        {
            _stack = new Stack<T>(poolPolicy.InitialPoolSize);
        }

        ///<inheritdoc/>
        public override bool IsThreadSafeMode => false;

        /// <inheritdoc/>
        public override int CountAll => _countAll;

        /// <inheritdoc/>
        public override IReadOnlyList<T> PoolItems => _stack.ToList().AsReadOnly();

        /// <inheritdoc/>
        public override int CountInactive => _stack.Count;

        /// <inheritdoc/>
        public override T Get()
        {
            // Always just create a new instance, if pooling is disabled
            if (!IsPoolingEnabled) return PoolPolicy.FunctionOnCreate();

            T element;
            if (_stack.Count == 0)
            {
                element = PoolPolicy.FunctionOnCreate();
                _countAll++;
                return element;
            }

            try
            {
                element = _stack.Pop();
            }
            catch
            {
                // No element could be taken from stack (emptied by another thread?)
                element = PoolPolicy.FunctionOnCreate();
                Interlocked.Increment(ref _countAll);
                return element;
            }

            PoolPolicy.ActionOnGet?.Invoke(element);
            return element;
        }

        /// <inheritdoc/>
        public override void Return(T element)
        {
            // Never put an instance to the stack, if pooling is disabled
            if (!IsPoolingEnabled) return;

            // This is a safe, but expensive check
            if (PoolSettings.CheckReturnedObjectsExistInPool && _stack.Count > 0 && _stack.Contains(element))
                throw new PoolingException(
                    $"Trying to return an object of type '{element.GetType()}', that has already been returned to the pool.", GetType());

            PoolPolicy.ActionOnReturn?.Invoke(element);

            if (CountInactive < PoolPolicy.MaximumPoolSize)
                _stack.Push(element);
            else
                PoolPolicy.ActionOnDestroy?.Invoke(element);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            if (PoolPolicy.ActionOnDestroy != null)
                foreach (var item in _stack)
                    PoolPolicy.ActionOnDestroy(item);

            _stack.Clear();
            _countAll = 0;
        }
    }
}
