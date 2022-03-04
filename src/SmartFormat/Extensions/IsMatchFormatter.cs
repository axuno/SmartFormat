// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Formatter with evaluation of regular expressions.
    /// The formatter can output matching group values.
    /// </summary>
    /// <example>
    /// Syntax:
    ///   {value:ismatch(regex): format | default}
    ///
    /// Or in context of a list:
    ///   {myList:list:{:ismatch(^regex$):{:format}|'no match'}|, | and }
    ///
    /// Or with output of the first matching group value:
    ///   {value:ismatch(regex):First match in '{}'\\: {m[1]}|No match}
    /// 
    /// </example>
    public class IsMatchFormatter : IFormatter, IInitializer
    {
        private char _splitChar = '|';

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"ismatch"};

        ///<inheritdoc/>
        public string Name { get; set; } = "ismatch";

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
            // Cannot deal with null
            if (formattingInfo.CurrentValue is null)
                return false;

            var expression = formattingInfo.FormatterOptions;
            var formats = formattingInfo.Format?.Split(SplitChar);

            // Check whether arguments can be handled by this formatter
            if (formats is null || formats.Count != 2)
            {
                if (formats?.Count == 0)
                    return true;

                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format options.");
            }

            var regEx = new Regex(expression, RegexOptions);
            var match = regEx.Match(formattingInfo.CurrentValue.ToString());

            if (!match.Success)
            {
                // Output the "no match" part of the format
                if (formats.Count == 2)
                    formattingInfo.FormatAsChild(formats[1], formattingInfo.CurrentValue);

                return true;
            }

            // Match successful

            // If we have child Placeholders in the format, we want to use the matches for the output
            var matchingGroupValues = (from Group grp in match.Groups select grp.Value).ToList();

            // Output the successful match part of the format
            foreach (var formatItem in formats[0].Items)
            {
                if (formatItem is Placeholder ph)
                {
                    var variable = new KeyValuePair<string, object?>(PlaceholderNameForMatches, matchingGroupValues);
                    Format(formattingInfo, ph, variable);
                    continue;
                }

                // so it must be a literal
                var literalText = (LiteralText) formatItem;
                // On Dispose() the Format goes back to the object pool
                using var childFormat = formattingInfo.Format?.Substring(literalText.StartIndex - formattingInfo.Format.StartIndex, literalText.Length);
                if (childFormat is null) continue;
                formattingInfo.FormatAsChild(childFormat, formattingInfo.CurrentValue);
            }

            return true;
        }

        private void Format(IFormattingInfo formattingInfo, Placeholder placeholder, object matchingGroupValues)
        {
            // On Dispose() the Format goes back to the object pool
            using var childFormat =
                formattingInfo.Format?.Substring(placeholder.StartIndex - formattingInfo.Format.StartIndex,
                    placeholder.Length);
            if (childFormat is null) return;

            // Is the placeholder a "magic IsMatchFormatter" one?
            if (placeholder.Selectors.Count > 0 && placeholder.Selectors[0]?.RawText == PlaceholderNameForMatches)
            {
                // The nested placeholder will output the matching group values
                formattingInfo.FormatAsChild(childFormat, matchingGroupValues);
            }
            else
            {
                formattingInfo.FormatAsChild(childFormat, formattingInfo.CurrentValue);                
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RegexOptions"/> for the <see cref="Regex"/> expression.
        /// </summary>
        public RegexOptions RegexOptions { get; set; }

        /// <summary>
        /// Gets or sets the name of the placeholder used to output RegEx matching group values.
        /// <para>
        /// Example:<br/>
        /// {value:ismatch(regex):First match in '{}'\\: {m[1]}|No match}<br/>
        /// "m" is the PlaceholderNameForMatches
        /// </para>
        /// </summary>
        public string PlaceholderNameForMatches { get; set; } = "m";
        
        ///<inheritdoc/>
        public void Initialize(SmartFormatter smartFormatter)
        {
            // The extension is needed to output the values of RegEx matching groups
            if (smartFormatter.GetSourceExtension<KeyValuePairSource>() is null)
                smartFormatter.AddExtensions(new KeyValuePairSource());
        }
    }
}
