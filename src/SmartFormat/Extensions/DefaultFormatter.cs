//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Do the default formatting, same logic as "String.Format".
    /// </summary>
    public class DefaultFormatter : IFormatter
    {
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"default", "d", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "d";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;
        
        /// <summary>
        /// Checks, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/>.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>Returns true, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/></returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;
            
            // If the format has nested placeholders, we process those first
            // instead of formatting the item.
            if (format is {HasNested: true})
            {
                formattingInfo.FormatAsChild(format, current);
                return true;
            }

            // Use the provider to see if a CustomFormatter is available:
            var provider = formattingInfo.FormatDetails.Provider;

            //  (The following code was adapted from the built-in String.Format code)

            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
            string? result; 
            if (provider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter cFormatter)
            {
                var formatText = format?.GetLiteralText();
                result = cFormatter.Format(formatText, current, provider);
            }
            // IFormattable:
            else if (current is IFormattable formattable)
            {
                var formatText = format?.ToString();
                result = formattable.ToString(formatText, provider);
            }
            // ToString:
            else
            {
                result = current?.ToString();
            }

            // Output the result:
            formattingInfo.Write(result ?? string.Empty);

            return true;
        }
    }
}