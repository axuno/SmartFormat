// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

/// <summary>
/// A class to format following culture specific pluralization rules.
/// The range of values the formatter can process is from <see cref="decimal.MinValue"/> to <see cref="decimal.MaxValue"/>.
/// </summary>
public class PluralLocalizationFormatter : IFormatter
{
    private char _splitChar = '|';

    /// <summary>
    /// CTOR for the plugin with rules for many common languages.
    /// </summary>
    /// <remarks>
    /// Language Plural Rules are described at
    /// https://unicode-org.github.io/cldr-staging/charts/37/supplemental/language_plural_rules.html
    /// </remarks>
    public PluralLocalizationFormatter()
    {
    }

    /// <summary>
    /// Initializes the plugin with rules for many common languages.
    /// </summary>
    /// <remarks>
    /// Culture is now determined in this sequence:<br/>
    /// 1. Get the culture from the <see cref="FormattingInfo.FormatterOptions"/>.<br/>
    /// 2. Get the culture from the <see cref="IFormatProvider"/> argument (which may be a <see cref="CultureInfo"/>) to <see cref="SmartFormatter.Format(IFormatProvider, string, object?[])"/><br/>
    /// 3. The <see cref="CultureInfo.CurrentUICulture"/>.<br/>
    /// Language Plural Rules are described at
    /// https://unicode-org.github.io/cldr-staging/charts/37/supplemental/language_plural_rules.html
    /// </remarks>
    [Obsolete("This constructor is not required. Changed process to determine the default culture.", true)]
    public PluralLocalizationFormatter(string defaultTwoLetterIsoLanguageName)
    {
        DefaultTwoLetterISOLanguageName = defaultTwoLetterIsoLanguageName;
    }

    /// <summary>
    /// Gets or sets the two-letter ISO language name.
    /// </summary>
    /// <remarks>
    /// Culture is now determined in this sequence:<br/>
    /// 1. Get the culture from the <see cref="FormattingInfo.FormatterOptions"/>.<br/>
    /// 2. Get the culture from the <see cref="IFormatProvider"/> argument (which may be a <see cref="CultureInfo"/>) to <see cref="SmartFormatter.Format(IFormatProvider, string, object?[])"/><br/>
    /// 3. The <see cref="CultureInfo.CurrentUICulture"/>.<br/>
    /// Language Plural Rules are described at
    /// https://unicode-org.github.io/cldr-staging/charts/37/supplemental/language_plural_rules.html
    /// </remarks>
    [Obsolete("This property is not supported any more. Changed process to get or set the default culture.", true)]
    public string DefaultTwoLetterISOLanguageName { get; set; } = "en";

    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    public string[] Names { get; set; } = {"plural", "p", string.Empty};

    ///<inheritdoc/>
    public string Name { get; set; } = "plural";

    /// <summary>
    /// Any extensions marked as <see cref="CanAutoDetect"/> will be called implicitly
    /// (when no formatter name is specified in the input format string).
    /// For example, "{0:N2}" will implicitly call extensions marked as <see cref="CanAutoDetect"/>.
    /// Implicit formatter invocations should not throw exceptions.
    /// With <see cref="CanAutoDetect"/> == <see langword="false"/>, the formatter can only be
    /// called by its name in the input format string.
    /// <para/>
    /// <b>Auto-detection only works with more than 1 format argument.
    /// It is recommended to set <see cref="CanAutoDetect"/> to <see langword="false"/>. This will be the default in a future version.
    /// </b>
    /// </summary>
    /// <remarks>
    /// If more than one registered <see cref="IFormatter"/> can auto-detect, the first one in the formatter list will win.
    /// </remarks>
    public bool CanAutoDetect { get; set; } = true;

    /// <summary>
    /// Gets or sets the character used to split the option text literals.
    /// Valid characters are: | (pipe) , (comma)  ~ (tilde)
    /// </summary>
    public char SplitChar
    {
        get => _splitChar;
        set => _splitChar = Validation.GetValidSplitCharOrThrow(value);
    }

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;
        var current = formattingInfo.CurrentValue;

