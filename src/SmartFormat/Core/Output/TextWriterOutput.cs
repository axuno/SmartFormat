//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using SmartFormat.Core.Extensions;
using SmartFormat.ZString;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Wraps a <see cref="TextWriter"/> so that it can be used for output.
    /// </summary>
    public class TextWriterOutput : IOutput
    {
        /// <summary>
        /// Creates a new instance of <see cref="TextWriterOutput"/>.
        /// </summary>
        /// <param name="output">The <see cref="TextWriter"/> to use for output.</param>
        public TextWriterOutput(TextWriter output)
        {
            Output = output;
        }

        /// <summary>
        /// Returns the <see cref="TextWriter"/> used for output.
        /// </summary>
        public TextWriter Output { get; }

        ///<inheritdoc/>
        public void Write(string text, IFormattingInfo? formattingInfo = null)
        {
            Output.Write(text);
        }

        ///<inheritdoc/>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
        {
#if NETSTANDARD2_1
            Output.Write(text);
#else
            Output.Write(text.ToString());
#endif
        }

        ///<inheritdoc/>
        public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
        {
#if NETSTANDARD2_1
            Output.Write(stringBuilder.AsSpan());
#else
            Output.Write(stringBuilder.ToString());
#endif
        }
    }
}