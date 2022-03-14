// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

/*
   Credits to Needle (https://github.com/needle-tools)
   and their PersistentVariablesSource extension to Smart.Format
   at https://github.com/needle-mirror/com.unity.localization/blob/master/Runtime/Smart%20Format/Extensions/PersistentVariablesSource.cs
*/
namespace SmartFormat.Extensions.PersistentVariables
{
    /// <summary>
    /// The class for the variable name and its corresponding <see cref="IVariable"/>.
    /// </summary>
    internal class NameVariablePair
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="variable">The <see cref="IVariable"/>.</param>
        public NameVariablePair(string name, IVariable variable)
        {
            Name = name;
            Variable = variable;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="IVariable"/> that corresponds to the variable name.
        /// </summary>
        public IVariable Variable { get; }

        /// <summary>
        /// Gets a string with the <see cref="Name"/>, <see cref="System.Type"/> and the value from <see cref="IVariable.GetValue()"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"'{Name}' - '{Variable.GetValue()?.GetType()}' - '{Variable.GetValue()}'";
    }
}