        if (format == null) return false;
            
        // Extract the plural words from the format string
        var pluralWords = format.Split(SplitChar);

        var useAutoDetection = string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName);

        // This extension requires at least two plural words for auto-detection
        // Valid types for auto-detection are checked later
        if (useAutoDetection && pluralWords.Count <= 1) return false;

        decimal value;

        /*
         Check whether arguments can be handled by this formatter:

         We can format numbers, and IEnumerables. For IEnumerables we look at the number of items
         in the collection: this means the user can e.g. use the same parameter for both plural and list, for example
         'Smart.Format("The following {0:plural:person is|people are} impressed: {0:list:{}|, |, and}", new[] { "bob", "alice" });'
        */
        switch (current)
        {
            case IConvertible convertible when convertible is not (bool or string) && TryGetDecimalValue(convertible, null, out value):
                break;
            case IEnumerable<object> objects:
                value = objects.Count();
                break;
            default:
            {
                // Auto-detection calls just return a failure to evaluate
                if (useAutoDetection) return false;

                // throw, if the formatter has been called explicitly
                throw new FormattingException(format,
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' can format numbers and IEnumerables, but the argument was of type '{current?.GetType().ToString() ?? "null"}'", 0);
            }
        }

        // Get the specific plural rule, or the default rule:
        var pluralRule = GetPluralRule(formattingInfo);

        var pluralCount = pluralWords.Count;
        var pluralIndex = pluralRule(value, pluralCount);

        if (pluralIndex < 0 || pluralWords.Count <= pluralIndex)
            throw new FormattingException(format, $"Invalid number of plural parameters in {nameof(PluralLocalizationFormatter)}",
                pluralWords.Count - 1);

        // Output the selected word (allowing for nested formats):
        var pluralForm = pluralWords[pluralIndex];
        formattingInfo.FormatAsChild(pluralForm, current);
        return true;
    }

    private static bool TryGetDecimalValue(IConvertible convertible, IFormatProvider? provider,  out decimal value)
    {
        try
        {
            value = convertible.ToDecimal(provider);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    private static PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
    {
        // Determine the culture
        var culture = GetCultureInfo(formattingInfo);
        var pluralOptions = formattingInfo.FormatterOptions.Trim();
        if (pluralOptions.Length != 0) return PluralRules.GetPluralRule(culture.TwoLetterISOLanguageName);

        // See if a CustomPluralRuleProvider is available from the FormatProvider:
        var provider = formattingInfo.FormatDetails.Provider;
        var pluralRuleProvider =
            (CustomPluralRuleProvider?) provider?.GetFormat(typeof(CustomPluralRuleProvider));
        if (pluralRuleProvider != null) return pluralRuleProvider.GetPluralRule();

        // No CustomPluralRuleProvider, so use the CultureInfo

        return PluralRules.GetPluralRule(culture.TwoLetterISOLanguageName);
    }

    private static CultureInfo GetCultureInfo(IFormattingInfo formattingInfo)
    {
        var culture = formattingInfo.FormatterOptions.Trim();
        CultureInfo cultureInfo;
        if (culture == string.Empty)
        {
            if (formattingInfo.FormatDetails.Provider is CultureInfo ci)
                cultureInfo = ci;
            else
                cultureInfo = CultureInfo.CurrentUICulture;

            // There is no pluralization rule for invariant culture (TwoLetterISOLanguageName == "iv"),
            // so we take English as default
            if(cultureInfo.Equals(CultureInfo.InvariantCulture))
                cultureInfo = CultureInfo.GetCultureInfo("en");
        }
        else
        {
            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(culture);
            }
            catch (Exception e)
            {
                throw new FormattingException(formattingInfo.Format, e, 0);
            }
        }

        return cultureInfo;
    }
}

