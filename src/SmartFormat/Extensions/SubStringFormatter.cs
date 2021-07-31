//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Formatter to access part of a string.
    /// </summary>
    public class SubStringFormatter : IFormatter
    {
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"substr"};

        ///<inheritdoc/>
        public string Name { get; set; } = "substr";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = false;

        /// <summary>
        /// The delimiter to separate parameters, defaults to comma.
        /// </summary>
        public char ParameterDelimiter { get; set; } = ',';

        /// <summary>
        /// Get or set the string to display for NULL values, defaults to <see cref="string.Empty"/>.
        /// </summary>
        public string NullDisplayString { get; set; } = string.Empty;

        /// <summary>
        /// Get or set the behavior for when start index and/or length is too great, defaults to <see cref="SubStringOutOfRangeBehavior.ReturnEmptyString"/>.
        /// </summary>
        public SubStringOutOfRangeBehavior OutOfRangeBehavior { get; set; } = SubStringOutOfRangeBehavior.ReturnEmptyString;

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var parameters = formattingInfo.FormatterOptions?.Split(ParameterDelimiter) ?? Array.Empty<string>();
            if (parameters.Length == 1 && parameters[0].Length == 0)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 1 formatter option.");
            }

            var currentValue = formattingInfo.CurrentValue?.ToString();
            if (currentValue == null)
            {
                formattingInfo.Write(NullDisplayString);
                return true;
            }

            var startPos = int.Parse(parameters[0]);
            var length = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
            if (startPos < 0)
                startPos = currentValue.Length + startPos;
            if (startPos > currentValue.Length)
                startPos = currentValue.Length;
            if (length < 0)
                length = currentValue.Length - startPos + length;

            switch(OutOfRangeBehavior)
            {
                case SubStringOutOfRangeBehavior.ReturnEmptyString:
                    if (startPos + length > currentValue.Length)
                        length = 0;
                    break;
                case SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString:
                    if (startPos + length > currentValue.Length)
                        length = (currentValue.Length - startPos);
                    break;
            }

            var substring = parameters.Length > 1
                ? currentValue.Substring(startPos, length)
                : currentValue.Substring(startPos);

            formattingInfo.Write(substring);

            return true;
        }

        /// <summary>
        /// Specify behavior when start index and/or length is out of range
        /// </summary>
        public enum SubStringOutOfRangeBehavior
        {
            /// <summary>
            /// Returns string.Empty
            /// </summary>
            ReturnEmptyString,
            /// <summary>
            /// Returns the remainder of the string, starting at StartIndex
            /// </summary>
            ReturnStartIndexToEndOfString,
            /// <summary>
            /// Throws <see cref="SmartFormat.Core.Formatting.FormattingException"/> 
            /// </summary>
            ThrowException
        }
    }
}