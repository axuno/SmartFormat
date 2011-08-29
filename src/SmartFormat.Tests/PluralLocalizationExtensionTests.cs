using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Extensions;
using TestCriteria = System.Collections.Generic.Dictionary<decimal, string>;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class PluralLocalizationExtensionTests
    {
        private void TestResults(string cultureName, string format, TestCriteria expectedValuesAndResults)
        {
            var cultureInfo = (cultureName == null) ? null : CultureInfo.GetCultureInfo(cultureName);
            foreach (var test in expectedValuesAndResults)
            {
                var value = test.Key;
                var expected = test.Value;
                var actual = Smart.Format(format, value);

                Assert.That(actual, Is.EqualTo(expected));
                Debug.WriteLine(actual);
            }
        }

        [Test]
        public void Test_Default()
        {
            TestResults(
                null,
                "There {0:is|are} {0} {0:item|items} remaining",
                new TestCriteria {
                    {   1, "There is 1 item remaining"},
                    {   0, "There are 0 items remaining"},
                    {   2, "There are 2 items remaining"},
                    {  11, "There are 11 items remaining"},
                    {0.5m, "There are 0.5 items remaining"},
                    {1.5m, "There are 1.5 items remaining"},
                    {  -1, "There are -1 items remaining"},
                });
        }

        [Test]
        public void Test_English()
        {
            TestResults(
                "en",
                "There {0:is|are} {0} {0:item|items} remaining",
                new TestCriteria {
                    {   1, "There is 1 item remaining"},
                    {   0, "There are 0 items remaining"},
                    {   2, "There are 2 items remaining"},
                    {  11, "There are 11 items remaining"},
                    {0.5m, "There are 0.5 items remaining"},
                    {1.5m, "There are 1.5 items remaining"},
                    {  -1, "There are -1 items remaining"},
                });
        }


    }
}
