//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

namespace SmartFormat.Core.Extensions
{
    /// <summary> Converts an object to a string. </summary>
    public interface IFormatter
    {
        /// <summary>
        /// An extension can be explicitly called by using any of its names.
        /// Any extensions with "" names will be called implicitly (when no named formatter is specified).
        /// For example, "{0:default:N2}" or "{0:d:N2}" will explicitly call the "default" extension.
        /// "{0:N2}" will implicitly call the "default" extension (and other extensions, too).
        /// </summary>
        string[] Names { get; set; }

        /// <summary>
        /// Writes the current value to the output, using the specified format.
        /// IF this extension cannot write the value, returns false, otherwise true.
        /// </summary>
        bool TryEvaluateFormat(IFormattingInfo formattingInfo);
    }
}