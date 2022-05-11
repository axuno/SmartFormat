// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

/*
   Credits to Unity Technologies (https://unity.com/)
   and their PersistentVariablesSource extension to Smart.Format
   at https://docs.unity3d.com/Packages/com.unity.localization@latest
   and https://docs.unity3d.com/Packages/com.unity.localization@1.3/manual/Smart/SmartStrings.html
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
