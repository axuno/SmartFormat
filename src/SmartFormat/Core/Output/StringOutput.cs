//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.ZString;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a <see cref="StringBuilder"/> so it can be used for output.
    /// </summary>
    /// <remarks>
    /// <see cref="StringBuilder"/>, <see cref="UnicodeEncoding"/>
    /// and <see langword="string"/> objects use <b>UTF-16</b> encoding to store characters.
    /// </remarks>
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
        /// <param name="capacity">The estimated capacity for the result string. Essential for performance and GC pressure.</param>
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
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, IFormattingInfo? formattingInfo = null)
        {
            _output.Append(text);
        }

        /// <summary>
        /// Writes text to the <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
        {
#if NETSTANDARD2_1
            _output.Append(text);
#else
            _output.Append(text.ToString());
#endif
        }

        ///<inheritdoc/>
        public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
        {
#if NETSTANDARD2_1
            _output.Append(stringBuilder.AsSpan());
#else
            _output.Append(stringBuilder.ToString());
#endif
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