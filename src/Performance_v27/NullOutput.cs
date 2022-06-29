//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Extensions;
#nullable enable

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Noop <see cref="IOutput"/>
    /// </summary>
    public class NullOutput : IOutput
    {

        /// <summary>
        /// Creates a new instance of <see cref="NullOutput"/>.
        /// </summary>
        public NullOutput()
        {
            
        }

        /// <summary>
        /// Noop writing text to the <see cref="NullOutput"/> object.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, IFormattingInfo? formattingInfo)
        {
        }

        /// <summary>
        /// Noop writing text to the <see cref="NullOutput"/> object.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="startIndex">The start index within the text to write.</param>
        /// <param name="length">The length of text to write starting at the start index.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
        {
        }

        /// <summary>
        /// Always returns <see cref="string.Empty"/>.
        /// </summary>
        public override string ToString()
        {
            return string.Empty;
        }
    }
}