//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// The class formats <see langword="null"/> values.
    /// </summary>
    public class NullFormatter : IFormatter
    {
        /// <inheritdoc />
        public string[] Names { get; set; } = {"isnull"};

        /// <inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            switch (current)
            {
                case null when format is not null:
                    formattingInfo.Write(format.GetLiteralText());
                    return true;
                default:
                    formattingInfo.Write(string.Empty);
                    return true;
            }
        }
    }
}
