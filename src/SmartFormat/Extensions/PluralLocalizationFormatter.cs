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
    public class PluralLocalizationFormatter : IFormatter
    {

        /// <summary>
        /// Initializes the plugin with rules for many common languages.
        /// If no CultureInfo is supplied to the formatter, the
        /// default language rules will be used by default.
        /// </summary>
        public PluralLocalizationFormatter(string defaultTwoLetterIsoLanguageName)
        {
            DefaultTwoLetterISOLanguageName = defaultTwoLetterIsoLanguageName;
        }

        public string DefaultTwoLetterISOLanguageName { get; set; }

        public string[] Names { get; set; } = {"plural", "p", ""};

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // Ignore formats that start with "?" (this can be used to bypass this extension)
            if (format == null || format.baseString[format.startIndex] == ':') return false;

            // Extract the plural words from the format string:
            var pluralWords = format.Split('|');
            // This extension requires at least two plural words:
            if (pluralWords.Count == 1) return false;

            decimal value;

            // We can format numbers, and IEnumerables. For IEnumerables we look at the number of items
            // in the collection: this means the user can e.g. use the same parameter for both plural and list, for example
            // 'Smart.Format("The following {0:plural:person is|people are} impressed: {0:list:{}|, |, and}", new[] { "bob", "alice" });'
            if (current is byte or short or ushort or int or uint  or long or ulong or float or double or decimal)
                value = Convert.ToDecimal(current);
            else if (current is IEnumerable<object> objects)
                value = objects.Count();
            else
                return false;


            // Get the plural rule:
            var pluralRule = GetPluralRule(formattingInfo);

            if (pluralRule == null) return false;

            var pluralCount = pluralWords.Count;
            var pluralIndex = pluralRule(value, pluralCount);

            if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
                throw new FormattingException(format, "Invalid number of plural parameters",
                    pluralWords.Last().endIndex);

            // Output the selected word (allowing for nested formats):
            var pluralForm = pluralWords[pluralIndex];
            formattingInfo.Write(pluralForm, current);
            return true;
        }

        private PluralRules.PluralRuleDelegate? GetPluralRule(IFormattingInfo formattingInfo)
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

        public CustomPluralRuleProvider(PluralRules.PluralRuleDelegate pluralRule)
        {
            _pluralRule = pluralRule;
        }

        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(CustomPluralRuleProvider) ? this : default;
        }

        public PluralRules.PluralRuleDelegate GetPluralRule()
        {
            return _pluralRule;
        }
    }
}