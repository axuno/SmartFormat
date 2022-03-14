// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Extensions
{
    /// <summary> Converts an object to a string. </summary>
    public interface IFormatter
    {
        /// <summary>
        /// An extension can be explicitly called by using its name.
        /// For example, "{0:list:N2}" will explicitly call the "list" extension.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Any extensions marked as <see cref="CanAutoDetect"/> will be called implicitly
        /// (when no formatter name is specified in the input format string).
        /// For example, "{0:N2}" will implicitly call extensions marked as <see cref="CanAutoDetect"/>.
        /// Implicit formatter invocations should not throw exceptions.
        /// With <see cref="CanAutoDetect"/> == <see langword="false"/>, the formatter can only be
        /// called by its name in the input format string.
        /// </summary>
        /// <remarks>
        /// If more than one registered <see cref="IFormatter"/> can auto-detect, the first one in the formatter list will win.
        /// </remarks>
        bool CanAutoDetect { get; set; }

        /// <summary>
        /// Writes the current value to the output, using the specified format.
        /// IF this extension cannot write the value, returns false, otherwise true.
        /// </summary>
        bool TryEvaluateFormat(IFormattingInfo formattingInfo);
    }
}
