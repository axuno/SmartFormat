// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

/*
   Credits to Needle (https://github.com/needle-tools)
   and their PersistentVariablesSource extension to Smart.Format
   at https://github.com/needle-mirror/com.unity.localization/blob/master/Runtime/Smart%20Format/Extensions/PersistentVariablesSource.cs
*/
using System;

namespace SmartFormat.Extensions.PersistentVariables
{
    /// <summary>
    /// Collection that contains <see cref="IVariable"/>.
    /// </summary>
    public interface IVariablesGroup
    {
        /// <summary>
        /// Gets the variable with the matching key if one exists.
        /// </summary>
        /// <param name="name">The variable name to match.</param>
        /// <param name="value">The found variable or <c>null</c> if one could not be found.</param>
        /// <returns><see langword="true"/> if a variable could be found or <see langword="false"/> if one could not.</returns>
        bool TryGetValue(string name, out IVariable value);
    }

    /// <summary>
    /// Represents a variable that can be provided through a global <see cref="VariablesGroup"/>
    /// instead of as a <c>SmartFormat</c> argument.
    /// A variable can be a single variable, in which case the value should be returned by <see cref="GetValue"/>
    /// or a class with multiple variables (<see cref="VariablesGroup"/>) which can then be further extracted as <c>SmartFormat</c> arguments.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Gets the <see cref="Variable{T}.Value"/> boxed into a <see cref="Nullable"/> <see cref="object"/>.
        /// </summary>
        /// <returns>The <see cref="Variable{T}.Value"/> boxed into a <see cref="Nullable"/> <see cref="object"/>.</returns>
        object? GetValue();
    }
}
