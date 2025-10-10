using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Utilities;

[TestFixture]
internal class CldrPluralRulesTests
{
    private static readonly Dictionary<string, List<PluralRuleInfo>> Samples
        = CldrPluralRuleSamples.LoadRules(Path.Combine(GetCldrPluralizationAssetFilePath(), "plurals.json"));

    private static string GetCldrPluralizationAssetFilePath()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory));
        while (!sourceRoot.EndsWith($"{Path.DirectorySeparatorChar}src"))
        {
            sourceRoot = Path.GetFullPath(Path.Combine(sourceRoot, ".."));
            // If root is reached
            if (sourceRoot.EndsWith($"{Path.DirectorySeparatorChar}"))
                throw new FileNotFoundException("Could not find the 'src' folder.");
        }

        var filePath = Path.Combine(sourceRoot, "SmartFormat", "Utilities");
        return filePath;
    }

    // Test case source: just gets the locale keys
    public static IEnumerable<TestCaseData> GetLocales =>
        Samples.Keys.Select(l => new TestCaseData(l));

    [TestCaseSource(nameof(GetLocales))]
    public void Test_CldrPluralRules_Against_CldrExampleData(string locale)
    {
        var rule = CldrPluralRules.GetPluralRule(locale);

        var sample = Samples[locale];
        using (Assert.EnterMultipleScope())
        {
            foreach (var ruleInfo in sample)
            {
                foreach (var val in ruleInfo.IntegerSamples)
                {
                    _ = Enum.TryParse<PluralCategory>(ruleInfo.Category, true, out var cat);
                    var message = $"Locale '{locale}' with integer '{val}' should be '{cat}'";
                    var catFound = rule.Delegate(val);

                    switch (ruleInfo.Category)
                    {
                        case "zero":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.Zero),
                                message);
                            break;
                        case "one":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.One),
                                message);
                            break;
                        case "two":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.Two),
                                message);
                            break;
                        case "few":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.Few),
                                message);
                            break;
                        case "many":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.Many),
                                message);
                            break;
                        case "other":
                            Assert.That(catFound, Is.EqualTo(PluralCategory.Other),
                                message);
                            break;
                    }
                }

                foreach (var val in ruleInfo.DecimalSamples)
                {
                    _ = Enum.TryParse<PluralCategory>(ruleInfo.Category, true, out var cat);
                    var samplePluralIndex = rule.GetCategory(val);
                    var message = $"Locale '{locale}' with decimal '{val.ToString(CultureInfo.InvariantCulture)}' should be '{cat}'";

                    switch (ruleInfo.Category)
                    {
                        case "zero":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.Zero),
                                message);
                            break;
                        case "one":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.One),
                                message);
                            break;
                        case "two":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.Two),
                                message);
                            break;
                        case "few":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.Few),
                                message);
                            break;
                        case "many":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.Many),
                                message);
                            break;
                        case "other":
                            Assert.That(samplePluralIndex, Is.EqualTo(PluralCategory.Other),
                                message);
                            break;
                    }
                }
            }
        }
    }

    [Test]
    public void UnknownIsoCode_ShouldReturn_SingularRule()
    {
        Assert.That(() => CldrPluralRules.GetPluralRule(null).Delegate.Method.Name, Is.EqualTo("Singular"));
        Assert.That(() => CldrPluralRules.GetPluralRule("unknown").Delegate.Method.Name, Is.EqualTo("Singular"));
    }

    [Test]
    public void RestoreDefault_ShouldClear_CustomRules()
    {
        var defaultSize = CldrPluralRules.IsoCodeToRule.Count;
        CldrPluralRules.IsoCodeToRule.Add("unknown", new CldrPluralRule([], n => PluralCategory.Other));
        var sizeAfterAdding = CldrPluralRules.IsoCodeToRule.Count;
        CldrPluralRules.RestoreDefault();
        var sizeAfterRestoring = CldrPluralRules.IsoCodeToRule.Count;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sizeAfterAdding, Is.EqualTo(defaultSize + 1));
            Assert.That(sizeAfterRestoring, Is.EqualTo(defaultSize));
        }
    }

#if NET6_0_OR_GREATER
    [Test, Explicit]
    public void Create_CldrPluralRules_Using_CldrPluralRuleGenerator()
    {
        // Creates or overwrites the CldrPluralRules.cs
        // file from the plurals.json asset file.
        var assetPath = GetCldrPluralizationAssetFilePath();
        var jsonFile = Path.GetFullPath(Path.Combine(assetPath, "plurals.json"));
        var csFile = Path.GetFullPath(Path.Combine(assetPath, "CldrPluralRules.cs"));

        using (Assert.EnterMultipleScope())
        {
            var fileWriteTimeBefore = File.GetLastWriteTime(csFile);
            Assert.That(() =>
            {
                CldrPluralRuleGenerator.Generate(jsonFile, csFile);
            }, Throws.Nothing);
            var fileWriteTimeAfter = File.GetLastWriteTime(csFile);
            Assert.That(fileWriteTimeAfter, Is.GreaterThan(fileWriteTimeBefore),
                "The CldrPluralRules.cs file should be overwritten with a newer file.");
        }
    }
#endif

}
