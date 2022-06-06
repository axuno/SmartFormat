using NUnit.Framework;
using System.Reflection;
using SmartFormat.Utilities;
using SmartFormat.Extensions.Time.Utilities;

namespace SmartFormat.Tests.Utilities;

[TestFixture]
public class CommonLanguagesTimeTextInfoTests
{
    [TestCase("en", "English")]
    [TestCase("fr", "French")]
    [TestCase("es", "Spanish")]
    [TestCase("pt", "Portuguese")]
    [TestCase("it", "Italian")]
    [TestCase("de", "German")]
    public void Get_TimeTextInfo_For_BuiltIn_Languages(string language, string property)
    {
        var tti =  typeof(CommonLanguagesTimeTextInfo)
            .GetProperty(property, BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null, null) as TimeTextInfo;

        Assert.That(tti, Is.Not.Null);
        Assert.That(() => CommonLanguagesTimeTextInfo.GetTimeTextInfo(language)!.GetLessThanText("1"),
            Is.EqualTo(tti?.GetLessThanText("1")));
    }

    [Test]
    public void Add_Valid_Custom_Language_TimeTextInfo()
    {
        var language = "nl"; // dummy - it's English, not Dutch ;-)
        TimeTextInfo custom = new()
        {
            PluralRule = PluralRules.GetPluralRule(language),
            Ptxt_week = new[] { "{0} week", "{0} weeks" },
            Ptxt_day = new[] { "{0} day", "{0} days" },
            Ptxt_hour = new[] { "{0} hour", "{0} hours" },
            Ptxt_minute = new[] { "{0} minute", "{0} minutes" },
            Ptxt_second = new[] { "{0} second", "{0} seconds" },
            Ptxt_millisecond = new[] { "{0} millisecond", "{0} milliseconds" },
            Ptxt_w = new[] { "{0}w" },
            Ptxt_d = new[] { "{0}d" },
            Ptxt_h = new[] { "{0}h" },
            Ptxt_m = new[] { "{0}m" },
            Ptxt_s = new[] { "{0}s" },
            Ptxt_ms = new[] { "{0}ms" },
            Ptxt_lessThan = "less than {0}"
        };
            
        Assert.That(() => CommonLanguagesTimeTextInfo.AddLanguage(language, custom), Throws.Nothing);
        Assert.That(() => CommonLanguagesTimeTextInfo.GetTimeTextInfo(language), Is.EqualTo(custom));
    }

    [Test]
    public void Add_Invalid_Custom_Language_TimeTextInfo_Should_Throw()
    {
        var language = "123xyz";
        Assert.That(() =>
            {
                TimeTextInfo custom = new()
                {
                    PluralRule = PluralRules.GetPluralRule(language),
                    Ptxt_week = new[] { "{0} week", "{0} weeks" },
                    Ptxt_day = new[] { "{0} day", "{0} days" },
                    Ptxt_hour = new[] { "{0} hour", "{0} hours" },
                    Ptxt_minute = new[] { "{0} minute", "{0} minutes" },
                    Ptxt_second = new[] { "{0} second", "{0} seconds" },
                    Ptxt_millisecond = new[] { "{0} millisecond", "{0} milliseconds" },
                    Ptxt_w = new[] { "{0}w" },
                    Ptxt_d = new[] { "{0}d" },
                    Ptxt_h = new[] { "{0}h" },
                    Ptxt_m = new[] { "{0}m" },
                    Ptxt_s = new[] { "{0}s" },
                    Ptxt_ms = new[] { "{0}ms" },
                    Ptxt_lessThan = "less than {0}"
                };

                CommonLanguagesTimeTextInfo.AddLanguage(language, custom);
            },
            Throws.ArgumentException.And.Message.Contains("IsoLangToDelegate not found"));
    }
}