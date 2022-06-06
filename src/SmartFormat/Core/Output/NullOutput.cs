// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.ZString;

namespace SmartFormat.Core.Output;

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
        // Nothing to do here
    }

    ///<inheritdoc/>
    public void Write(string text, IFormattingInfo? formattingInfo = null)
    {
        // Nothing to do here
    }

    ///<inheritdoc/>
    public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
    {
        // Nothing to do here
    }

    ///<inheritdoc/>
    public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
    {
        // Nothing to do here
    }

    /// <summary>
    /// Always return <see cref="string.Empty"/>.
    /// </summary>
    public override string ToString()
    {
        return string.Empty;
    }
}