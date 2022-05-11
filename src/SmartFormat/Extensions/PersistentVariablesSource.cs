// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

/*
   Credits to Unity Technologies (https://unity.com/)
   and their PersistentVariablesSource extension to Smart.Format
   at https://docs.unity3d.com/Packages/com.unity.localization@latest
   and https://docs.unity3d.com/Packages/com.unity.localization@1.3/manual/Smart/SmartStrings.html
*/
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
    /// Provides persistent variables of type <see cref="VariablesGroup"/> to the <see cref="SmartFormatter"/>
    /// that do not need to be passed in as arguments when formatting a string.
    /// <para>The smart string should take the placeholder format like {groupName.variableName}.</para>
    /// <para>Note: <see cref="IVariablesGroup"/>s from args to SmartFormatter.Format(...) take precedence over <see cref="PersistentVariablesSource"/>.</para>
    /// </summary>
    public class PersistentVariablesSource : Source, IDictionary<string, VariablesGroup>
    {
        /// <summary>
        /// Contains <see cref="VariablesGroup"/>s and their name.
        /// </summary>
        protected internal class NameGroupPair
        {
            /// <summary>
            /// CTOR.
            /// </summary>
            /// <param name="name">The name of the <see cref="VariablesGroup"/>.</param>
            /// <param name="group">The <see cref="VariablesGroup"/>.</param>
            public NameGroupPair(string name, VariablesGroup group)
            {
                Name = name;
                Group = group;
            }

            /// <summary>
            /// Gets the name of the <see cref="VariablesGroup"/>.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the <see cref="VariablesGroup"/>.
            /// </summary>
            public VariablesGroup Group { get; }
        }

        /// <summary>
        /// The container for <see cref="VariablesGroup"/>s.
        /// </summary>
        protected IDictionary<string, NameGroupPair> GroupLookup = SmartSettings.IsThreadSafeMode
            ? new ConcurrentDictionary<string, NameGroupPair>()
            : new Dictionary<string, NameGroupPair>();

        /// <summary>
        /// The number of stored variables.
        /// </summary>
        public int Count => GroupLookup.Values.Count;

        /// <summary>
        /// Implemented as part of IDictionary. Will always return <see langword="false"/>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the names of stored <see cref="VariablesGroup"/>s.
        /// </summary>
        public ICollection<string> Keys => GroupLookup.Keys;

        /// <summary>
        /// Gets the values of stored <see cref="VariablesGroup"/>s.
        /// </summary>
        /// <remarks>
        /// Just implemented as part of <see cref="IDictionary"/>.
        /// </remarks>
        public ICollection<VariablesGroup> Values => GroupLookup.Values.Select(k => k.Group).ToList(); //NOSONAR

        /// <summary>
        /// Gets the <see cref="VariablesGroup"/> that matches the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="VariablesGroup"/> to return.</param>
        /// <returns>The <see cref="VariablesGroup"/> that matches <paramref name="name"/></returns>
        public VariablesGroup this[string name]
        {
            get => GroupLookup[name].Group;
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
            if (GroupLookup.TryGetValue(name, out var v))
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

            GroupLookup[name] = pair;
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
            if (GroupLookup.TryGetValue(name, out var v))
            {
                GroupLookup.Remove(name);
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
            GroupLookup.Clear();
        }

        /// <summary>
        /// Returns <see langword="true"/> if a <see cref="VariablesGroup"/> is found with the same name.
        /// </summary>
        /// <param name="name">The name of the global variable group to check for.</param>
        /// <returns><see langword="true"/> if a <see cref="VariablesGroup"/> is found with the same name.</returns>
        public bool ContainsKey(string name) => GroupLookup.ContainsKey(name);

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
            foreach (var entry in GroupLookup)
                array[arrayIndex++] = new KeyValuePair<string, VariablesGroup>(entry.Key, entry.Value.Group);
        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentVariablesSource"/>, respecting current <see cref="SmartSettings"/>,
        /// with containing variables as a Shallow Copy.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="PersistentVariablesSource"/>, respecting current <see cref="SmartSettings"/>,
        /// with containing variables as a Shallow Copy.
        /// </returns>
        public PersistentVariablesSource Clone()
        {
            var pvs = new PersistentVariablesSource();
            foreach (var entry in GroupLookup)
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
            return GroupLookup.Select(v => new KeyValuePair<string, VariablesGroup>(v.Key, v.Value.Group)).GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for all <see cref="VariablesGroup"/>s in the source.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return GroupLookup.Select(v => new KeyValuePair<string, VariablesGroup>(v.Key, v.Value.Group)).GetEnumerator();
        }

        /// <inheritdoc/>
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            switch (selectorInfo.CurrentValue)
            {
                case null when TrySetResultForNullableOperator(selectorInfo):
                    return true;

                // Next, we test the current value
                // If selectorInfo.SelectorOperator == string.Empty, the CurrentValue comes from an arg to the SmartFormatter.Format(...)
                // IVariablesGroups from args have priority over PersistentVariablesSource
                case IVariablesGroup grp when TryEvaluateGroup(selectorInfo, grp):
                    return true;
            }

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
