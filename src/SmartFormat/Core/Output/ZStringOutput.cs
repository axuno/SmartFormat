//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Text;
using SmartFormat.ZString;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a <see cref="ZStringBuilder"/> so it can be used for output.
    /// This is used for the default output.
    /// </summary>
    /// <remarks>
    /// <see cref="StringBuilder"/>, <see cref="ZStringBuilder"/>,
    /// <see cref="UnicodeEncoding"/> and <see langword="string"/> objects use <b>UTF-16</b> encoding to store characters.
    /// </remarks>
    public class ZStringOutput : IOutput, IDisposable
    {
        private ZStringBuilder _output;

        /// <summary>
        /// Returns the <see cref="ZStringBuilder"/> used for output.
        /// </summary>
        public ZStringBuilder Output => _output; // Use with a backing field!

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/>.
        /// </summary>
        public ZStringOutput()
        {
            _output = Utilities.ZStringExtensions.CreateStringBuilder();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/> with the given initial capacity.
        /// </summary>
        /// <param name="capacity">The estimated capacity required. This will reduce or avoid incremental buffer increases.</param>
        public ZStringOutput(int capacity)
        {
            _output = Utilities.ZStringExtensions.CreateStringBuilder(capacity);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/> using the given <see cref="ZStringBuilder"/>.
        /// </summary>
        public ZStringOutput(ZStringBuilder stringBuilder)
        {
            _output = stringBuilder;
        }

        ///<inheritdoc/>
        public void Write(string text, IFormattingInfo? formattingInfo = null)
        {
            _output.Append(text);
        }

        ///<inheritdoc/>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
        {
            _output.Append(text);
        }

        ///<inheritdoc/>
        public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
        {
            _output.Append(stringBuilder);
        }

        /// <summary>
        /// Returns the string result of the <see cref="ZStringBuilder"/>.
        /// </summary>
        public override string ToString()
        {
            return _output.ToString();
        }

        /// <summary>
        /// Disposes resources of <see cref="ZStringOutput"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _output.Dispose();
            }
        }

        /// <summary>
        /// Disposes resources of <see cref="ZStringOutput"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}