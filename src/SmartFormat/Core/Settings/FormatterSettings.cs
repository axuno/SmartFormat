//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// Class for <see cref="SmartFormatter"/> settings.
    /// </summary>
    public class FormatterSettings
    {
        /// <summary>
        /// Gets or sets the <see cref="FormatterSettings.ErrorAction" /> to use for the <see cref="SmartFormatter" />.
        /// The default is <see cref="FormatErrorAction.ThrowError"/>.
        /// </summary>
        public FormatErrorAction ErrorAction { get; set; } = FormatErrorAction.ThrowError;
    }
}
