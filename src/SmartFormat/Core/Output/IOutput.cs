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
    /// <remarks>
    /// <b>Only implement a call</b> to <see cref="Write(ReadOnlySpan{char}, IFormattingInfo?)"/>
    /// using <see cref="MemoryExtensions.AsSpan(string)"/> for '<see paramref="text"/>'.
    /// </remarks>
    /// <param name="text"></param>
    /// <param name="formattingInfo"></param>
    [Obsolete("Use Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null) instead.", false)]
    void Write(string text, IFormattingInfo? formattingInfo = null);

    /// <summary>
    /// Writes a <see cref="ReadOnlySpan{T}"/> text to the output.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="formattingInfo"></param>
    // v4: Remove formattingInfo argument? Or make it non-optional and non-nullable?
    void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null);

    /// <summary>
    /// Writes text of a <see cref="ZStringBuilder"/> to the output.
    /// </summary>
    /// <remarks>
    /// <b>Only implement a call</b> to <see cref="Write(ReadOnlySpan{char}, IFormattingInfo?)"/>
    /// using <see cref="ZStringBuilder.AsSpan()"/> for '<see paramref="stringBuilder"/>'.
    /// </remarks>
    /// <param name="stringBuilder"></param>
    /// <param name="formattingInfo"></param>
    [Obsolete("Use Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null) instead.", false)]
    void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null);
}
