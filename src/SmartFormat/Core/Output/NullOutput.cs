//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Noop implementation of <see cref="IOutput"/>
    /// </summary>
    /// <remarks>
    /// Useful for performance tests excluding the result string generation.
    /// </remarks>
    public class NullOutput : IOutput
    {
        /// <summary>
        /// Creates a new instance of <see cref="NullOutput"/>.
        /// </summary>
        public NullOutput()
        {
        }

        /// <summary>
        /// Noop for writing a string.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(string text, IFormattingInfo formattingInfo)
        {
        }

        /// <summary>
        /// Noop for writing a <see cref="ReadOnlySpan{T}"/>
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo formattingInfo)
        {
        }

        /// <summary>
        /// Always return <see cref="string.Empty"/>.
        /// </summary>
        public override string ToString()
        {
            return string.Empty;
        }
    }
}