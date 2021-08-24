//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
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
        private Utf16ValueStringBuilder _output;

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/>.
        /// </summary>
        public ZStringOutput()
        {
            _output = ZString.CreateStringBuilder();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ZStringOutput"/> using the given <see cref="Utf16ValueStringBuilder"/>.
        /// </summary>
        public ZStringOutput(Utf16ValueStringBuilder output)
        {
            _output = output;
        }

        /// <summary>
        /// Writes text to the <see cref="Utf16ValueStringBuilder"/> object.
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            _output.Append(text);
        }

        /// <summary>
        /// Returns the string result of the <see cref="Utf16ValueStringBuilder"/>.
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