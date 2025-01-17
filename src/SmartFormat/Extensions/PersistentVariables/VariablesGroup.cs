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
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions.PersistentVariables;

/// <summary>
/// A collection of <see cref="IVariable"/>s that can be used
/// as arguments to the Format(...) overloads of <see cref="SmartFormatter"/>,
/// or it can be added to a <seealso cref="PersistentVariablesSource"/> or a <seealso cref="GlobalVariablesSource.Instance"/>
/// <br/>
/// Each instance of <see cref="VariablesGroup"/> keeps its own collection.
/// <para>
/// One instance of <see cref="VariablesGroup"/> can be used from different threads,
/// if <see cref="SmartSettings.IsThreadSafeMode"/> is <see langword="true"/> when creating the instance.
/// </para>
/// </summary>
public class VariablesGroup : IVariablesGroup, IVariable, IDictionary<string, IVariable>
{
    private readonly IDictionary<string, NameVariablePair> _variableLookup = SmartSettings.IsThreadSafeMode
        ? new ConcurrentDictionary<string, NameVariablePair>()
        : new Dictionary<string, NameVariablePair>();

    /// <inheritdoc/>
    /// <summary>
    /// Gets the number of <see cref="IVariable"/>s in the group.
    /// </summary>
    public int Count => _variableLookup.Count;

    /// <summary>
    /// Gets an <see cref="ICollection"/> containing all the variable names.
    /// </summary>
    public ICollection<string> Keys => _variableLookup.Keys;

    /// <summary>
    /// Gets all the <see cref="IVariable"/>s for this group.
    /// </summary>
    /// <remarks>
    /// Just implemented as part of <see cref="IDictionary"/>.
    /// </remarks>
    public ICollection<IVariable> Values => _variableLookup.Values.Select(s => s.Variable).ToList(); //NOSONAR

    /// <summary>
    /// Always returns <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Just implemented as part of <see cref="IDictionary"/>.
    /// </remarks>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the <see cref="IVariable"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the <see cref="IVariable"/>.</param>
    /// <returns>The found variable.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if a variable with the specified name does not exist.</exception>
    public IVariable this[string name]
    {
        get => _variableLookup[name].Variable;
        set => Add(name, value);
    }

    /// <inheritdoc/>
    public object GetValue() => this;

    /// <summary>
    /// Gets the <see cref="IVariable"/> with the specified name from this <see cref="VariablesGroup"/>.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The variable that was found or <c>default</c>.</param>
    /// <returns><see langword="true"/> if a variable was found and <see langword="false"/> if one could not.</returns>
    public bool TryGetValue(string name, out IVariable value)
    {
        if (_variableLookup.TryGetValue(name, out var v))
        {
            value = v.Variable;
            return true;
        }

        value = default!;
            
        return false;
    }

    /// <summary>
    /// Adds a new <see cref="IVariable"/> to the group.
    /// </summary>
    /// <param name="name">The name of the variable, must be unique and must only contain selector characters which are also accepted by the <see cref="Core.Parsing.Parser"/>.</param>
    /// <param name="variable">The variable to use when formatting. See also <seealso cref="BoolVariable"/>, <seealso cref="FloatVariable"/>, <seealso cref="IntVariable"/>, <seealso cref="StringVariable"/>, <seealso cref="ObjectVariable"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
    public void Add(string name, IVariable variable)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name must not be null or empty.", nameof(name));

        var v = new NameVariablePair(name, variable);
        _variableLookup.Add(name, v);
    }

    /// <summary>
    /// <inheritdoc cref="Add(string, IVariable)"/>
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<string, IVariable> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Removes an <see cref="IVariable"/> with the specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns><see langword="true"/> if a variable with the specified name was removed, <see langword="false"/> if one was not.</returns>
    public bool Remove(string name)
    {
        return _variableLookup.Remove(name);
    }

    /// <summary>
    /// Removes an <see cref="IVariable"/> with the specified key.
    /// </summary>
    /// <param name="item">The item to be removed. Only the <see cref="KeyValuePair{TKey,TValue}.Key"/> field will be considered.</param>
    /// <returns><see langword="true"/> if a variable with the specified name was removed, <see langword="false"/> if one was not.</returns>
    public bool Remove(KeyValuePair<string, IVariable> item) => Remove(item.Key);

    /// <summary>
    /// Returns <see langword="true"/> if a variable with the specified name exists.
    /// </summary>
    /// <param name="name">The variable name to check for.</param>
    /// <returns><see langword="true"/> if a matching variable could be found or <see langword="false"/> if one could not.</returns>
    public bool ContainsKey(string name) => _variableLookup.ContainsKey(name);

    /// <summary>
    /// <inheritdoc cref="ContainsKey(string)"/>
    /// </summary>
    /// <param name="item">The item to check for. Both the Key and Value must match.</param>
    /// <returns><see langword="true"/> if a matching variable could be found or <see langword="false"/> if one could not.</returns>
    public bool Contains(KeyValuePair<string, IVariable> item) => TryGetValue(item.Key, out var v) && v == item.Value;

    /// <summary>
    /// Copies the variables into an array starting at <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The array to copy the variables into.</param>
    /// <param name="arrayIndex">The index to start copying the items into.</param>
    public void CopyTo(KeyValuePair<string, IVariable>[] array, int arrayIndex)
    {
        foreach (var entry in _variableLookup)
            array[arrayIndex++] = new KeyValuePair<string, IVariable>(entry.Key, entry.Value.Variable);
    }

    /// <summary>
    /// Creates a Shallow Copy of this <see cref="VariablesGroup"/>.
    /// </summary>
    /// <returns>A Shallow Copy of this <see cref="VariablesGroup"/>.</returns>
    public VariablesGroup Clone()
    {
        var vg = new VariablesGroup();
        foreach (var entry in _variableLookup)
            vg.Add(entry.Key, entry.Value.Variable);

        return vg;
    }

    /// <summary>
    /// <inheritdoc cref="GetEnumerator"/>
    /// </summary>
    /// <returns>The enumerator that can be used to iterate through all the variables.</returns>
    IEnumerator<KeyValuePair<string, IVariable>> IEnumerable<KeyValuePair<string, IVariable>>.GetEnumerator()
    {
        return _variableLookup.Select(v => new KeyValuePair<string, IVariable>(v.Key, v.Value.Variable)).GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator for all variables in this group.
    /// </summary>
    /// <returns>The enumerator that can be used to iterate through all the variables.</returns>
    public IEnumerator GetEnumerator()
    {
        return _variableLookup.Select(v => new KeyValuePair<string, IVariable>(v.Key, v.Value.Variable)).GetEnumerator();
    }

    /// <summary>
    /// Removes all variables in the group.
    /// </summary>
    public void Clear()
    {
        _variableLookup.Clear();
    }
}
