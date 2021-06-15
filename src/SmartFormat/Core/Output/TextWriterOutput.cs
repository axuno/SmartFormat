//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.IO;
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
        /// <param name="startIndex">The start index within the text to write.</param>
        /// <param name="length">The length of text to write starting at the start index.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
        {
            Output.Write(text.Substring(startIndex, length));
        }
    }
}