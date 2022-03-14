// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// The class formats <see langword="null"/> values.
    /// </summary>
    /// <example>
    /// Smart.Format("{0:isnull:It's null}", arg)
    /// Smart.Format("{0:isnull:It's null|Not null}", arg)
    /// Smart.Format("{0:isnull:It's null|{}}", arg)
    /// </example>
    public class NullFormatter : IFormatter
    {
        private char _splitChar = '|';

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"isnull"};

        ///<inheritdoc/>
        public string Name { get; set; } = "isnull";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = false;

        /// <summary>
        /// Gets or sets the character used to split the option text literals.
        /// Valid characters are: | (pipe) , (comma)  ~ (tilde)
        /// </summary>
        public char SplitChar
        {
            get => _splitChar;
            set => _splitChar = Utilities.Validation.GetValidSplitCharOrThrow(value);
        }

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var currentValue = formattingInfo.CurrentValue;
            var chooseOptions = formattingInfo.FormatterOptions.AsSpan().Trim();
            var formats = formattingInfo.Format?.Split(SplitChar);
            
            // Check whether arguments can be handled by this formatter
            if (chooseOptions.Length != 0)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' does not allow choose options");
            }
            
            if (formats is null || formats.Count is < 1 or > 2)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' must have 1 or 2 format options");
            }

            // Use the format for null
            if (currentValue is null)
            {
                formattingInfo.FormatAsChild(formats[0], currentValue);
                return true;
            }

            // Use the format for a value other than null
            if (formats.Count > 1)
            {
                formattingInfo.FormatAsChild(formats[1], currentValue);
                return true;
            }

            // There is no format for a value other than null
            formattingInfo.Write(ReadOnlySpan<char>.Empty);

            return true;
        }
    }
}
