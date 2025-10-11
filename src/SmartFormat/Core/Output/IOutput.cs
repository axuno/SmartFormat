// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.ZString;

namespace SmartFormat.Core.Output;

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
    // vNext: Remove formattingInfo argument?
    [Obsolete("Use Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null) instead.", false)]
    void Write(string text, IFormattingInfo? formattingInfo = null);

    /// <summary>
    /// Writes a <see cref="ReadOnlySpan{T}"/> text to the output.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="formattingInfo"></param>
    // vNext: Remove formattingInfo argument?
    void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null);

    /// <summary>
    /// Writes text of a <see cref="ZStringBuilder"/> to the output.
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="formattingInfo"></param>
    // vNext: Remove formattingInfo argument?
    [Obsolete("Use Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null) instead.", false)]
    void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null);
}
