// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Text;
using SmartFormat.ZString;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output;

/// <summary>
/// Wraps an UTF-16 <see cref="ZStringBuilder"/> so that it can be used for output.
/// <see cref="ZStringOutput"/> is used for the default output.
/// </summary>
/// <remarks>
/// <para>
/// Note: <see cref="ZStringOutput"/> cannot be used for object pooling,
/// because it contains a <see langword="struct"/> and a stack only returns a copy of the <see langword="struct"/>.
/// </para>
/// <see cref="StringBuilder"/>, <see cref="ZStringBuilder"/>,
/// <see cref="UnicodeEncoding"/> and <see langword="string"/> objects also use <b>UTF-16</b> encoding to store characters.
/// </remarks>
public class ZStringOutput : IOutput, IDisposable
{
    /// <summary>
    /// Returns the <see cref="ZStringBuilder"/> used for output.
    /// </summary>
    public ZStringBuilder Output { get; }

    /// <summary>
    /// Creates a new instance of <see cref="ZStringOutput"/>.
    /// </summary>
    public ZStringOutput()
    {
        Output = Utilities.ZStringBuilderExtensions.CreateZStringBuilder();
    }

    /// <summary>
    /// Creates a new instance of <see cref="ZStringOutput"/> with the given initial capacity.
    /// </summary>
    /// <param name="capacity">The estimated capacity required. This will reduce or avoid incremental buffer increases.</param>
    public ZStringOutput(int capacity)
    {
        Output = Utilities.ZStringBuilderExtensions.CreateZStringBuilder(capacity);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ZStringOutput"/> using the given <see cref="ZStringBuilder"/>.
    /// </summary>
    public ZStringOutput(ZStringBuilder stringBuilder)
    {
        Output = stringBuilder;
    }

    ///<inheritdoc/>
    public void Write(string text, IFormattingInfo? formattingInfo = null)
    {
        Output.Append(text);
    }

    ///<inheritdoc/>
    public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
    {
        Output.Append(text);
    }

    ///<inheritdoc/>
    public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
    {
        Output.Append(stringBuilder);
    }

    /// <summary>
    /// Returns the string result of the <see cref="ZStringBuilder"/>.
    /// </summary>
    public override string ToString()
    {
        return Output.ToString();
    }

    /// <summary>
    /// Disposes resources of <see cref="ZStringOutput"/>.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Output.Dispose();
        }
    }

    /// <summary>
    /// Disposes resources of <see cref="ZStringOutput"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}