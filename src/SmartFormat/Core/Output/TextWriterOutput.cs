//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Cysharp.Text;
using SmartFormat.Core.Extensions;

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

        /// <summary>
        /// Writes text to the <see cref="TextWriter"/> object.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, IFormattingInfo formattingInfo)
        {
            Output.Write(text);
        }

        /// <summary>
        /// Writes text to the <see cref="TextWriter"/> object.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo formattingInfo)
        {
#if NETSTANDARD2_1
            Output.Write(text);
#else
            Output.Write(text.ToString());
#endif
        }

        
        public void Write(Utf16ValueStringBuilder stringBuilder, IFormattingInfo formattingInfo)
        {
            throw new NotImplementedException();
        }
    }
}