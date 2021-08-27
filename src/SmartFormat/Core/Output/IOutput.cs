//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Writes a string to the output.
    /// </summary>
    public interface IOutput
    {
        /// <summary>
        /// Writes a string to the output.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formattingInfo"></param>
        void Write(string text, IFormattingInfo formattingInfo);

        /// <summary>
        /// Writes a <see cref="ReadOnlySpan{T}"/> text to the output.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formattingInfo"></param>
        void Write(ReadOnlySpan<char> text, IFormattingInfo formattingInfo);
    }
}