//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;
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
