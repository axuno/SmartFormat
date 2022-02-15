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
    /// Base class for all single source variables.
    /// </summary>
    /// <typeparam name="T">The value type to store in this variable.</typeparam>
    public class Variable<T> : IVariable
    {
        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <param name="value">The value of the variable.</param>
        public Variable(T? value)
        {
            Value = value;
        }

        /// <summary>
        /// The value for the <see cref="Variable{T}"/>.
        /// </summary>
        public T? Value { get; set; }

        /// <inheritdoc/>
        public object? GetValue() => Value;

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// A <see cref="IVariable"/> that holds a single float value.
    /// </summary>
    public class FloatVariable : Variable<float?>
    {
        /// <inheritdoc/>
        public FloatVariable(float? value) : base(value) {}
    }

    /// <summary>
    /// A <see cref="IVariable"/> that holds a single string value.
    /// </summary>
    public class StringVariable : Variable<string>
    {
        /// <inheritdoc/>
        public StringVariable(string? value) : base(value) {}
    }

    /// <summary>
    /// A <see cref="IVariable"/> that holds a single integer value.
    /// </summary>
    public class IntVariable : Variable<int?>
    {
        /// <inheritdoc/>
        public IntVariable(int? value) : base(value) {}
    }

    /// <summary>
    /// A <see cref="IVariable"/> that holds a single boolean value.
    /// </summary>
    public class BoolVariable : Variable<bool?>
    {
        /// <inheritdoc/>
        public BoolVariable(bool? value) : base(value) {}
    }

    /// <summary>
    /// A <see cref="IVariable"/> that holds an <see cref="object"/> instance.
    /// </summary>
    public class ObjectVariable : Variable<object?>
    {
        /// <inheritdoc/>
        public ObjectVariable(object? value) : base(value) {}
    }
}
