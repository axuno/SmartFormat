//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.Text;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a StringBuilder so it can be used for output.
    /// This is used for the default output.
    /// </summary>
    public class StringOutput : IOutput
    {
        private readonly StringBuilder _output;

        /// <summary>
        /// Creates a new instance of <see cref="StringOutput"/>.
        /// </summary>
        public StringOutput()
        {
            _output = new StringBuilder();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringOutput"/> with the given capacity.
        /// </summary>
        public StringOutput(int capacity)
        {
            _output = new StringBuilder(capacity);
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringOutput"/> using the given <see cref="StringBuilder"/>.
        /// </summary>
        public StringOutput(StringBuilder output)
        {
            _output = output;
        }

        /// <summary>
        /// Writes text to the <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            _output.Append(text);
        }

        /// <summary>
        /// Returns the results of the <see cref="StringBuilder"/>.
        /// </summary>
        public override string ToString()
        {
            return _output.ToString();
        }
    }
}