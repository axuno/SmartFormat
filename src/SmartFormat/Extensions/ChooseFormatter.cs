// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// A class to output literals depending on the value of the input variable.
    /// </summary>
    public class ChooseFormatter : IFormatter
    {
        /// <summary>
        /// Gets or sets the character used to split the option text literals.
        /// </summary>
        public char SplitChar { get; set; } = '|';
        
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"choose", "c"};

        ///<inheritdoc/>
        public string Name { get; set; } = "choose";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = false;

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var chooseOptions = formattingInfo.FormatterOptions.Split(SplitChar);
            var formats = formattingInfo.Format?.Split(SplitChar);
            
            // Check whether arguments can be handled by this formatter
            if (formats is null || formats.Count < 2 || chooseOptions is null)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format options.");
            }

            var chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);

            formattingInfo.FormatAsChild(chosenFormat, formattingInfo.CurrentValue);

            return true;
        }

        private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats,
            string[] chooseOptions)
        {
            var currentValue = formattingInfo.CurrentValue;
            var currentValueString = currentValue == null ? "null" : currentValue.ToString();

            var chosenIndex = Array.IndexOf(chooseOptions, currentValueString);

            // Validate the number of formats:
            if (choiceFormats.Count < chooseOptions.Length)
                throw formattingInfo.FormattingException("You must specify at least " + chooseOptions.Length +
                                                         " choices");
            if (choiceFormats.Count > chooseOptions.Length + 1)
                throw formattingInfo.FormattingException("You cannot specify more than " + (chooseOptions.Length + 1) +
                                                         " choices");
            if (chosenIndex == -1 && choiceFormats.Count == chooseOptions.Length)
                throw formattingInfo.FormattingException("\"" + currentValueString +
                                                         "\" is not a valid choice, and a \"default\" choice was not supplied");

            if (chosenIndex == -1) chosenIndex = choiceFormats.Count - 1;

            var chosenFormat = choiceFormats[chosenIndex];
            return chosenFormat;
        }
    }
}
