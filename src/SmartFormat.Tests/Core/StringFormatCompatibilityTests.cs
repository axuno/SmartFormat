using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace SmartFormat.Tests.Core
{
    /// <summary>
    /// These tests run with string.Format compatibility switched ON
    /// </summary>
    [TestFixture]
    public class StringFormatCompatibilityTests
    {
        private static SmartFormatter _formatter = Smart.CreateDefaultSmartFormat();

        [SetUp]
        public void Setup()
        {
            _formatter.Settings.StringFormatCompatibility = true;
        }

        [Test]
        public void IndexPlaceholderDecimal()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "Today's temperature is {0}°C.";
            var temp = 20.45m;
            Assert.That(_formatter.Format(cultureUS, fmt, temp), Is.EqualTo(string.Format(cultureUS, fmt, temp)));
            Assert.That(_formatter.Format(cultureDE, fmt, temp), Is.EqualTo(string.Format(cultureDE, fmt, temp)));
        }

        [Test]
        public void IndexPlaceholderDateTime()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "It is now {0:d} at {0:t}";
            var now = DateTime.Now;
            Assert.That(_formatter.Format(cultureUS, fmt, now), Is.EqualTo(string.Format(cultureUS, fmt, now)));
            Assert.That(_formatter.Format(cultureDE, fmt, now), Is.EqualTo(string.Format(cultureDE, fmt, now)));
        }

        [Test]
        public void IndexPlaceholderDateTimeHHmmss()
        {
            // columns in the time part must not be recognized as delimiters of a named placeholder
            var fmt = "It is now {0:yyyy/MM/dd HH:mm:ss}";
            var now = DateTime.Now;
            Assert.That(_formatter.Format(fmt, now), Is.EqualTo(string.Format(fmt, now)));
        }

        [Test]
        public void NamedPlaceholderDateTimeHHmmss()
        {
            // columns in the time part must not be recognized as delimiters of a named placeholder
            var now = DateTime.Now;
            var smartFmt = "It is now {Date:yyyy/MM/dd HH:mm:ss}";
            var stringFmt = $"It is now {now.Date:yyyy/MM/dd HH:mm:ss}";
            Assert.That(_formatter.Format(smartFmt, now), Is.EqualTo(stringFmt));
        }

        [Test]
        public void IndexPlaceholderAlignment()
        {
            // columns in the time part must not be recognized as delimiters of a named placeholder
            var fmt = "Year: {0,-6}  Amount: {1,15:N0}";
            var year = 2017;
            var amount = 1025632;
            Assert.That(_formatter.Format(fmt, year, amount), Is.EqualTo(string.Format(fmt, year, amount)));
        }

        [Test]
        public void SmartFormat_With_Three_Arguments()
        {
            var args = new Dictionary<string, object> { {"key1", "value1"}, {"key2", "value2"}, {"key3", "value3"}};
            Assert.That(_formatter.Format("{0} {1} {2}", args["key1"], args["key2"], args["key3"]), Is.EqualTo($"{args["key1"]} {args["key2"]} {args["key3"]}"));
        }

        [Test]
        public void NamedPlaceholderDecimal()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "Today's temperature is {0}°C.";
            var temp = 20.45m;
            Assert.That(_formatter.Format(cultureUS, fmt, temp), Is.EqualTo(string.Format(cultureUS, fmt, temp)));
            Assert.That(_formatter.Format(cultureDE, fmt, temp), Is.EqualTo(string.Format(cultureDE, fmt, temp)));
        }

        [Test]
        public void NamedPlaceholderDateTime()
        {
            var now = new DateTime(2021, 12, 22, 14, 18, 12);
            var smartFmt = "It is now {Date:d} at {Date:t}";
            var stringFmt = $"It is now {now.Date:d} at {now.Date:t}";
            
            Assert.That(_formatter.Format(smartFmt, now), Is.EqualTo(stringFmt));
        }

        [Test]
        public void NamedPlaceholderAlignment()
        {
            var yearAmount = new Tuple<long,long>(2017, 1025632);
            var smartFmt = "Year: {Item1,-6}  Amount: {Item2,15:N0}";
            var stringFmt = $"Year: {yearAmount.Item1,-6}  Amount: {yearAmount.Item2,15:N0}";

            Assert.That(_formatter.Format(smartFmt, yearAmount), Is.EqualTo(stringFmt));
        }

        [Test]
        public void NamedPlaceholder_DecimalArg()
        {
            _formatter.Settings.StringFormatCompatibility = false;
            var pricePerOunce = 17.36m;
            var format = "The current price is {0} per ounce.";

            Assert.That(_formatter.Format(format, pricePerOunce), Is.EqualTo(string.Format(format, pricePerOunce)));
        }

        [Test]
        public void NamedPlaceholder_DecimalCurrencyArg()
        {
            _formatter.Settings.StringFormatCompatibility = false;
            var pricePerOunce = 17.36m;
            var format = "The current price is {0:C2} per ounce.";

            Assert.That(_formatter.Format(format, pricePerOunce), Is.EqualTo(string.Format(format, pricePerOunce)));
        }

        /// <summary>
        /// Custom formatters are not recognized with string.Format compatibility switched ON
        /// </summary>
        [TestCase("{0:FormatterName(true|false):one|two|default}", true)]
        [TestCase("{0:FormatterName(string|String):one|two|default}", "String")]
        [TestCase("{0,10:FormatterName(string|String):one|two|default}", "value")]
        [TestCase("{0:d:FormatterName(string|String):one|two|default}", "2021-12-01")]
        public void FormatterName_And_Options_Should_Be_Ignored(string format, object arg0)
        {
            Assert.That(_formatter.Format(format, arg0), Is.EqualTo(string.Format(format, arg0)));
        }

        [TestCase("{0:yyyy/MM/dd HH:mm:ss FormatterName(string|String):one|two|default}", "2021-12-01")] // results in "nonsense"
        [TestCase("{0:yyyy/MM/dd HH:mm:ss}", "2021-12-01")]
        public void FormatterName_And_Options_Should_Be_Ignored2(string format, DateTime arg0)
        {
            Assert.That(_formatter.Format(format, arg0), Is.EqualTo(string.Format(format, arg0)));
        }
    }
}