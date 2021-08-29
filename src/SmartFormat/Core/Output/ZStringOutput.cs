//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Text;
using Cysharp.Text;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a StringBuilder so it can be used for output.
    /// This is used for the default output.
    /// </summary>
    public class ZStringOutput : IOutput, IDisposable
    {
        /// <summary>
        /// Returns the <see cref="Utf16ValueStringBuilder"/> used for output.
        /// </summary>
        /// <remarks>Must NOT be a property.</remarks>
        public Utf16ValueStringBuilder Output;

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/>.
        /// </summary>
        public ZStringOutput()
        {
            Output = ZString.CreateStringBuilder();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/> with the given initial capacity.
        /// </summary>
        /// <param name="capacity">The estimated capacity required. This will reduce or avoid incremental buffer increases.</param>
        public ZStringOutput(int capacity)
        {
            Output = Utilities.ZStringExtensions.CreateStringBuilder(capacity);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/> using the given <see cref="Utf16ValueStringBuilder"/>.
        /// </summary>
        public ZStringOutput(Utf16ValueStringBuilder stringBuilder)
        {
            Output = stringBuilder;
        }

        ///<inheritdoc/>
        public void Write(string text, IFormattingInfo? formattingInfo = null)
        {
            Output.Append(text);
        }

        ///<inheritdoc/>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
        {
            Output.Append(text);
        }

        ///<inheritdoc/>
        public void Write(Utf16ValueStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
        {
            Output.Append(stringBuilder);
        }

        /// <summary>
        /// Returns the string result of the <see cref="Utf16ValueStringBuilder"/>.
        /// </summary>
        public override string ToString()
        {
            return Output.ToString();
        }

        /// <summary>
        /// Disposes resources of <see cref="ZStringOutput"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Output.Dispose();
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