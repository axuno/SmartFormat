// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.ObjectPools
{
    /// <summary>
    /// Generic linked object pool implementation optimized for single thread use cases.
    /// It uses a linked list of pool items instead of a collection, and is not thread-safe.
    /// </summary>
    /// <typeparam name="T"><see langword="type"/> of the object pool elements.</typeparam>
    internal class LinkedPool<T> : ObjectPool<T> where T : class
    {
        internal class LinkedPoolItem
        {
            internal LinkedPoolItem? PoolNext;
            internal T? Value;
        }

        internal LinkedPoolItem? PoolFirst; // The pool of available T objects
        internal LinkedPoolItem? NextAvailableListItem; // When Get is called we place the node here for reuse and to prevent GC
        private int _countAll;
        private int _countInactive;

        ///<inheritdoc/>
        public LinkedPool(PoolPolicy<T> poolPolicy) : base(poolPolicy)
        {
        }

        ///<inheritdoc/>
        public override bool IsThreadSafeMode => false;

        ///<inheritdoc/>
        public override int CountAll => _countAll;

        ///<inheritdoc/>
        public override IReadOnlyList<T> PoolItems => GetAllPoolItems().Where(i => i.Value != null)
            .Select(i => i.Value!).ToList().AsReadOnly();
        public override int CountInactive => _countInactive;

        ///<inheritdoc/>
        public override T Get()
        {
            // Always just create a new instance, if pooling is disabled
            if (!IsPoolingEnabled) return PoolPolicy.FunctionOnCreate();

            T item;
            if (PoolFirst == null)
            {
                item = PoolPolicy.FunctionOnCreate();
                _countAll++;
                return item;
            }
            
            var first = PoolFirst;
            item = first.Value!;
            PoolFirst = first.PoolNext;

            // Add the empty node to our pool for reuse and to prevent GC
            first.PoolNext = NextAvailableListItem;
            NextAvailableListItem = first;
            NextAvailableListItem.Value = null;
            _countInactive--;
            
            PoolPolicy.ActionOnGet?.Invoke(item);
            return item;
        }

        ///<inheritdoc/>
        public override void Return(T element)
        {
            // Never put an instance to the stack, if pooling is disabled
            if (!IsPoolingEnabled) return;

            var listItem = PoolFirst;
            while (listItem != null)
            {
                // This is a safe, but expensive
                if (PoolSettings.CheckReturnedObjectsExistInPool && ReferenceEquals(listItem.Value, element))
                    throw new PoolingException("Trying to return an object that has already been returned to the pool.", GetType());
                listItem = listItem.PoolNext;
            }

            PoolPolicy.ActionOnReturn?.Invoke(element);

            if (CountInactive < PoolPolicy.MaximumPoolSize)
            {
                var poolItem = NextAvailableListItem;
                if (poolItem == null)
                {
                    poolItem = new LinkedPoolItem();
                }
                else
                {
                    NextAvailableListItem = poolItem.PoolNext;
                }

                poolItem.Value = element;
                poolItem.PoolNext = PoolFirst;
                PoolFirst = poolItem;
                _countInactive++;
            }
            else
            {
                PoolPolicy.ActionOnDestroy?.Invoke(element);
            }
        }

        ///<inheritdoc/>
        public override void Clear()
        {
            if (PoolPolicy.ActionOnDestroy != null)
            {
                foreach (var item in GetAllPoolItems().Select(item => item.Value))
                {
                    if (item != null) PoolPolicy.ActionOnDestroy(item);
                }
            }

            PoolFirst = null;
            NextAvailableListItem = null;
            _countInactive = 0;
            _countAll = 0;
        }

        private IEnumerable<LinkedPoolItem> GetAllPoolItems()
        {
            for (var item = PoolFirst; item != null; item = item.PoolNext)
            {
                if (item.Value != null) yield return item;
            }
        }
    }
}

