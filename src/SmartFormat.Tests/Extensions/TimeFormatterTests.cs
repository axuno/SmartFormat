using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class TimeFormatterTests
    {
        public TimeFormatterTests()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            smart.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

            var timeFormatter = smart.FormatterExtensions.FirstOrDefault(fmt => fmt.Names.Contains("time")) as TimeFormatter;
            if (timeFormatter == null)
            {
                timeFormatter = new TimeFormatter("en");
                smart.AddExtensions(timeFormatter);
            }
        }

        [Test]
        public void CreateTimeFormatterCtor_WithIllegalLanguage()
        {
            TimeFormatter tf;
            Assert.Throws<ArgumentException>(() => tf = new TimeFormatter("illegal-language"));
        }

        [Test]
        public void CreateTimeFormatterCtor_WithLegalLanguage()
        {
            TimeFormatter? tf = null;
            Assert.DoesNotThrow(() => tf = new TimeFormatter("en"));
            Assert.AreEqual("en", tf?.DefaultTwoLetterISOLanguageName);
        }

        public object[] GetArgs()
        {
            return new object[] {
                TimeSpan.Zero,
                new TimeSpan(1,1,1,1,1),
                new TimeSpan(0,2,0,2,0),
                new TimeSpan(3,0,0,3,0),
                new TimeSpan(0,0,0,0,4),
                new TimeSpan(5,0,0,0,0),
            };
        }

        [Test]
        public void Test_Defaults()
        {
            var formats = new string[] {
                "{0:time:}",
                "{1:time:}",
                "{2:time:}",
                "{3:time:}",
                "{4:time:}",
                "{5:time:}",
            };
            var expected = new string[] {
                "less than 1 second",
                "1 day 1 hour 1 minute 1 second",
                "2 hours 2 seconds",
                "3 days 3 seconds",
                "less than 1 second",
                "5 days",
            };
            var args = GetArgs();
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(formats, args, expected);
        }

        [Test]
        public void Test_Options()
        {
            var formats = new string[] {
                "{0:time:noless}",
                "{1:time:hours}",
                "{1:time:hours minutes}",
                "{2:time:days milliseconds}",
                "{2:time:days milliseconds auto}",
                "{2:time:days milliseconds short}",
                "{2:time:days milliseconds fill}",
                "{2:time:days milliseconds full}",
                "{3:time:abbr}",
            };
            var expected = new string[] {
                "0 seconds",
                "25 hours",
                "25 hours 1 minute",
                "2 hours 2 seconds",
                "2 hours 2 seconds",
                "2 hours",
                "2 hours 0 minutes 2 seconds 0 milliseconds",
                "0 days 2 hours 0 minutes 2 seconds 0 milliseconds",
                "3d 3s",
            };
            var args = GetArgs();
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(formats, args, expected);
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(23)]
        [TestCase(-12)]
        [TestCase(-23)]
        public void TimeSpanFromGivenTimeToCurrentTime(int diffHours)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            // test will work in any TimeZone
            var now = DateTime.Now;
            var dateTime = now.AddHours(diffHours);
            SystemTime.SetDateTime(now);
            var format = "{0:time(abbr hours noless)}";
            // The difference to current time with a DateTime as an argument
            var actual = smart.Format(format, dateTime);
            Assert.AreEqual($"{diffHours * -1}h", actual);
            // Make sure that logic for TimeSpan and DateTime arguments are the same
            Assert.AreEqual(actual, smart.Format(format, now - dateTime));
            Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
            SystemTime.ResetDateTime();
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(23)]
        [TestCase(-12)]
        [TestCase(-23)]
        public void TimeSpanOffsetFromGivenTimeToCurrentTime(int diffHours)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            // test will work in any TimeZone
            var now = DateTimeOffset.Now;
            var dateTimeOffset = now.AddHours(diffHours);
            SystemTime.SetDateTimeOffset(now);
            var format = "{0:time(abbr hours noless)}";
            // The difference to current time with a DateTimeOffset as an argument
            var actual = smart.Format(format, dateTimeOffset);
            Assert.AreEqual($"{diffHours * -1}h", actual);
            // Make sure that logic for TimeSpan and DateTime arguments are the same
            Assert.AreEqual(actual, smart.Format(format, now - dateTimeOffset));
            Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
            SystemTime.ResetDateTime();
        }
    }
}
