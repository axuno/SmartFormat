//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Formatter to access part of a string.
    /// </summary>
    public class SubStringFormatter : IFormatter
    {
        /// <summary>
        /// The names for this Formatter.
        /// </summary>
        public string[] Names { get; set; } = {"substr"};

        /// <summary>
        /// The delimiter to separate parameters, defaults to comma.
        /// </summary>
        public char ParameterDelimiter { get; set; } = ',';

        /// <summary>
        /// Get or set the string to display for NULL values, defaults to "(null)".
        /// </summary>
        public string NullDisplayString { get; set; } = "(null)";

        /// <summary>
        /// Get or set the behavior for when start index and/or length is too great, defaults to <see cref="SubStringOutOfRangeBehavior.ReturnEmptyString"/>.
        /// </summary>
        public SubStringOutOfRangeBehavior OutOfRangeBehavior { get; set; } = SubStringOutOfRangeBehavior.ReturnEmptyString;

        /// <summary>
        /// Tries to process the given <see cref="IFormattingInfo"/>.
        /// </summary>
        /// <param name="formattingInfo">Returns true if processed, otherwise false.</param>
        /// <returns></returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (string.IsNullOrEmpty(formattingInfo.FormatterOptions)) return false;
            var parameters = formattingInfo.FormatterOptions!.Split(ParameterDelimiter);

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
                    if (startPos > currentValue.Length)
                        startPos = currentValue.Length;
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