using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using ExpectedResults = System.Collections.Generic.Dictionary<decimal, string>;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class PluralLocalizationFormatterTests
    {
        private void TestAllResults(CultureInfo cultureInfo, string format, ExpectedResults expectedValuesAndResults)
        {
            foreach (var test in expectedValuesAndResults)
            {
                var value = test.Key;
                var expected = test.Value;
                var actual = Smart.Format(cultureInfo, format, value);

                Assert.That(actual, Is.EqualTo(expected));
                Debug.WriteLine(actual);
            }
        }

        [Test]
        public void Test_Default()
        {
            TestAllResults(
                CultureInfo.CreateSpecificCulture("en-us"),
                "There {0:is|are} {0} {0:item|items} remaining",
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
                CultureInfo.GetCultureInfo("en-US"),
                "There {0:is|are} {0} {0:item|items} remaining",
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
        public void Test_Turkish()
        {
            TestAllResults(
                CultureInfo.GetCultureInfo("tr-TR"),
                "{0} nesne kaldı.",
                new ExpectedResults {
                    {   0, "0 nesne kaldı."},
                    {   1, "1 nesne kaldı."},
                    {   2, "2 nesne kaldı."},
                });

            TestAllResults(
                CultureInfo.GetCultureInfo("tr"),
                "Seçili {0:nesneyi|nesneleri} silmek istiyor musunuz?",
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
                CultureInfo.GetCultureInfo("ru-RU"),
                "Я купил {0} {0:банан|банана|бананов}.",
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


    }
}
