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
            this._output = output;
        }

        /// <summary>
        /// Writes text to the <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, IFormattingInfo formattingInfo)
        {
            _output.Append(text);
        }

        /// <summary>
        /// Writes text to the <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="startIndex">The start index within the text to write.</param>
        /// <param name="length">The length of text to write starting at the start index.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
        {
            _output.Append(text, startIndex, length);
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