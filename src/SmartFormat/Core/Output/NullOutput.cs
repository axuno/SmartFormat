//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using Cysharp.Text;
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

        ///<inheritdoc/>
        public void Write(string text, IFormattingInfo formattingInfo)
        {
        }

        ///<inheritdoc/>
        public void Write(ReadOnlySpan<char> text, IFormattingInfo formattingInfo)
        {
        }

        ///<inheritdoc/>
        public void Write(Utf16ValueStringBuilder stringBuilder, IFormattingInfo formattingInfo)
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