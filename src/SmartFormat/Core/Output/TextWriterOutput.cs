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
        public void Write(string text)
        {
            Output.Write(text);
        }
    }
}