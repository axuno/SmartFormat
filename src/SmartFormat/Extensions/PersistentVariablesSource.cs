// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions.PersistentVariables;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Provides (global) variables of type <see cref="VariablesGroup"/> to the <see cref="SmartFormatter"/>
    /// that do not need to be passed in as arguments when formatting a string.
    /// <para>The smart string should take the placeholder format like {groupName.variableName}.</para>
    /// <para>Note: <see cref="IVariablesGroup"/>s from args to SmartFormatter.Format(...) take precedence over <see cref="PersistentVariablesSource"/>.</para>
    /// </summary>
    public class PersistentVariablesSource : ISource, IDictionary<string, VariablesGroup>
    {
        internal class NameGroupPair
        {
            public NameGroupPair(string name, VariablesGroup group)
            {
                Name = name;
                Group = group;
            }

            public string Name { get; }

            public VariablesGroup Group { get; }
        }

        private readonly IDictionary<string, NameGroupPair> _groupLookup = SmartSettings.IsThreadSafeMode
            ? new ConcurrentDictionary<string, NameGroupPair>()
            : new Dictionary<string, NameGroupPair>();

        /// <summary>
        /// The number of stored variables.
        /// </summary>
        public int Count => _groupLookup.Values.Count;

        /// <summary>
        /// Implemented as part of IDictionary. Will always return <see langword="false"/>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the names of stored <see cref="VariablesGroup"/>s.
        /// </summary>
        public ICollection<string> Keys => _groupLookup.Keys;

        /// <summary>
        /// Gets the values of stored <see cref="VariablesGroup"/>s.
        /// </summary>
        #pragma warning disable CA1819 // Properties should not make collection or array copies
        public ICollection<VariablesGroup> Values => _groupLookup.Values.Select(k => k.Group).ToList();
        #pragma warning restore CA1819 // Properties should not make collection or array copies

        /// <summary>
        /// Gets the <see cref="VariablesGroup"/> that matches <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="VariablesGroup"/> to return.</param>
        /// <returns></returns>
        public VariablesGroup this[string name]
        {
            get => _groupLookup[name].Group;
            set => Add(name, value);
        }

        /// <summary>
        /// Returns <see langword="true"/> if a <see cref="VariablesGroup"/> could be found with a matching name, or <see langword="false"/> if one could not.
        /// </summary>
        /// <param name="name">The name of the <see cref="VariablesGroup"/> to find.</param>
        /// <param name="value">The found <see cref="VariablesGroup"/> or <c>default</c> if one could not be found with a matching name.</param>
        /// <returns><see langword="true"/> if a group could be found or <see langword="false"/> if one could not.</returns>
        public bool TryGetValue(string name, out VariablesGroup value)
        {
            if (_groupLookup.TryGetValue(name, out var v))
            {
                value = v.Group;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Add a <see cref="VariablesGroup"/> to the source.
        /// </summary>
        /// <param name="name">The name of the <see cref="VariablesGroup"/> to add.</param>
        /// <param name="group">The <see cref="VariablesGroup"/> to add.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is <c>null</c> or empty.</exception>
        public void Add(string name, VariablesGroup group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name must not be null or empty.", nameof(name));

            var pair = new NameGroupPair(name, group);

            _groupLookup[name] = pair;
        }

        /// <inheritdoc cref="Add(string, VariablesGroup)"/>
        public void Add(KeyValuePair<string, VariablesGroup> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes the <see cref="VariablesGroup"/> with the matching name.
        /// </summary>
        /// <param name="name">The name of the <see cref="VariablesGroup"/> to remove.</param>
        /// <returns><see langword="true"/> if a <see cref="VariablesGroup"/> with a matching name was found and removed, or <see langword="true"/> if one was not.</returns>
        public bool Remove(string name)
        {
            if (_groupLookup.TryGetValue(name, out var v))
            {
                _groupLookup.Remove(name);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="Remove(string)"/>
        public bool Remove(KeyValuePair<string, VariablesGroup> item) => Remove(item.Key);

        /// <summary>
        /// Removes all <see cref="VariablesGroup"/>s.
        /// </summary>
        public void Clear()
        {
            _groupLookup.Clear();
        }

        /// <summary>
        /// Returns <see langword="true"/> if a <see cref="VariablesGroup"/> is found with the same name.
        /// </summary>
        /// <param name="name">The name of the global variable group to check for.</param>
        /// <returns><see langword="true"/> if a <see cref="VariablesGroup"/> is found with the same name.</returns>
        public bool ContainsKey(string name) => _groupLookup.ContainsKey(name);

        /// <inheritdoc cref="ContainsKey(string)"/>
        public bool Contains(KeyValuePair<string, VariablesGroup> item) =>
            TryGetValue(item.Key, out var v) && v == item.Value;

        /// <summary>
        /// Copy all <see cref="VariablesGroup"/>s into the provided array starting at <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">The array to copy the global variables into.</param>
        /// <param name="arrayIndex">The index to start copying into.</param>
        public void CopyTo(KeyValuePair<string, VariablesGroup>[] array, int arrayIndex)
        {
            foreach (var entry in _groupLookup)
                array[arrayIndex++] = new KeyValuePair<string, VariablesGroup>(entry.Key, entry.Value.Group);
        }

        /// <summary>
        /// Creates a Shallow Copy of this <see cref="PersistentVariablesSource"/> instance.
        /// </summary>
        /// <returns>A Shallow Copy of this <see cref="PersistentVariablesSource"/> instance.</returns>
        public PersistentVariablesSource Clone()
        {
            var pvs = new PersistentVariablesSource();
            foreach (var entry in _groupLookup)
                pvs.Add(entry.Key, entry.Value.Group);

            return pvs;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> for all the <see cref="VariablesGroup"/>s in the source.
        /// </summary>
        /// <returns></returns>
        IEnumerator<KeyValuePair<string, VariablesGroup>> IEnumerable<KeyValuePair<string, VariablesGroup>>.
            GetEnumerator()
        {
            return _groupLookup.Select(v => new KeyValuePair<string, VariablesGroup>(v.Key, v.Value.Group)).GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for all <see cref="VariablesGroup"/>s in the source.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _groupLookup.Select(v => new KeyValuePair<string, VariablesGroup>(v.Key, v.Value.Group)).GetEnumerator();
        }

        /// <inheritdoc/>
        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // First, we test the current value
            // If selectorInfo.SelectorOperator== string.Empty, the CurrentValue comes from an arg to the SmartFormatter.Format(...)
            // IVariablesGroups from args have priority over PersistentVariablesSource
            if (selectorInfo.CurrentValue is IVariablesGroup grp && TryEvaluateGroup(selectorInfo, grp)) 
                return true;

            if (TryGetValue(selectorInfo.SelectorText, out var group))
            {
                selectorInfo.Result = group;
                return true;
            }

            return false;
        }

        private static bool TryEvaluateGroup(ISelectorInfo selectorInfo, IVariablesGroup variablesGroup)
        {
            if (!variablesGroup.TryGetValue(selectorInfo.SelectorText, out var variable)) return false;
            
            selectorInfo.Result = variable.GetValue();
            return true;
        }
    }
}
