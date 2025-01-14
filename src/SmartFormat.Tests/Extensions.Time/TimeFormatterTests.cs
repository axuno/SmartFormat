using System;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Extensions.Time.Utilities;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class TimeFormatterTests
{
    private static SmartFormatter GetFormatter()
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
        {
            Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError},
            Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}
        });

        if (smart.GetFormatterExtension<TimeFormatter>() is null)
        {
            smart.AddExtensions(new TimeFormatter());
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
            new TimeSpan(5,0,0,0,0)
        };
    }

    [Test]
    public void UseTimeFormatter_WithUnsupportedLanguage()
    {
        var smart = GetFormatter();
        var timeFormatter = smart.GetFormatterExtension<TimeFormatter>()!;
        timeFormatter.FallbackLanguage = string.Empty;

        Assert.That(
            delegate { return smart.Format(CultureInfo.InvariantCulture, "{0:time:noless}", new TimeSpan(1, 2, 3)); },
            Throws.InstanceOf<FormattingException>().And.Message.Contains("TimeTextInfo could not be found"),
            "Language as argument");
    }

    [Test]
    public void Setting_Unknown_FallbackLanguage_Should_Throw()
    {
        var smart = GetFormatter();
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

    [Test]
    public void UseTimeFormatter_WithSupportedlLanguage()
    {
        var smart = GetFormatter();

        Assert.DoesNotThrow(() => smart.Format("{0:time(en):noless}", new TimeSpan(1,2,3)));
        Assert.DoesNotThrow(() => smart.Format(CultureInfo.GetCultureInfo("en"), "{0:time:noless}", new TimeSpan(1,2,3)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void UseTimeFormatter_With_Unimplemented_FallbackLanguage(bool useFallbackLanguage)
    {
        var smart = GetFormatter();
        var timeFormatter = smart.GetFormatterExtension<TimeFormatter>()!;
        timeFormatter.FallbackLanguage = useFallbackLanguage ? "en" : string.Empty;

        if (useFallbackLanguage)
            Assert.That(() => smart.Format("{0:time(nl):noless}", new TimeSpan(1, 2, 3)), Throws.Nothing);
        else
            Assert.That(() => smart.Format("{0:time(nl):noless}", new TimeSpan(1, 2, 3)), Throws.TypeOf<FormattingException>().And.Message.Contains(nameof(TimeTextInfo)));
    }

    [Test]
    public void Explicit_Formatter_With_UnsupportedArgType_Should_Throw()
    {
        var smart = GetFormatter();
        // Arrays are not supported
        Assert.That(() => smart.Format("{0:time:}", Array.Empty<TimeSpan>()), Throws.Exception.TypeOf<FormattingException>());
    }

    [Test]
    public void Formatter_ReturnsFalse_For_Implicit_Invocation()
    {
        var tf = new TimeFormatter();
        Assert.That(tf.TryEvaluateFormat(new FormattingInfo()), Is.False);
    }

    [TestCase("{0:time: {:list:|, | and }}", "en", "less than 1 second")]
    [TestCase("{1:time: {:list:|, | and }}", "en", "1 day, 1 hour, 1 minute and 1 second")]
    [TestCase("{2:time: {:list:|, | and }}", "en", "2 hours and 2 seconds")]
    [TestCase("{3:time: {:list:|, | and }}", "en", "3 days and 3 seconds")]
    [TestCase("{4:time: {:list:|, | and }}", "en", "less than 1 second")]
    [TestCase("{5:time: {:list:|, | and }}", "en", "5 days")]
    public void NestedListFormatTest(string format, string language, string expected)
    {
        var args = GetArgs();
        var smart = GetFormatter();
        var actual = smart.Format(CultureInfo.GetCultureInfo(language), format, args);
        Assert.That(actual, Is.EqualTo(expected));
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
        var smart = GetFormatter();
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
        var smart = GetFormatter();
            
        var actual = smart.Format(format, args);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TimeSpanRoundedToString()
    {
        var tsInfo = CommonLanguagesTimeTextInfo.GetTimeTextInfo("en")!;
        var result = new TimeSpan(0, 23, 0, 0)
            .Round(TimeSpan.FromDays(1).Ticks).ToTimeString(TimeSpanFormatOptions.None, tsInfo);

        Assert.That(result, Is.EqualTo("1 day"));
    }

#if NET6_0_OR_GREATER
    [Test]
    public void TimeOnlyArgument()
    {
        var args = GetArgs();
        var smart = GetFormatter();

        var actual = smart.Format("{0:time(en):noless}", new TimeOnly(13, 12, 11));
        Assert.That(actual, Is.EqualTo("13 hours 12 minutes 11 seconds"));
    }
#endif

    [TestCase(0)]
    [TestCase(12)]
    [TestCase(23)]
    [TestCase(-12)]
    [TestCase(-23)]
    public void TimeSpanFromGivenTimeToCurrentTime(int diffHours)
    {
        var smart = GetFormatter();
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

    [Test]
    public void DateTimeOffset_As_Argument_Should_FormatAsTimeSpan_ToLocaltime()
    {
        var smart = GetFormatter();
        var dateTimeOffset = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        SystemTime.SetDateTimeOffset(dateTimeOffset.AddDays(1));
        var format = "{0:time(en):abbr hours noless:}";

        var actual = smart.Format(format, dateTimeOffset);
        Assert.That(actual, Is.EqualTo("24h"));
        SystemTime.ResetDateTime();
    }

    [Test]
    public void DateTime_As_Argument_Should_FormatAsTimeSpan_ToLocaltime()
    {
        var smart = GetFormatter();
        var dateTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        SystemTime.SetDateTime(dateTime.AddDays(5));
        var format = "{0:time(en):abbr days noless:}";

        var actual = smart.Format(format, dateTime);
        Assert.That(actual, Is.EqualTo("5d"));
        SystemTime.ResetDateTime();
    }

    [TestCase(0)]
    [TestCase(12)]
    [TestCase(23)]
    [TestCase(-12)]
    [TestCase(-23)]
    public void TimeSpanOffsetFromGivenTimeToCurrentTime(int diffHours)
    {
        var smart = GetFormatter();
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
