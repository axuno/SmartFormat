﻿using System;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
	public class PluralLocalizationFormatter : IFormatter
	{
		private string[] names = { "plural", "p", "" };
		public string[] Names { get { return names; } set { names = value; } }

		/// <summary>
		/// Initializes the plugin with rules for many common languages.
		/// If no CultureInfo is supplied to the formatter, the
		/// default language rules will be used by default.
		/// </summary>
		public PluralLocalizationFormatter(string defaultTwoLetterISOLanguageName)
		{
			this.DefaultTwoLetterISOLanguageName = defaultTwoLetterISOLanguageName;
		}

		private PluralRules.PluralRuleDelegate defaultPluralRule;
		private string defaultTwoLetterISOLanguageName;
		public string DefaultTwoLetterISOLanguageName
		{
			get
			{
				return this.defaultTwoLetterISOLanguageName;
			}
			set
			{
				this.defaultTwoLetterISOLanguageName = value;
				this.defaultPluralRule = PluralRules.GetPluralRule(value);
			}
		}

		private PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
		{
			// See if the language was explicitly passed:
			var pluralOptions = formattingInfo.FormatterOptions;
			if (pluralOptions.Length != 0)
			{
				return PluralRules.GetPluralRule(pluralOptions);
			}

			// See if a CustomPluralRuleProvider is available from the FormatProvider:
			var provider = formattingInfo.FormatDetails.Provider;
			if (provider != null)
			{
				var pluralRuleProvider = (CustomPluralRuleProvider) provider.GetFormat(typeof (CustomPluralRuleProvider));
				if (pluralRuleProvider != null)
				{
					return pluralRuleProvider.GetPluralRule();
				}
			}

			// Use the CultureInfo, if provided:
			var cultureInfo = provider as CultureInfo;
			if (cultureInfo != null)
			{
				var culturePluralRule = PluralRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
				return culturePluralRule;
			}


			// Use the default, if provided:
			if (this.defaultPluralRule != null)
			{
				return this.defaultPluralRule;
			}

			return null;
		}

		public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			var format = formattingInfo.Format;
			var current = formattingInfo.CurrentValue;

			// Ignore formats that start with "?" (this can be used to bypass this extension)
			if (format == null || format.baseString[format.startIndex] == ':')
			{
				return false;
			}

			// Extract the plural words from the format string:
			var pluralWords = format.Split('|');
			// This extension requires at least two plural words:
			if (pluralWords.Count == 1) return false;

            // In order to support dictionary<string,string>()
            long parsedString;
            if ((current is string) && (Int64.TryParse(Convert.ToString(current), out parsedString)))
                current = parsedString;

			// See if the value is a number:
			var currentIsNumber =
				current is byte || current is short || current is int || current is long
				|| current is float || current is double || current is decimal;
			// This extension only formats numbers:
			if (!currentIsNumber) return false;

			// Normalize the number to decimal:
			var value = Convert.ToDecimal(current);

			// Get the plural rule:
			var pluralRule = GetPluralRule(formattingInfo);

			if (pluralRule == null)
			{
				// Not a supported language.
				return false;
			}

			var pluralCount = pluralWords.Count;
			var pluralIndex = pluralRule(value, pluralCount);

			if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
			{
				// The plural rule should always return a value in-range!
				throw new FormattingException(format, "Invalid number of plural parameters", pluralWords.Last().endIndex);
			}

			// Output the selected word (allowing for nested formats):
			var pluralForm = pluralWords[pluralIndex];
			formattingInfo.Write(pluralForm, current);
			return true;
		}

	}

	/// <summary>
	/// Use this class to provide custom plural rules to Smart.Format
	/// </summary>
	public class CustomPluralRuleProvider : IFormatProvider
	{
		public object GetFormat(Type formatType)
		{
			return (formatType == typeof(CustomPluralRuleProvider)) ? this : null;
		}

		private readonly PluralRules.PluralRuleDelegate pluralRule;
		public CustomPluralRuleProvider(PluralRules.PluralRuleDelegate pluralRule)
		{
			this.pluralRule = pluralRule;
		}

		public PluralRules.PluralRuleDelegate GetPluralRule()
		{
			return pluralRule;
		}
	}

}
