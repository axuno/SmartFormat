using System;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class TimeFormatterTests
    {
        private SmartFormatter GetStandardFormatter()
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            {
                Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError},
                Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}
            });

            if (smart.GetFormatterExtension<TimeFormatter>() is null)
            {
                var timeFormatter = new TimeFormatter();
                smart.AddExtensions(timeFormatter);
            }

            return smart;
        }

        private static object[] GetArgs()
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
        public void UseTimeFormatter_WithIllegalLanguage()
        {
            var smart = GetStandardFormatter();
            var timeFormatter = smart.GetFormatterExtension<TimeFormatter>()!;
            timeFormatter.FallbackLanguage = string.Empty;

            Assert.That(() => smart.Format(CultureInfo.InvariantCulture, "{0:time:noless}", new TimeSpan(1, 2, 3)),
                Throws.InstanceOf<FormattingException>().And.Message.Contains("TimeTextInfo could not be found"),
                "Language as argument");
        }

        [Test]
        public void Setting_Unknown_FallbackLanguage_Should_Throw()
        {
            var smart = GetStandardFormatter();
            var timeFormatter = smart.GetFormatterExtension<TimeFormatter>()!;
            Exception? ex = null;
            try
            {
                timeFormatter.FallbackLanguage = "something-not-existing";
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.That(ex, Is.InstanceOf<Exception>().And.Message.Contains(nameof(TimeTextInfo)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UseTimeFormatter_With_Unimplemented_Language(bool useFallbackLanguage)
        {
            var smart = GetStandardFormatter();
            var timeFormatter = smart.GetFormatterExtension<TimeFormatter>()!;
            timeFormatter.FallbackLanguage = useFallbackLanguage ? "en" : string.Empty;

            if(useFallbackLanguage)
                Assert.That(() => smart.Format("{0:time(nl):noless}", new TimeSpan(1,2,3)), Throws.Nothing);
            else
                Assert.That(() => smart.Format("{0:time(nl):noless}", new TimeSpan(1,2,3)), Throws.TypeOf<FormattingException>().And.Message.Contains(nameof(TimeTextInfo)));
        }


        [Test]
        public void UseTimeFormatter_WithLegalLanguage()
        {
            var smart = GetStandardFormatter();

            Assert.DoesNotThrow(() => smart.Format("{0:time(en):noless}", new TimeSpan(1,2,3)));
            Assert.DoesNotThrow(() => smart.Format(CultureInfo.GetCultureInfo("en"), "{0:time:noless}", new TimeSpan(1,2,3)));
        }

        [Test]
        public void Explicit_Formatter_With_Unsupported_ArgType_Should_Throw()
        {
            var smart = GetStandardFormatter();
            Assert.That(() => smart.Format("{0:time:}", DateTime.UtcNow), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void Formatter_With_NestedFormat_Should_Throw()
        {
            var smart = GetStandardFormatter();
            Assert.That(() => smart.Format("{0:time:{}}", DateTime.UtcNow), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void DefaultFormatOptions_Can_Be_Set()
        {
            var formatter = new TimeFormatter {DefaultFormatOptions = TimeSpanFormatOptions.RangeDays};
            Assert.That(formatter.DefaultFormatOptions, Is.EqualTo(TimeSpanFormatOptions.RangeDays));
        }
        
        [TestCase("{0:time:}", "en", "less than 1 second")]
        [TestCase("{1:time:}","en", "1 day 1 hour 1 minute 1 second")]
        [TestCase("{2:time:}","en", "2 hours 2 seconds")]
        [TestCase("{3:time:}","en", "3 days 3 seconds")]
        [TestCase("{4:time:}","en", "less than 1 second")]
        [TestCase("{5:time:}", "en", "5 days")]
        [TestCase("{0:time:}", "de", "weniger als 1 Sekunde")]
        [TestCase("{1:time:}","de", "1 Tag 1 Stunde 1 Minute 1 Sekunde")]
        [TestCase("{2:time:}","de", "2 Stunden 2 Sekunden")]
        [TestCase("{3:time:}","de", "3 Tage 3 Sekunden")]
        [TestCase("{4:time:}","de", "weniger als 1 Sekunde")]
        [TestCase("{5:time:}", "de", "5 Tage")]
        public void Test_Defaults(string format, string language, string expected)
        {
            var args = GetArgs();
            var smart = GetStandardFormatter();
            var actual = smart.Format(CultureInfo.GetCultureInfo(language), format, args);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("{0:time(en):noless}","0 seconds")]
        [TestCase("{1:time(en):hours}","25 hours")]
        [TestCase("{1:time(en):hours minutes}","25 hours 1 minute")]
        [TestCase("{2:time(en):days milliseconds}","2 hours 2 seconds")]
        [TestCase("{2:time(en):days milliseconds auto}","2 hours 2 seconds")]
        [TestCase("{2:time(en):days milliseconds short}","2 hours")]
        [TestCase("{2:time(en):days milliseconds fill}","2 hours 0 minutes 2 seconds 0 milliseconds")]
        [TestCase("{2:time(en):days milliseconds full}","0 days 2 hours 0 minutes 2 seconds 0 milliseconds")]
        [TestCase( "{3:time(en):abbr}", "3d 3s")]
        [TestCase("{0:time(fr):noless}","0 seconde")]
        [TestCase("{1:time(fr):hours}","25 heures")]
        [TestCase("{1:time(fr):hours minutes}","25 heures 1 minute")]
        [TestCase("{2:time(fr):days milliseconds}","2 heures 2 secondes")]
        [TestCase("{2:time(fr):days milliseconds auto}","2 heures 2 secondes")]
        [TestCase("{2:time(fr):days milliseconds short}","2 heures")]
        [TestCase("{2:time(fr):days milliseconds fill}","2 heures 0 minute 2 secondes 0 milliseconde")]
        [TestCase("{2:time(fr):days milliseconds full}","0 jour 2 heures 0 minute 2 secondes 0 milliseconde")]
        [TestCase( "{3:time(fr):abbr}", "3j 3s")]
        public void Test_Options(string format, string expected)
        {
            var args = GetArgs();
            var smart = GetStandardFormatter();
            
            var actual = smart.Format(format, args);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(23)]
        [TestCase(-12)]
        [TestCase(-23)]
        public void TimeSpanFromGivenTimeToCurrentTime(int diffHours)
        {
            var smart = GetStandardFormatter();
            // test will work in any TimeZone
            var now = DateTime.Now;
            var dateTime = now.AddHours(diffHours);
            SystemTime.SetDateTime(now);
            
            // This notation - using formats as formatter options - was allowed in Smart.Format v2.x, but is now depreciated.
            // It is still detected and working, as long as the format part is left empty
            var formatDepreciated = "{0:time(abbr hours noless)}";

            // This format string is recommended for Smart.Format v3 and later
            var format = "{0:time:abbr hours noless:}";

            // The difference to current time with a DateTime as an argument
            var actual = smart.Format(format, dateTime);
            Assert.That(actual, Is.EqualTo(smart.Format(formatDepreciated, dateTime)));
            Assert.That(actual, Is.EqualTo($"{diffHours * -1}h"));
            // Make sure that logic for TimeSpan and DateTime arguments are the same
            Assert.That(actual, Is.EqualTo(smart.Format(format, now - dateTime)));
            
            SystemTime.ResetDateTime();
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(23)]
        [TestCase(-12)]
        [TestCase(-23)]
        public void TimeSpanOffsetFromGivenTimeToCurrentTime(int diffHours)
        {
            var smart = GetStandardFormatter();
            // test will work in any TimeZone
            var now = DateTimeOffset.Now;
            var dateTimeOffset = now.AddHours(diffHours);
            SystemTime.SetDateTimeOffset(now);

            // This notation - using formats as formatter options - was allowed in Smart.Format v2.x, but is now depreciated.
            // It is still detected and working, as long as the format part is left empty
            var formatDepreciated = "{0:time(abbr hours noless)}";

            // This format string is recommended for Smart.Format v3 and later
            var format = "{0:time:abbr hours noless:}";

            // The difference to current time with a DateTimeOffset as an argument
            var actual = smart.Format(format, dateTimeOffset);
            Assert.That(actual, Is.EqualTo(smart.Format(formatDepreciated, dateTimeOffset)));
            Assert.That(actual, Is.EqualTo($"{diffHours * -1}h"));

            // Make sure that logic for TimeSpan and DateTime arguments are the same
            Assert.That(actual, Is.EqualTo(smart.Format(format, now - dateTimeOffset)));
            
            SystemTime.ResetDateTime();
        }
    }
}
