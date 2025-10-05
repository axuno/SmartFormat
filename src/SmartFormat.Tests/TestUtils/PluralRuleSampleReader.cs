//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartFormat.Tests.TestUtils;

internal struct PluralRuleInfo
{
    public PluralRuleInfo()
    {
        Locale = string.Empty;
        Category = string.Empty;
        RawRule = string.Empty;
        IntegerSamples = [];
        DecimalSamples = [];
    }
    public string Locale { get; set; }
    public string Category { get; set; }
    public string RawRule { get; set; }
    public List<int> IntegerSamples { get; set; }
    public List<decimal> DecimalSamples { get; set; }
}

internal static class CldrPluralRuleSamples
{
    public static Dictionary<string, List<PluralRuleInfo>> LoadRules(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var doc = JsonDocument.Parse(json);
        var rules = doc.RootElement.GetProperty("supplemental").GetProperty("plurals-type-cardinal");

        var result = new Dictionary<string, List<PluralRuleInfo>>();

        foreach (var locale in rules.EnumerateObject())
        {
            var localeCode = locale.Name;
            var ruleList = new List<PluralRuleInfo>();

            foreach (var category in locale.Value.EnumerateObject())
            {
                var rawKey = category.Name; // e.g. "pluralRule-count-one"
                var pluralCategory = rawKey.Replace("pluralRule-count-", "");
                var rawRule = category.Value.GetString() ?? "";

                var info = new PluralRuleInfo
                {
                    Locale = localeCode,
                    Category = pluralCategory,
                    RawRule = StripSamples(rawRule),
                    IntegerSamples = ExtractSamples(rawRule, "integer").Select(x => (int) x).ToList(),
                    DecimalSamples = ExtractSamples(rawRule, "decimal")
                };

                ruleList.Add(info);
            }

            result[localeCode] = ruleList;
        }

        return result;
    }

    private static string StripSamples(string rule)
    {
        return Regex.Replace(rule, @"@(?:integer|decimal)[^@]*", "").Trim();
    }

    internal static List<decimal> ExtractSamples(string rule, string type)
    {
        var samples = new List<decimal>();
        var matches = Regex.Matches(rule, @$"@{type}\s+([^\@]+)");

        foreach (Match match in matches)
        {
            var parts = match.Groups[1].Value.Split(',');

            foreach (var part in parts)
            {
                var trimmed = part.Trim();

                if (trimmed.Contains('~')) // Range
                {
                    var bounds = trimmed.Split('~');
                    if (decimal.TryParse(bounds[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var start) &&
                        decimal.TryParse(bounds[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var end))
                    {
                        // Generate samples within the range
                        var step = type == "decimal" ? 0.1m : 1m;
                        for (var val = start; val <= end; val += step)
                            samples.Add(decimal.Round(val, 2));
                    }
                }
                else if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    samples.Add(value); // Single value
                }
            }
        }

        return samples;
    }
}

