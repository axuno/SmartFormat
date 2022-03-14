// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// Class for <see cref="SmartFormatter"/> settings.
    /// Properties should be considered as 'init-only' like implemented in C# 9.
    /// Any changes after passing settings as argument to CTORs may not have effect. 
    /// </summary>
    public class FormatterSettings
    {
        /// <summary>
        /// Gets or sets the <see cref="FormatterSettings.ErrorAction" /> to use for the <see cref="SmartFormatter" />.
        /// The default is <see cref="FormatErrorAction.ThrowError"/>.
        /// </summary>
        public FormatErrorAction ErrorAction { get; set; } = FormatErrorAction.ThrowError;

        /// <summary>
        /// Gets or sets the character which is used for pre-aligning or post-aligning items (e.g.: {Placeholder,10} for an alignment width of 10).
        /// Default is the space character (0x20).
        /// </summary>
        public char AlignmentFillCharacter { get; set; } = ' ';
    }
}
