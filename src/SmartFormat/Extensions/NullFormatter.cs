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
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"isnull"};

        ///<inheritdoc/>
        public string Name { get; set; } = "isnull";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = false;

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
