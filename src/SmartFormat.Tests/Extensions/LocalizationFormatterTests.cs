using System;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.Localization;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class LocalizationFormatterTests
    {
        private static SmartFormatter GetFormatterWithRegisteredResource(CaseSensitivityType caseSensitivity = CaseSensitivityType.CaseSensitive, FormatErrorAction formatErrorAction = FormatErrorAction.ThrowError)
        {
            var localizationFormatter = new LocalizationFormatter {CanAutoDetect = false};
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            {
                CaseSensitivity = caseSensitivity,
                Localization =
                {
                    LocalizationProvider = new LocalizationProvider(true, LocTest1.ResourceManager)
                        { FallbackCulture = null, ReturnNameIfNotFound = false }
                },
                Formatter = { ErrorAction = formatErrorAction }
            });
            smart.AddExtensions(localizationFormatter);
            smart.AddExtensions(1, new PluralLocalizationFormatter());

            return smart;
        }

        [Test]
        public void Missing_Format_Should_Throw()
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            Assert.That(() => smart.Format("{:L:}"), Throws.InstanceOf<LocalizationFormattingException>().With.InnerException.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Unknown_Culture_Should_Throw()
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            Assert.That(() => smart.Format("{:L(unknown):dummy}"), Throws.InstanceOf<LocalizationFormattingException>());
        }

        [Test]
        public void No_Initialization_Of_LocalizationProvider_Should_Throw()
        {
            var smart = GetFormatterWithRegisteredResource();
            var formatter = smart.GetFormatterExtension<LocalizationFormatter>();
            formatter!.LocalizationProvider = null;
            Assert.That(() => smart.Format("{:L(en):dummy}"), Throws.InstanceOf<LocalizationFormattingException>().With.InnerException.InstanceOf<NullReferenceException>());
        }

        [TestCase(FormatErrorAction.Ignore)]
        [TestCase(FormatErrorAction.MaintainTokens)]
        [TestCase(FormatErrorAction.OutputErrorInResult)]
        [TestCase(FormatErrorAction.ThrowError)]
        public void No_Localized_String_Found(FormatErrorAction errorAction)
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive, errorAction);
            string? result;

            switch (errorAction)
            {
                case FormatErrorAction.ThrowError:
                    Assert.That(() => smart.Format("{:L(es):NonExisting}"), Throws.InstanceOf<FormattingException>());
                    break;
                case FormatErrorAction.OutputErrorInResult:
                    result = smart.Format("{:L(es):NonExisting}");
                    Assert.That(result, Contains.Substring("No localized string found"));
                    break;
                case FormatErrorAction.Ignore:
                    result = smart.Format("{:L(es):NonExisting}");
                    Assert.That(result, Is.EqualTo(string.Empty));
                    break;
                case FormatErrorAction.MaintainTokens:
                    result = smart.Format("{:L(es):NonExisting}");
                    Assert.That(result, Is.EqualTo("{:L(es):NonExisting}"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(errorAction), errorAction, null);
            }
        }

        [Test]
        public void No_Localized_String_Found_With_Name_Fallback()
        {
            var smart = GetFormatterWithRegisteredResource();
            ((LocalizationProvider)smart.GetFormatterExtension<LocalizationFormatter>()!.LocalizationProvider!)
                .ReturnNameIfNotFound = true;
            var actual = smart.Format("{:L(es):NonExisting}");
            Assert.That(actual, Is.EqualTo("NonExisting"));
        }

        [Test]
        public void No_Localized_String_Only_In_Fallback_Culture()
        {
            var smart = GetFormatterWithRegisteredResource();
            var actual = smart.Format(CultureInfo.GetCultureInfo("pt"), "{:L:OnlyExistForInvariantCulture}");
            Assert.That(actual, Is.EqualTo("This entry only exists in the invariant culture resource"));
        }

        [Test]
        public void Should_Use_Existing_Localized_Format()
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            var locFormatter = smart.GetFormatterExtension<LocalizationFormatter>();
            _ = smart.Format("{:L(es):WeTranslateText}");
            var result = smart.Format("{:L(es):WeTranslateText}");

            Assert.That(locFormatter!.LocalizedFormatCache!.Keys.Contains(result), Is.True);
        }

        [TestCase("{:L():WeTranslateText}", "Traducimos el texto", "es")]
        [TestCase("{:L:WeTranslateText}", "Traducimos el texto", "es")]
        [TestCase("{:L():WeTranslateText}", "We translate text", "")]
        [TestCase("{:L:WeTranslateText}", "We translate text", "")]
        public void Pure_Text_CurrentCulture(string format, string expected, string culture)
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            CultureInfo.CurrentUICulture = culture == string.Empty ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);

            var actual = smart.Format(format);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("{:L():WeTranslateText}", "Traducimos el texto", "es")]
        [TestCase("{:L:WeTranslateText}", "Traducimos el texto", "es")]
        [TestCase("{:L():WeTranslateText}", "We translate text", "")]
        [TestCase("{:L:WeTranslateText}", "We translate text", "")]
        public void Pure_Text_CultureByArgument(string format, string expected, string cultureString)
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            var culture = cultureString == string.Empty ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureString);

            var actual = smart.Format(culture, format);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("{:L(es):WeTranslateText}", "Traducimos el texto")]
        [TestCase("{:L(en):WeTranslateText}", "We translate text")]
        [TestCase("{:L(fr):WeTranslateText}", "Nous traduisons des textes")]
        [TestCase("{:L(de):WeTranslateText}", "Wir übersetzen Text")]
        public void Pure_Text_CultureByFormatString(string format, string expected)
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);

            var actual = smart.Format(format);
            Assert.That(actual, Is.EqualTo(expected));
        }

        // Possible, but not recommended
        [TestCase("{:L(es):{0} has {1:#,#} inhabitants}", "{1} tiene {2:#,#} habitantes", "es")]
        [TestCase("{:L(en):{0} has {1:#,#} inhabitants}", "{1} has {2:#,#} inhabitants", "en")]
        [TestCase("{:L(fr):{0} has {1:#,#} inhabitants}", "{1} compte {2:#,#} habitants", "fr")]
        [TestCase("{:L(de):{0} has {1:#,#} inhabitants}", "{1} hat {2:#,#} Einwohner", "de")]
        // Best practice, because the selector is not part of the format to localize ({:#,#} applies for any selector)
        [TestCase("{0} {1:L(es):has {:#,#} inhabitants}", "{1} tiene {2:#,#} habitantes", "es")]
        [TestCase("{0} {1:L(en):has {:#,#} inhabitants}", "{1} has {2:#,#} inhabitants", "en")]
        [TestCase("{0} {1:L(fr):has {:#,#} inhabitants}", "{1} compte {2:#,#} habitants", "fr")]
        [TestCase("{0} {1:L(de):has {:#,#} inhabitants}", "{1} hat {2:#,#} Einwohner", "de")]
        // Best practice (same localization as above, with different selector)
        [TestCase("{2.City.Name} {2.City.Inhabitants:L(es):has {:#,#} inhabitants}", "{1} tiene {2:#,#} habitantes", "es")]
        [TestCase("{2.City.Name} {2.City.Inhabitants:L(en):has {:#,#} inhabitants}", "{1} has {2:#,#} inhabitants", "en")]
        [TestCase("{2.City.Name} {2.City.Inhabitants:L(fr):has {:#,#} inhabitants}", "{1} compte {2:#,#} habitants", "fr")]
        [TestCase("{2.City.Name} {2.City.Inhabitants:L(de):has {:#,#} inhabitants}", "{1} hat {2:#,#} Einwohner", "de")]
        public void TextWithPlaceholder_CultureByFormatString(string format, string expected, string culture)
        {
            var smart = GetFormatterWithRegisteredResource(CaseSensitivityType.CaseSensitive);
            // number should be localized
            expected = string.Format(CultureInfo.GetCultureInfo(culture), expected, new {City = new { Name = "X-City", Inhabitants = 8900000}}, "X-City", 8900000);

            var actual = smart.Format(format, "X-City", 8900000, new {City = new { Name = "X-City", Inhabitants = 8900000}});
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 0, "en", "0 items")]
        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 1, "en", "1 item")]
        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 200, "en", "200 items")]
        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 200, "es", "200 elementos")]
        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 200, "fr", "200 éléments")]
        [TestCase("{0:cond:{:L:{} items}|{:L:{} item}|{:L:{} items}}", 200, "de", "200 Elemente")]
        public void Combine_With_ConditionalFormatter(string format, int count, string cultureName, string expected)
        {
            // Just for demo - PluralLocalizationFormatter is the best choice for pluralization
            // zero and two hundred: plural, one: singular
            var smart = GetFormatterWithRegisteredResource();
            var actual = smart.Format(CultureInfo.GetCultureInfo(cultureName), format, count);
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 0, "en", "0 items")]
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 1, "en", "1 item")]
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 200, "de", "200 Elemente")]
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 0, "fr", "0 élément")]
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 1, "fr", "1 élément")]
        [TestCase("{0:plural:{:L:{} item}|{:L:{} items}}", 200, "fr", "200 éléments")]
        public void Combine_With_PluralLocalizationFormatter(string format, int count, string cultureName, string expected)
        {
            var smart = GetFormatterWithRegisteredResource();
            var actual = smart.Format(CultureInfo.GetCultureInfo(cultureName), format, count);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}