//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// A class to format following culture specific pluralization rules.
    /// </summary>
    public class PluralLocalizationFormatter : IFormatter
    {
        /// <summary>
        /// Initializes the plugin with rules for many common languages.
        /// If no <see cref="CultureInfo"/> is supplied to the formatter, the
        /// default language rules will be used by default.
        /// </summary>
        /// <remarks>
        /// Language Plural Rules are described at
        /// https://unicode-org.github.io/cldr-staging/charts/37/supplemental/language_plural_rules.html
        /// </remarks>
        public PluralLocalizationFormatter(string defaultTwoLetterIsoLanguageName = "en")
        {
            DefaultTwoLetterISOLanguageName = defaultTwoLetterIsoLanguageName;
        }

        /// <summary>
        /// Gets or sets the two letter ISO language name.
        /// </summary>
        public string DefaultTwoLetterISOLanguageName { get; set; }

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"plural", "p", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "plural";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // Ignore formats that start with "?" (this can be used to bypass this extension)
            if (format == null || format.BaseString.Length > 0 && format.BaseString[format.StartIndex] == ':') return false;

            // Extract the plural words from the format string:
            var pluralWords = format.Split('|');
            // This extension requires at least two plural words:
            if (pluralWords.Count == 1)
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 plural words.");

            }

            decimal value;

            // Check whether arguments can be handled by this formatter
            
            // We can format numbers, and IEnumerables. For IEnumerables we look at the number of items
            // in the collection: this means the user can e.g. use the same parameter for both plural and list, for example
            // 'Smart.Format("The following {0:plural:person is|people are} impressed: {0:list:{}|, |, and}", new[] { "bob", "alice" });'
            if (current is byte or short or int or long or float or double or decimal or ushort or uint or ulong)
                value = Convert.ToDecimal(current);
            else if (current is IEnumerable<object> objects)
                value = objects.Count();
            else
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' can format numbers and IEnumerables, but the argument was of type '{current?.GetType().ToString() ?? "null"}'");
            }

            // Get the specific plural rule, or the default rule:
            var pluralRule = GetPluralRule(formattingInfo);

            var pluralCount = pluralWords.Count;
            var pluralIndex = pluralRule(value, pluralCount);

            if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
                throw new FormattingException(format, $"Invalid number of plural parameters in {nameof(PluralLocalizationFormatter)}",
                    pluralWords.Last().EndIndex);

            // Output the selected word (allowing for nested formats):
            var pluralForm = pluralWords[pluralIndex];
            formattingInfo.FormatAsChild(pluralForm, current);
            return true;
        }

        private PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
        {
            // See if the language was explicitly passed:
            var pluralOptions = formattingInfo.FormatterOptions;
            if (pluralOptions?.Length != 0) return PluralRules.GetPluralRule(pluralOptions);

            // See if a CustomPluralRuleProvider is available from the FormatProvider:
            var provider = formattingInfo.FormatDetails.Provider;
            var pluralRuleProvider =
                (CustomPluralRuleProvider?) provider?.GetFormat(typeof(CustomPluralRuleProvider));
            if (pluralRuleProvider != null) return pluralRuleProvider.GetPluralRule();

            // Use the CultureInfo, if provided:
            if (provider is CultureInfo cultureInfo)
            {
                var culturePluralRule = PluralRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
                return culturePluralRule;
            }
            
            // Use the default, if provided:
            return PluralRules.GetPluralRule(DefaultTwoLetterISOLanguageName);;
        }
    }

    /// <summary>
    /// Use this class to provide custom plural rules to Smart.Format
    /// </summary>
    public class CustomPluralRuleProvider : IFormatProvider
    {
        private readonly PluralRules.PluralRuleDelegate _pluralRule;

        /// <summary>
        /// Creates a new instance of a <see cref="CustomPluralRuleProvider"/>.
        /// </summary>
        /// <param name="pluralRule">The delegate for plural rules.</param>
        public CustomPluralRuleProvider(PluralRules.PluralRuleDelegate pluralRule)
        {
            _pluralRule = pluralRule;
        }

        /// <summary>
        /// Gets the format <see cref="object"/> for a <see cref="CustomPluralRuleProvider"/>.
        /// </summary>
        /// <param name="formatType"></param>
        /// <returns>The format <see cref="object"/> for a <see cref="CustomPluralRuleProvider"/> or <see langword="null"/>.</returns>
        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(CustomPluralRuleProvider) ? this : default;
        }

        /// <summary>
        /// Gets the <see cref="PluralRules.PluralRuleDelegate"/> of the current <see cref="CustomPluralRuleProvider"/> instance.
        /// </summary>
        /// <returns></returns>
        public PluralRules.PluralRuleDelegate GetPluralRule()
        {
            return _pluralRule;
        }
    }
}