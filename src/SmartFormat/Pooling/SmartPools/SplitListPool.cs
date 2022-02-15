// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Threading;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Pooling.SmartPools
{
    /// <summary>
    /// The object pool for <see cref="SplitList"/>.
    /// </summary>
    internal sealed class SplitListPool : SmartPoolAbstract<SplitList>
    {
        // Note: The pool for the SplitList can be thread safe,
        // but this is not needed for the SplitList elements of the pool (which are an IList<Format>).
        private static readonly Lazy<SplitListPool> Lazy = new(() => new SplitListPool(),
            SmartSettings.IsThreadSafeMode
                ? LazyThreadSafetyMode.PublicationOnly
                : LazyThreadSafetyMode.None);
        
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <remarks>
        /// <see cref="SpecializedPoolAbstract{T}.Policy"/> must be set before initializing the pool
        /// </remarks>
        private SplitListPool()
        {
            Policy.FunctionOnCreate = () => new SplitList();
            Policy.ActionOnReturn = sl => sl.Clear();
        }

        /// <inheritdoc/>
        public override void Return(SplitList toReturn)
        {
            if (ReferenceEquals(toReturn, InitializationObject.SplitList)) throw new PoolingException($"{nameof(InitializationObject)}s cannot be returned to the pool.", GetType());
            base.Return(toReturn);
        }

        /// <summary>
        /// Gets a singleton instance of the pool.
        /// </summary>
        public static SplitListPool Instance =>
            Lazy.IsValueCreated ? Lazy.Value : PoolRegistry.Add(Lazy.Value);
    }
}