using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class SettingsTests
{
    [Test]
    public void TryingToAddDisallowedSelectorCharacters_Should_Throw()
    {
        var settings = new SmartSettings();
        Assert.That(() => settings.Parser.AddCustomSelectorChars(new[] {settings.Parser.PlaceholderBeginChar}),
            Throws.ArgumentException.And.Message.Contains($"{settings.Parser.PlaceholderBeginChar}"));
    }

    [Test]
    public void ExistingSelectorCharacter_Should_Not_Be_Added()
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomSelectorChars(new[] {'A', ' '});
        settings.Parser.AddCustomSelectorChars(new[] {' '});
        Assert.Multiple(() =>
        {
            Assert.That(settings.Parser.CustomSelectorChars().Count(c => c == 'A'), Is.EqualTo(0));
            Assert.That(settings.Parser.CustomSelectorChars().Count(c => c == ' '), Is.EqualTo(1));
        });
    }

    [Test]
    public void TryingToAddDisallowedOperatorCharacters_Should_Throw()
    {
        var settings = new SmartSettings();
        Assert.That(() => settings.Parser.AddCustomOperatorChars(new[] {settings.Parser.PlaceholderBeginChar}),
            Throws.ArgumentException.And.Message.Contains($"{settings.Parser.PlaceholderBeginChar}"));
    }

    [Test]
    public void ExistingOperatorCharacter_Should_Not_Be_Added()
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomOperatorChars(new[] {settings.Parser.OperatorChars()[0], '°'});
        settings.Parser.AddCustomOperatorChars(new[] {'°'});

        Assert.Multiple(() =>
        {
            Assert.That(settings.Parser.CustomOperatorChars().Count(c => c == settings.Parser.OperatorChars()[0]), Is.EqualTo(0));
            Assert.That(settings.Parser.CustomOperatorChars().Count(c => c == '°'), Is.EqualTo(1));
        });
    }

    [TestCase('°')] // a custom char
    [TestCase('A')] // a standard selector char
    public void Add_CustomOperator_Used_As_Separator_Should_Throw(char operatorChar)
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomSelectorChars(new[] {operatorChar}); // reserve as selector char

        // try to add the same char as operator
        Assert.That(() => settings.Parser.AddCustomOperatorChars(new[] {operatorChar}),
            Throws.ArgumentException.And.Message.Contains($"{operatorChar}"));
    }

    [TestCase('°')] // a custom char
    [TestCase('.')] // a standard operator char
    public void Add_CustomSelector_Used_As_Operator_Should_Throw(char selectorChar)
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomOperatorChars(new[] {selectorChar}); // reserve as operator char

        // try to add the same char as selector
        Assert.That(() => settings.Parser.AddCustomSelectorChars(new[] {selectorChar}),
            Throws.ArgumentException.And.Message.Contains($"{selectorChar}"));
    }
}
