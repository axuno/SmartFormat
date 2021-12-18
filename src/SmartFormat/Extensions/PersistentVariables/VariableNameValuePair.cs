// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.

namespace SmartFormat.Extensions.PersistentVariables
{
    internal class NameVariablePair
    {
        public NameVariablePair(string name, IVariable variable)
        {
            Name = name;
            Variable = variable;
        }

        public string Name { get; }

        public IVariable Variable { get; }

        public override string ToString() => $"'{Name}' - '{Variable.GetValue()?.GetType()}' - '{Variable.GetValue()}'";
    }
}