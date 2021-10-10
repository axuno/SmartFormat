using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils.Localization;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class LocalizationProviderTests
    {
        private static LocalizationProvider GetInitializedProvider()
        {
            return new LocalizationProvider();
        }

        [Test]
        public void Resources_Should_Be_Added_Exactly_Once()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            // only one of the same resource should be added
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));

            Assert.That(provider.Resources.Count, Is.EqualTo(1));
        }

        [Test]
        public void Remove_Resource()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));

            var success1 = provider.Remove("does-not-exist");
            var success2 = provider.Remove(provider.Resources.First().Value.BaseName);

            Assert.That(success1, Is.False);
            Assert.That(success2, Is.True);
            Assert.That(provider.Resources.Count, Is.EqualTo(0));
        }

        [Test]
        public void Clear_All_Resources()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
            provider.Clear();

            Assert.That(provider.Resources.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetString_Without_Culture()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
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
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
            var result = provider.GetString(nameof(LocTest1.WeTranslateText), CultureInfo.GetCultureInfo("es"));
            LocTest1.Culture = CultureInfo.GetCultureInfo("es");
            Assert.That(result, Is.EqualTo(LocTest1.WeTranslateText));
        }

        [Test]
        public void GetString_With_CultureName()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
            var result = provider.GetString(nameof(LocTest1.WeTranslateText), "es");
            LocTest1.Culture = CultureInfo.GetCultureInfo("es");
            Assert.That(result, Is.EqualTo(LocTest1.WeTranslateText));
        }

        [TestCase("Jack", "en")]
        [TestCase("Jack", "fr")]
        [TestCase("Jack", "es")]
        [TestCase("Jack", "de")]
        public void GetString_2_Resources(string input, string cultureName)
        {
            // Gets the name similar to "Jack" in other languages
            var provider = GetInitializedProvider();
            // Both resources should be used to find the localized string
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest2).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
            var result = provider.GetString(input, cultureName);
            Console.WriteLine(cultureName + ": " + result);
            LocTest2.Culture = CultureInfo.GetCultureInfo(cultureName);
            Assert.That(provider.Resources.Count, Is.EqualTo(2));
            Assert.That(result, Is.EqualTo(LocTest2.Jack));
        }

        [Test]
        public void GetString_NonExisting_Value()
        {
            var provider = GetInitializedProvider();
            provider.AddResource(new System.Resources.ResourceManager(
                typeof(LocTest1).FullName!,
                typeof(LocalizationFormatterTests).Assembly));
            
            var result = provider.GetString("does-not-exist", CultureInfo.InvariantCulture);
            Assert.That(result, Is.Null);
        }
    }
}