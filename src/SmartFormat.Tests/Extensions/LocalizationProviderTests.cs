using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.Localization;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class LocalizationProviderTests
{
    private static LocalizationProvider GetInitializedProvider()
    {
        return new LocalizationProvider(true, LocTest1.ResourceManager);
    }

    [Test]
    public void Resources_Should_Be_Added_Exactly_Once()
    {
        var provider = GetInitializedProvider();
        provider.AddResource(LocTest1.ResourceManager);
        // only one of the same resource should be added
        provider.AddResource(LocTest1.ResourceManager);
        Assert.That(provider.Resources, Has.Count.EqualTo(1));
    }

    [Test]
    public void Remove_Resource()
    {
        var provider = GetInitializedProvider();

        var success1 = provider.Remove("does-not-exist");
        var success2 = provider.Remove(provider.Resources.First().Value.BaseName);

        Assert.Multiple(() =>
        {
            Assert.That(success1, Is.False);
            Assert.That(success2, Is.True);
            Assert.That(provider.Resources, Is.Empty);
        });
    }

    [Test]
    public void Clear_All_Resources()
    {
        var provider = GetInitializedProvider();
            
        provider.Clear();

        Assert.That(provider.Resources, Is.Empty);
    }

    [Test]
    public void GetString_Without_Culture()
    {
        var provider = GetInitializedProvider();
            
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        // uses the caller's UI culture
        var result = provider.GetString(nameof(LocTest1.WeTranslateText));
        LocTest1.Culture = CultureInfo.InvariantCulture;
        Assert.That(result, Is.EqualTo(LocTest1.WeTranslateText));
    }

    [Test]
    public void GetString_With_Culture()
    {
        var provider = GetInitializedProvider();
            
        var result = provider.GetString(nameof(LocTest1.WeTranslateText), CultureInfo.GetCultureInfo("es"));
        LocTest1.Culture = CultureInfo.GetCultureInfo("es");
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(LocTest1.WeTranslateText));
            Assert.That(
                provider.Resources.First().Value
                    .GetString(nameof(LocTest1.WeTranslateText), CultureInfo.GetCultureInfo("es")), Is.EqualTo("Traducimos el texto"));
        });
        Assert.That(result, Is.EqualTo("Traducimos el texto"));
    }

    [Test]
    public void GetString_With_CultureName()
    {
        var provider = GetInitializedProvider();
            
        var result = provider.GetString(nameof(LocTest1.WeTranslateText), "es");
        LocTest1.Culture = CultureInfo.GetCultureInfo("es");
        Assert.That(result, Is.EqualTo(LocTest1.WeTranslateText));
        Assert.That(result, Is.EqualTo("Traducimos el texto"));
    }

    [TestCase("Jack", "en", "Jack")]
    [TestCase("Jack", "fr", "Jean")]
    [TestCase("Jack", "es", "Juan")]
    [TestCase("Jack", "de", "Hans")]
    public void GetString_2_Resources(string input, string cultureName, string expected)
    {
        // Gets the name similar to "Jack" in other languages
        var provider = GetInitializedProvider();
        // Both (initial and LocTest2) resources should be used to find the localized string
        provider.AddResource(LocTest2.ResourceManager);
            
        var result = provider.GetString(input, cultureName);
        LocTest2.Culture = CultureInfo.GetCultureInfo(cultureName);
        Assert.Multiple(() =>
        {
            Assert.That(provider.Resources, Has.Count.EqualTo(2));
            Assert.That(result, Is.EqualTo(LocTest2.Jack));
        });
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetString_NonExisting_Value()
    {
        var provider = GetInitializedProvider();
            
        var result = provider.GetString("does-not-exist", CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }

    #region * Custom ILocalizationProvider Implementation *

    [Test]
    public void CustomLocalizationProvider()
    {
        // Important: Register the ILocalizationProvider in SmartSettings
        // before creating the LocalizationFormatter
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            { Localization = { LocalizationProvider = new DictLocalizationProvider() } });
        smart.AddExtensions(new LocalizationFormatter());

        // Only the word "COUNTRY" is translated into 3 languages
        var result = smart.Format("{:L(en):COUNTRY} * {:L(fr):COUNTRY} * {:L(es):COUNTRY}");
        Assert.That(result, Is.EqualTo("country * pays * país"));
    }

    private class DictLocalizationProvider : ILocalizationProvider
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations;

        public DictLocalizationProvider()
        {
            _translations = new Dictionary<string, Dictionary<string, string>> {
                { "en", new Dictionary<string, string> { { "COUNTRY", "country" } } },
                { "fr", new Dictionary<string, string> { { "COUNTRY", "pays" } } },
                { "es", new Dictionary<string, string> { { "COUNTRY", "país" } } }
            };
        }

        public string? GetString(string name)
        {
            return GetTranslation(name, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        public string? GetString(string name, string cultureName)
        {
            return GetTranslation(name, cultureName);
        }

        public string? GetString(string name, CultureInfo cultureInfo)
        {
            return GetTranslation(name, cultureInfo.TwoLetterISOLanguageName);
        }

        private string? GetTranslation(string name, string cultureName)
        {
            if (!_translations.TryGetValue(cultureName, out var entry)) return null;
            return entry.TryGetValue(name, out var localized) ? localized : null;
        }
    }

    #endregion
}