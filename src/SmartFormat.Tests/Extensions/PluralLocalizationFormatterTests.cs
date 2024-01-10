using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;
using ExpectedResults = System.Collections.Generic.Dictionary<decimal, string>;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class PluralLocalizationFormatterTests
{
    private static SmartFormatter GetFormatter(SmartSettings? smartSettings = null)
    {
        var smart = Smart.CreateDefaultSmartFormat(smartSettings ?? new SmartSettings());
        smart.InsertExtension(1, new PluralLocalizationFormatter());
        return smart;
    }

    private static void TestAllResults(CultureInfo cultureInfo, string format, ExpectedResults expectedValuesAndResults)
    {
        foreach (var testResult in expectedValuesAndResults)
        {
            var smart = GetFormatter();
            var count = testResult.Key;
            var expected = testResult.Value;
            var actual = smart.Format(cultureInfo, format, count);

            Assert.That(actual, Is.EqualTo(expected));
            Debug.WriteLine(actual);
        }
    }

    [Test]
    public void Explicit_Formatter_With_Not_Enough_Parameters_Should_Throw()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(() => smart.Format("{0:plural:One}", 1), Throws.Exception.TypeOf<FormattingException>());
    }

    [Test]
    public void Explicit_Formatter_Without_IEnumerable_Arg_Should_Throw()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(() => smart.Format("{0:plural:One|Two}", new object()), Throws.Exception.TypeOf<FormattingException>());
    }

    [TestCase("")] // no string
    [TestCase("1234")] // don't convert numeric string to decimal, see https://github.com/axuno/SmartFormat/issues/345
    [TestCase(false)] // no boolean
    [TestCase(3.40282347E+38f)] // float.MaxValue exceeds decimal.MaxValue
    public void Explicit_Formatter_Without_Valid_Argument_Should_Throw(object arg)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(() => smart.Format("{0:plural:One|Two}", arg), Throws.Exception.TypeOf<FormattingException>(), "Invalid argument type or value");
    }

    [TestCase("String", "String")]
    [TestCase(false, "other")]
    [TestCase(default(string?), "other")]
    public void AutoDetect_Formatter_Should_Not_Handle_bool_string_null(object? arg, string expected)
    {
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource())
            .AddExtensions(new PluralLocalizationFormatter { CanAutoDetect = true },
                new ConditionalFormatter { CanAutoDetect = true },
                new DefaultFormatter());

        // Result comes from ConditionalFormatter!
        var result = smart.Format("{0:{}|other}", arg);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(100)]
    public void Explicit_Should_Process_Singular_PluralRule(int count)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        // Japanese does not have plural definitions (is a Singular language)
        // "リンゴを2個持っています。" => "I have 2 apple(s)"
        var result = smart.Format("リンゴを{0:plural(ja):{}個持っています。}", count);
        Assert.That(result, Is.EqualTo($"リンゴを{count}個持っています。"));
    }

    [Test]
    public void Test_Default()
    {
        TestAllResults(
            new CultureInfo("en-US"),
            "There {0:plural:is|are} {0} {0:plural:item|items} remaining",
            new ExpectedResults {
                {  -1, "There are -1 items remaining"},
                {   0, "There are 0 items remaining"},
                {0.5m, "There are 0.5 items remaining"},
                {   1, "There is 1 item remaining"},
                {1.5m, "There are 1.5 items remaining"},
                {   2, "There are 2 items remaining"},
                {  11, "There are 11 items remaining"},
            });
    }

    [Test]
    public void Test_English()
    {
        TestAllResults(
            new CultureInfo("en-US"),
            "There {0:plural:is|are} {0} {0:plural:item|items} remaining",
            new ExpectedResults {
                {  -1, "There are -1 items remaining"},
                {   0, "There are 0 items remaining"},
                {0.5m, "There are 0.5 items remaining"},
                {   1, "There is 1 item remaining"},
                {1.5m, "There are 1.5 items remaining"},
                {   2, "There are 2 items remaining"},
                {  11, "There are 11 items remaining"},
            });
    }

    [Test]
    public void Test_English_Unsigned()
    {
        var smart = GetFormatter();
        const string format = "There {0:plural(en):is|are} {0} {0:plural(en):item|items} remaining";

        var expectedResults = new[]
        {
            "There are 0 items remaining",
            "There is 1 item remaining",
            "There are 2 items remaining"
        };

        for (ushort i = 0; i < expectedResults.Length; i++)
        {
            var actualResult = smart.Format(format, i);
            Assert.That(actualResult, Is.EqualTo(expectedResults[i]));
        }

        for (uint i = 0; i < expectedResults.Length; i++)
        {
            var actualResult = smart.Format(format, i);
            Assert.That(actualResult, Is.EqualTo(expectedResults[i]));
        }

        for (ulong i = 0; i < (ulong)expectedResults.Length; i++)
        {
            var actualResult = smart.Format(format, i);
            Assert.That(actualResult, Is.EqualTo(expectedResults[i]));
        }
    }

    [TestCase(0, "{0} personne")] // 0 is singular
    [TestCase(1, "{0} personne")] // 1 is singular
    [TestCase(2, "{0} personnes")] // 2 or more is plural
    [TestCase(50, "{0} personnes")]
    public void Test_French_2words(int count, string expected)
    {
        var smart = GetFormatter();
        var ci = CultureInfo.GetCultureInfo("fr");
        var actual = smart.Format(ci, "{0:plural:{0} personne|{0} personnes}", count);

        Assert.That(actual, Is.EqualTo(string.Format(ci, expected, count)));
    }

    [TestCase(0, "pas de personne")] // 0 is singular
    [TestCase(1, "une personne")] // 1 is singular
    [TestCase(2, "{0} personnes")] // 2 or more is plural
    [TestCase(50, "{0} personnes")]
    public void Test_French_3words(int count, string expected)
    {
        var smart = GetFormatter();
        var ci = CultureInfo.GetCultureInfo("fr");
        var actual = smart.Format(ci, "{0:plural:pas de personne|une personne|{0} personnes}", count);

        Assert.That(actual, Is.EqualTo(string.Format(ci, expected, count)));
    }

    [TestCase(-1, "-")]
    [TestCase(0, "pas de personne")] // 0 is singular
    [TestCase(1, "une personne")] // 1 is singular
    [TestCase(2, "{0} personnes")] // 2 is plural
    [TestCase(50, "{0} personnes")] // more than 2
    public void Test_French_4words(int count, string expected)
    {
        var smart = GetFormatter();
        var ci = CultureInfo.GetCultureInfo("fr");
        var actual = smart.Format(ci, "{0:plural:-|pas de personne|une personne|{0} personnes}", count);

        Assert.That(actual, Is.EqualTo(string.Format(ci, expected, count)));
    }

    [Test]
    public void Test_Turkish()
    {
        TestAllResults(
            new CultureInfo("tr-TR"),
            "{0} nesne kaldı.",
            new ExpectedResults {
                {   0, "0 nesne kaldı."},
                {   1, "1 nesne kaldı."},
                {   2, "2 nesne kaldı."},
            });

        TestAllResults(
            new CultureInfo("tr"),
            "Seçili {0:plural:nesneyi|nesneleri} silmek istiyor musunuz?",
            new ExpectedResults {
                {  -1, "Seçili nesneleri silmek istiyor musunuz?"},
                {   0, "Seçili nesneleri silmek istiyor musunuz?"},
                {0.5m, "Seçili nesneleri silmek istiyor musunuz?"},
                {   1, "Seçili nesneyi silmek istiyor musunuz?"},
                {1.5m, "Seçili nesneleri silmek istiyor musunuz?"},
                {   2, "Seçili nesneleri silmek istiyor musunuz?"},
                {  11, "Seçili nesneleri silmek istiyor musunuz?"},
            });
    }

    [Test]
    public void Test_Russian()
    {
        TestAllResults(
            new CultureInfo("ru-RU"),
            "Я купил {0} {0:plural:банан|банана|бананов}.",
            new ExpectedResults {
                {   0, "Я купил 0 бананов."},
                {   1, "Я купил 1 банан."},
                {   2, "Я купил 2 банана."},
                {  11, "Я купил 11 бананов."},
                {  20, "Я купил 20 бананов."},
                {  21, "Я купил 21 банан."},
                {  22, "Я купил 22 банана."},
                {  25, "Я купил 25 бананов."},
                {  120, "Я купил 120 бананов."},
                {  121, "Я купил 121 банан."},
                {  122, "Я купил 122 банана."},
                {  125, "Я купил 125 бананов."},
            });
    }

    [Test]
    public void Test_Czech()
    {
        TestAllResults(
            new CultureInfo("cs"),
            "{0:plural:Nemáte zprávu|Máte {} zprávu|Přišly Vám {} zprávy|Přišlo Vám {} zpráv}!",
            new ExpectedResults {
                {   0, "Nemáte zprávu!"},
                {   1, "Máte 1 zprávu!"},
                {   2, "Přišly Vám 2 zprávy!"},
                {   4, "Přišly Vám 4 zprávy!"},
                {   5, "Přišlo Vám 5 zpráv!"},
                {   6, "Přišlo Vám 6 zpráv!"},
            });
    }

    [Test]
    public void Test_Polish()
    {
        TestAllResults(
            new CultureInfo("pl"),
            "{0} {0:plural:miesiąc|miesiące|miesięcy} temu",
            new ExpectedResults {
                {   0, "0 miesięcy temu"},
                {   1, "1 miesiąc temu"},
                {   2, "2 miesiące temu"},
                {   3, "3 miesiące temu"},
                {   4, "4 miesiące temu"},
                {   5, "5 miesięcy temu"},
                {   6, "6 miesięcy temu"},
                {   7, "7 miesięcy temu"},
                {   8, "8 miesięcy temu"},
                {   9, "9 miesięcy temu"},
                {  10, "10 miesięcy temu"},
                {  11, "11 miesięcy temu"},
                {  12, "12 miesięcy temu"},
                {  13, "13 miesięcy temu"},
                {  14, "14 miesięcy temu"},
                {  15, "15 miesięcy temu"},
                {  16, "16 miesięcy temu"},
                {  17, "17 miesięcy temu"},
                {  18, "18 miesięcy temu"},
                {  19, "19 miesięcy temu"},
                {  20, "20 miesięcy temu"},
                {  21, "21 miesięcy temu"},
                {  22, "22 miesiące temu"},
                {  23, "23 miesiące temu"},
                {  24, "24 miesiące temu"},
                {  25, "25 miesięcy temu"},
                {  100, "100 miesięcy temu"},
                {  101, "101 miesięcy temu"},
                {  102, "102 miesiące temu"},
                {  103, "103 miesiące temu"},
                {  104, "104 miesiące temu"},
                {  105, "105 miesięcy temu"},
            });
    }

    [TestCase("{0} {0:plural(en):zero|one|many} {0:plural(pl):miesiąc|miesiące|miesięcy}", 0, "0 zero miesięcy")]
    [TestCase("{0} {0:plural(en):zero|one|many} {0:plural(pl):miesiąc|miesiące|miesięcy}", 1, "1 one miesiąc")]
    [TestCase("{0} {0:plural(en):zero|one|many} {0:plural(pl):miesiąc|miesiące|miesięcy}", 2, "2 many miesiące")]
    [TestCase("{0} {0:plural(en):zero|one|many} {0:plural(pl):miesiąc|miesiące|miesięcy}", 5, "5 many miesięcy")]
    public void NamedFormatter_should_use_specific_language(string format, object arg0, string expectedResult)
    {
        var smart = GetFormatter();
        var actualResult = smart.Format(format, arg0);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase("{0:plural:zero|one|many}", new string[0], "zero")]
    [TestCase("{0:plural:zero|one|many}", new[] { "alice" }, "one")]
    [TestCase("{0:plural:zero|one|many}", new[] { "alice", "bob" }, "many")]
    public void Should_Allow_IEnumerable_Parameter(string format, object arg0, string expectedResult)
    {
        var smart = GetFormatter();
        var culture = new CultureInfo("en-US");
        var actualResult = smart.Format(culture, format, arg0);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void Use_CustomPluralRuleProvider()
    {
        var smart = GetFormatter();

        Assert.Multiple(() =>
        {
            // ** German **
            var actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("de")),
                "{0:plural:Frau|Frauen}", new string[2], "more");
            Assert.That(actualResult, Is.EqualTo("Frauen"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("de")),
                "{0:plural:Frau|Frauen|einige Frauen|viele Frauen}", new string[4], "more");
            Assert.That(actualResult, Is.EqualTo("viele Frauen"));

            // ** English **

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("en")),
                "{0:plural:person|people}", new string[2], "more");
            Assert.That(actualResult, Is.EqualTo("people"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("en")),
                "{0:plural:person|people}", new string[1], "one");
            Assert.That(actualResult, Is.EqualTo("person"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")),
                "{0:plural:pas de personne|une personne|plusieurs personnes}", Array.Empty<string>(), "none");
            Assert.That(actualResult, Is.EqualTo("pas de personne"));

            // ** French **

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")),
                "{0:plural:pas de personne|une personne|plusieurs personnes}", new string[1], "one");
            Assert.That(actualResult, Is.EqualTo("une personne"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")),
                "{0:plural:pas de personne|une personne|deux personnes}", new string[2], "two");
            Assert.That(actualResult, Is.EqualTo("deux personnes"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")),
                "{0:plural:pas de personne|une personne|deux personnes|plusieurs personnes}", new string[3], "several");
            Assert.That(actualResult, Is.EqualTo("plusieurs personnes"));

            actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")),
                "{0:plural:une personne|deux personnes|plusieurs personnes|beaucoup de personnes}", new string[3],
                "many");
            Assert.That(actualResult, Is.EqualTo("beaucoup de personnes"));
        });
    }

    [TestCase("{0:plural:one|many} {1:plural:one|many} {2:plural:one|many}", "many one many")]
    [TestCase("There {0:plural:is|are} {0} {0:plural:item|items} remaining", "There are Convertible(0) items remaining")]
    [TestCase("There {1:plural:is|are} {1} {1:plural:item|items} remaining", "There is Convertible(1) item remaining")]
    public void Test_With_IConvertible(string format, string expectedResult)
    {
        var smart = GetFormatter();
        var culture = new CultureInfo("en-US");
        var args = new object[] { new ConvertibleDecimal(0), new ConvertibleDecimal(1), new ConvertibleDecimal(2) };
        var actualResult = smart.Format(culture, format, args);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void Should_Process_Signed_And_Unsigned_Numbers()
    {
        var smart = GetFormatter();
        foreach (var number in new object[]
                 {
                     (long)123, (ulong)123, (short)123, (ushort)123, (int)123, (uint)123,
                     (long)-123, (short) -123, (int) -123
                 })
        {
            Assert.That(smart.Format("{0:plural(en):zero|one|many}", number), Is.EqualTo("many"));
        }
    }

    [TestCase(1, "There {People.Count:plural:is a person.|are {} people.}", false)]
    [TestCase(2, "There {People.Count:plural:is a person.|are {} people.}", false)]
    [TestCase(1, "There {People.Count:is a person.|are {} people.}", true)]
    [TestCase(2, "There {People.Count:is a person.|are {} people.}", true)]
    public void Nested_PlaceHolders_Pluralization(int numOfPeople, string format, bool markAsDefault)
    {
        var data = numOfPeople == 1
            ? new {People = new List<object> {new {Name = "Name 1", Age = 20}}}
            : new {People = new List<object> {new {Name = "Name 1", Age = 20}, new {Name = "Name 2", Age = 30}}};
            
        var formatter = new SmartFormatter()
            .AddExtensions(new ReflectionSource())
            // Note: If pluralization AND conditional formatters are registered, the formatter
            //       name MUST be included in the format string, because both could return successful automatic evaluation
            // Here, we register only pluralization:
            .AddExtensions(new PluralLocalizationFormatter { CanAutoDetect = markAsDefault },
                new DefaultFormatter());
        var result = formatter.Format(CultureInfo.GetCultureInfo("en"), format, data);
            
        Assert.That(result, numOfPeople == 1 ? Is.EqualTo("There is a person.") : Is.EqualTo("There are 2 people."));
    }

    [TestCase(1, "There {People.Count:plural:|is a person|.~|are {} people|.}", "There |is a person|.")]
    [TestCase(2, "There {People.Count:plural:|is a person|.~|are {} people|.}", "There |are 2 people|.")]
    public void Pluralization_With_Changed_SplitChar(int numOfPeople, string format, string expected)
    {
        var data = numOfPeople == 1
            ? new {People = new List<object> {new {Name = "Name 1", Age = 20}}}
            : new {People = new List<object> {new {Name = "Name 1", Age = 20}, new {Name = "Name 2", Age = 30}}};

        var smart = new SmartFormatter()
            .AddExtensions(new ReflectionSource())
            // Set SplitChar from | to ~, so we can use | for the output string
            .AddExtensions(new PluralLocalizationFormatter { SplitChar = '~'},
                new DefaultFormatter());
            
        var result = smart.Format(format, data);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(0, "nobody", "pas de personne")] // 0 is singular
    [TestCase(1, "{0} person", "{0} personne")] // 1 is singular
    [TestCase(2, "{0} people", "{0} personnes")] // 2 or more is plural
    [TestCase(5, "a couple of people", "quelques personnes")]
    [TestCase(15, "many people", "beaucoup de gens")]
    [TestCase(50, "a lot of people", "une foule de personnes")]
    public void Pluralization_With_Changed_Default_Rule_Delegate(int count, string rawExpectedEnglish, string rawExpectedFrench)
    {
        // Note: This test changes a default rule delegate *globally*.
        //       It is not recommended, but possible.
        PluralRules.IsoLangToDelegate["en"] = (value, wordsCount) =>
        {
            if (wordsCount != 6) return -1;

            return Math.Abs(value) switch
            {
                <= 0 => 0,
                > 0 and < 2 => 1,
                >= 2 and < 3 => 2,
                > 2 and < 10 => 3,
                >= 10 and < 20 => 4,
                >= 20 => 5
            };
        };
        // Use the same rule delegate for English and French:
        PluralRules.IsoLangToDelegate["fr"] = PluralRules.IsoLangToDelegate["en"];

        var smart = GetFormatter();
        var ciEnglish = CultureInfo.GetCultureInfo("en");
        var ciFrench = CultureInfo.GetCultureInfo("fr");

        var actualEnglish = smart.Format(ciEnglish, "{0:plural:nobody|{} person|{} people|a couple of people|many people|a lot of people}", count);
        var actualFrench = smart.Format(ciFrench, "{0:plural:pas de personne|{} personne|{} personnes|quelques personnes|beaucoup de gens|une foule de personnes}", count);

        // Restore default rule delegates:
        PluralRules.RestoreDefault();

        Assert.Multiple(() =>
        {
            Assert.That(actualEnglish, Is.EqualTo(string.Format(ciEnglish, rawExpectedEnglish, count)));
            Assert.That(actualFrench, Is.EqualTo(string.Format(ciFrench, rawExpectedFrench, count)));
        });
    }
}
