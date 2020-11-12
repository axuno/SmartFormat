using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class StringFormatCompatibilityTests
    {

        [Test]
        public void IndexPlaceholderDecimal()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "Today's temperature is {0}°C.";
            var temp = 20.45m;
            Assert.AreEqual(string.Format(cultureUS, fmt, temp), Smart.Format(cultureUS, fmt, temp));
            Assert.AreEqual(string.Format(cultureDE, fmt, temp), Smart.Format(cultureDE, fmt, temp));
        }

        [Test]
        public void IndexPlaceholderDateTime()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "It is now {0:d} at {0:t}";
            var now = DateTime.Now;
            Assert.AreEqual(string.Format(cultureUS, fmt, now), Smart.Format(cultureUS, fmt, now));
            Assert.AreEqual(string.Format(cultureDE, fmt, now), Smart.Format(cultureDE, fmt, now));
        }

        [Test]
        public void IndexPlaceholderDateTimeHHmmss()
        {
            // columns in the time part must not be recognized as delimiters of a named placeholder
            // (except a formatter's name would really be 'yyyy/MM/dd HH')
            var fmt = "It is now {0:yyyy/MM/dd HH:mm:ss}";
            var now = DateTime.Now;
            Assert.AreEqual(string.Format(fmt, now), Smart.Format(fmt, now));
        }

        [Test]
        public void IndexPlaceholderAlignment()
        {
            // columns in the time part must not be recogniced as delimiters of a named placeholder
            // (except a formatter's name would really be 'yyyy/MM/dd HH')
            var fmt = "Year: {0,-6}  Amount: {1,15:N0}";
            var year = 2017;
            var amount = 1025632;
            Assert.AreEqual(string.Format(fmt, year, amount), Smart.Format(fmt, year, amount));
        }

        [Test]
        public void SmartFormat_With_Three_Arguments()
        {
            var args = new Dictionary<string, object> { {"key1", "value1"}, {"key2", "value2"}, {"key3", "value3"}};
            Assert.AreEqual($"{args["key1"]} {args["key2"]} {args["key3"]}", Smart.Format("{0} {1} {2}", args["key1"], args["key2"], args["key3"]));
        }

        [Test]
        public void NamedPlaceholderDecimal()
        {
            var cultureUS = new CultureInfo("en-US");
            var cultureDE = new CultureInfo("de-DE");
            var fmt = "Today's temperature is {0}°C.";
            var temp = 20.45m;
            Assert.AreEqual(string.Format(cultureUS, fmt, temp), Smart.Format(cultureUS, fmt, temp));
            Assert.AreEqual(string.Format(cultureDE, fmt, temp), Smart.Format(cultureDE, fmt, temp));
        }

        [Test]
        public void NamedPlaceholderDateTime()
        {
            var now = DateTime.Now;
            var smartFmt = "It is now {Date:d} at {Date:t}";
            var stringFmt = $"It is now {now.Date:d} at {now.Date:t}";
            
            Assert.AreEqual(stringFmt, Smart.Format(smartFmt, now));
        }

        [Test]
        public void NamedPlaceholderDateTimeHHmmss()
        {
            // columns in the time part must not be recogniced as delimiters of a named placeholder
            // (except a formatter's name would really be 'yyyy/MM/dd HH')
            var now = DateTime.Now;
            var smartFmt = "It is now {Date:yyyy/MM/dd HH:mm:ss}";
            var stringFmt = $"It is now {now.Date:yyyy/MM/dd HH:mm:ss}";
            Assert.AreEqual(stringFmt, Smart.Format(smartFmt, now));
        }

        [Test]
        public void NamedPlaceholderAlignment()
        {
            var yearAmount = new Tuple<long,long>(2017, 1025632);
            var smartFmt = "Year: {Item1,-6}  Amount: {Item2,15:N0}";
            var stringFmt = $"Year: {yearAmount.Item1,-6}  Amount: {yearAmount.Item2,15:N0}";

            Assert.AreEqual(stringFmt, Smart.Format(smartFmt, yearAmount));
        }
    }
}