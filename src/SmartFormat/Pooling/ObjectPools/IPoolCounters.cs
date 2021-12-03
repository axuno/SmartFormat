//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

namespace SmartFormat.Pooling.ObjectPools
{
    internal interface IPoolCounters
    {
        /// <summary>
        /// The total number of active and inactive objects.
        /// </summary>
        int CountAll { get; }

        /// <summary>
        /// Number of objects that have been created by the pool but are currently in use and have not yet been returned.
        /// </summary>
        int CountActive { get; }

        /// <summary>
        /// Number of objects that are currently available in the pool.
        /// </summary>
        int CountInactive { get; }
    }
}
