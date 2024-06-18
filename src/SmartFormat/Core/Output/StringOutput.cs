// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.ZString;

namespace SmartFormat.Core.Output;

/// <summary>
/// Wraps a <see cref="StringBuilder"/> so it can be used for output.
/// </summary>
/// <remarks>
/// <see cref="StringBuilder"/>, <see cref="UnicodeEncoding"/>
/// and <see langword="string"/> objects use <b>UTF-16</b> encoding to store characters.
/// </remarks>
public class StringOutput : IOutput
{
    /// <summary>
    /// Returns the <see cref="StringBuilder"/> used for output.
    /// </summary>
    internal StringBuilder Output { get; }

    /// <summary>
    /// Creates a new instance of <see cref="StringOutput"/>.
    /// </summary>
    public StringOutput()
    {
        Output = new StringBuilder();
    }

    /// <summary>
    /// Creates a new instance of <see cref="StringOutput"/> with the given capacity.
    /// </summary>
    /// <param name="capacity">The estimated capacity for the result string. Essential for performance and GC pressure.</param>
    public StringOutput(int capacity)
    {
        Output = new StringBuilder(capacity);
    }

    /// <summary>
    /// Creates a new instance of <see cref="StringOutput"/> using the given <see cref="StringBuilder"/>.
    /// </summary>
    public StringOutput(StringBuilder output)
    {
        Output = output;
    }

    /// <summary>
    /// Writes text to the <see cref="StringBuilder"/> object.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
    public void Write(string text, IFormattingInfo? formattingInfo = null)
    {
        Output.Append(text);
    }

    /// <summary>
    /// Writes text to the <see cref="StringBuilder"/> object.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="formattingInfo">This parameter from <see cref="IOutput"/> will not be used here.</param>
    public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
    {
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
        Output.Append(text);
#else
        Output.Append(text.ToString());
#endif
    }

    ///<inheritdoc/>
    public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
    {
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
        Output.Append(stringBuilder.AsSpan());
#else
        Output.Append(stringBuilder);
#endif
    }

    /// <summary>
    /// Clears the <see cref="StringBuilder"/> used to create the output.
    ///  <para>This method gets called by <see cref="StringOutputPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.</para>
    /// </summary>
    public void Clear()
    {
        Output.Clear();
    }

    /// <summary>
    /// Returns the results of the <see cref="StringBuilder"/>.
    /// </summary>
    public override string ToString()
    {
        return Output.ToString();
    }
}
