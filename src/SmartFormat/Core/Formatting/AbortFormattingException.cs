// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Formatting;

/// <summary>
/// An exception designed to halt further processing for a specific format item.
/// This exception can be raised from an <see cref="ISource"/> to prevent additional processing,
/// particularly in scenarios where the data's state is not yet prepared or deemed valid.
/// </summary>
[Serializable]
public class AbortFormattingException : Exception
{
    /// <summary>
    /// Creates a new instance of <see cref="AbortFormattingException"/>.
    /// </summary>
    /// <param name="text">Optional text to be used in the output string.</param>
    public AbortFormattingException(string text)
    {
        Text = text;
    }

    ///<inheritdoc/>
    public AbortFormattingException() { }

    /// <summary>
    /// A message to be used in the output string.
    /// </summary>
    public string? Text { get; }
}
