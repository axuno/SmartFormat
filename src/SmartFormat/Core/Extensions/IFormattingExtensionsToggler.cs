// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;

namespace SmartFormat.Core.Extensions;

/// <summary>
/// Represents a toggle for enabling or disabling <see cref="IFormatter"/> extensions.
/// This interface is primarily used by <see cref="ISource"/> extensions
/// that receive it as part of the <see cref="ISelectorInfo"/> parameter.
/// </summary>
public interface IFormattingExtensionsToggle
{
    // This interface should become part of ISelectorInfo in the future.

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="IFormatter"/> extensions are enabled.
    /// The value should be <see langword="false"/> (default), unless the <see cref="ISource"/> extension
    /// found a value in <seealso cref="ISource.TryEvaluateSelector"/> where default formatting cannot reasonably be done.
    /// <br/>
    /// In this case the <see cref="ISource"/> may directly write some output using <see cref="IFormattingInfo.Write(ReadOnlySpan{char})"/>,
    /// or produce no output at all.
    /// </summary>
    public bool DisableFormattingExtensions { get; set; }
}
