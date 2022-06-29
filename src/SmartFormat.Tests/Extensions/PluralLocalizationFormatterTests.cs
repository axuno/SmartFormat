using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Utilities;
using ExpectedResults = System.Collections.Generic.Dictionary<decimal, string>;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class PluralLocalizationFormatterTests
{
    public static SmartFormatter GetFormatter(SmartSettings? smartSettings = null)
    {
        var smart = Smart.CreateDefaultSmartFormat(smartSettings ?? new SmartSettings());
        smart.InsertExtension(1, new PluralLocalizationFormatter());
        return smart;
    }

    private static void TestAllResults(CultureInfo cultureInfo, string format, ExpectedResults expectedValuesAndResults)
    {
        foreach (var test in expectedValuesAndResults)
        {
            var smart = GetFormatter();
            var value = test.Key;
            var expected = test.Value;
            var actual = smart.Format(cultureInfo, format, value);

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
            Assert.AreEqual(expectedResults[i], actualResult);
        }

        for (uint i = 0; i < expectedResults.Length; i++)
        {
            var actualResult = smart.Format(format, i);
            Assert.AreEqual(expectedResults[i], actualResult);
        }

        for (ulong i = 0; i < (ulong)expectedResults.Length; i++)
        {
            var actualResult = smart.Format(format, i);
            Assert.AreEqual(expectedResults[i], actualResult);
        }
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
        Assert.AreEqual(expectedResult, actualResult);
    }

    [TestCase("{0:plural:zero|one|many}", new string[0], "zero")]
    [TestCase("{0:plural:zero|one|many}", new[] { "alice" }, "one")]
    [TestCase("{0:plural:zero|one|many}", new[] { "alice", "bob" }, "many")]
    public void Test_should_allow_ienumerable_parameter(string format, object arg0, string expectedResult)
    {
        var smart = GetFormatter();
        var culture = new CultureInfo("en-US");
        var actualResult = smart.Format(culture, format, arg0);
        Assert.AreEqual(expectedResult, actualResult);
    }

    [Test]
    public void Test_With_CustomPluralRuleProvider()
    {
        var smart = GetFormatter();
        var actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("de")), "{0:plural:Frau|Frauen}", new string[2], "more");
        Assert.AreEqual("Frauen", actualResult);

        actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("en")), "{0:plural:person|people}", new string[2], "more");
        Assert.AreEqual("people", actualResult);

        actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("en")), "{0:plural:person|people}", new string[1], "one");
        Assert.AreEqual("person", actualResult);

        actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")), "{0:plural:une personne|deux personnes|plusieurs personnes}", new string[3], "several");
        Assert.AreEqual("plusieurs personnes", actualResult);

        actualResult = smart.Format(new CustomPluralRuleProvider(PluralRules.GetPluralRule("fr")), "{0:plural:une personne|deux personnes|plusieurs personnes|beaucoup de personnes}", new string[3], "several");
        Assert.AreEqual("beaucoup de personnes", actualResult);
    }

    [Test]
    public void Should_Process_Signed_And_Unsigned_Numbers()
    {
        var smart = GetFormatter();
        foreach (var number in new object[]
                     { (long)123, (ulong)123, (short)123, (ushort)123, (int)123, (uint)123 })
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
}