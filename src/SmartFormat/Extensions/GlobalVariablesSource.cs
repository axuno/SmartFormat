//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions.PersistentVariables;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Provides global (static) variables of type <see cref="VariablesGroup"/> to the <see cref="SmartFormatter"/>
    /// that do not need to be passed in as arguments when formatting a string.
    /// <para>The smart string should take the placeholder format like {groupName.variableName}.</para>
    /// <para>Note: <see cref="IVariablesGroup"/>s from args to SmartFormatter.Format(...) take precedence over <see cref="PersistentVariablesSource"/>.</para>
    /// </summary>
    public class GlobalVariablesSource : PersistentVariablesSource
    {
        private readonly IDictionary<string, NameGroupPair> _globalGroupLookup = SmartSettings.IsThreadSafeMode
            ? new ConcurrentDictionary<string, NameGroupPair>()
            : new Dictionary<string, NameGroupPair>();

        private static Lazy<GlobalVariablesSource> _lazySource = new(() => new GlobalVariablesSource(),
            SmartSettings.IsThreadSafeMode
                ? LazyThreadSafetyMode.PublicationOnly
                : LazyThreadSafetyMode.None);

        private GlobalVariablesSource()
        {
            GroupLookup = _globalGroupLookup;
        }

        /// <summary>
        /// Initializes the current <see cref="GlobalVariablesSource.Instance"/> with a new, empty instance.
        /// </summary>
        public static void Reset()
        {
            _lazySource = new Lazy<GlobalVariablesSource>(() => new GlobalVariablesSource(),
                SmartSettings.IsThreadSafeMode
                    ? LazyThreadSafetyMode.PublicationOnly
                    : LazyThreadSafetyMode.None);
        }

        /// <summary>
        /// Gets the static instance of the <see cref="GlobalVariablesSource"/>.
        /// </summary>
        public static GlobalVariablesSource Instance => _lazySource.Value;
    }
}
