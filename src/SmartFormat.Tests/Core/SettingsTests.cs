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
        Assert.That(() => settings.Parser.AddCustomSelectorChars([ParserSettings.PlaceholderBeginChar]),
            Throws.ArgumentException.And.Message.Contains($"{ParserSettings.PlaceholderBeginChar}"));
    }

    [Test]
    public void ExistingSelectorCharacter_Should_Not_Be_Added()
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomSelectorChars(['A', ' ']);
        Assert.Multiple(() =>
        {
            Assert.That(settings.Parser.CustomSelectorChars.Count(c => c == 'A'), Is.EqualTo(0));
            Assert.That(settings.Parser.CustomSelectorChars.Count(c => c == ' '), Is.EqualTo(1));
        });
    }

    [TestCase(FilterType.Allowlist)]
    [TestCase(FilterType.Blocklist)]
    public void ControlCharacters_Should_Be_Added_As_SelectorChars(FilterType filterType)
    {
        var settings = new SmartSettings { Parser = { SelectorCharFilter = filterType } };
        var controlChars = ParserSettings.ControlChars().ToList();
        settings.Parser.AddCustomSelectorChars(controlChars);
        
        Assert.Multiple(() =>
        {
            Assert.That(settings.Parser.CustomSelectorChars, Has.Count.EqualTo(controlChars.Count));
            foreach (var c in settings.Parser.CustomSelectorChars)
            {
                Assert.That(settings.Parser.GetSelectorChars(), filterType == FilterType.Allowlist ? Does.Contain(c) : Does.Not.Contain(c),
                    $"Control char U+{(int) c:X4} should be allowed as selector char.");
            }
        });
    }

    [Test]
    public void TryingToAddDisallowedOperatorCharacters_Should_Throw()
    {
        var settings = new SmartSettings();
        Assert.That(() => settings.Parser.AddCustomOperatorChars([ParserSettings.PlaceholderBeginChar]),
            Throws.ArgumentException.And.Message.Contains($"{ParserSettings.PlaceholderBeginChar}"));
    }

    [Test]
    public void ExistingOperatorCharacter_Should_Not_Be_Added()
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomOperatorChars([ParserSettings.OperatorChars[0], '°']);
        settings.Parser.AddCustomOperatorChars(['°']);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Parser.CustomOperatorChars.Count(c => c == ParserSettings.OperatorChars[0]), Is.EqualTo(0));
            Assert.That(settings.Parser.CustomOperatorChars.Count(c => c == '°'), Is.EqualTo(1));
        });
    }

    [TestCase('{')]
    [TestCase('}')]
    [TestCase(':')]
    [TestCase('(')]
    [TestCase(')')]
    public void Add_Separators_As_Custom_Operator_Should_Throw(char operatorChar)
    {
        var settings = new SmartSettings();

        // try to add the same char as operator
        Assert.That(() => settings.Parser.AddCustomOperatorChars([operatorChar]),
            Throws.ArgumentException.And.Message.Contains($"'{operatorChar}'"));
    }

    [TestCase('°')] // a custom selector char
    [TestCase('.')] // a standard operator char
    public void Add_CustomSelector_Used_As_Operator_Should_Throw(char selectorChar)
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomOperatorChars([selectorChar]); // reserve as operator char

        // try to add the same char as selector
        Assert.That(() => settings.Parser.AddCustomSelectorChars([selectorChar]),
            Throws.ArgumentException.And.Message.Contains($"{selectorChar}"));
    }

    [TestCase((char) 127)] // a custom char
    [TestCase((char) 30)] // a standard operator char
    public void Add_CustomOperator_Used_As_Selector_Should_Throw(char operatorChar)
    {
        var settings = new SmartSettings();
        settings.Parser.AddCustomSelectorChars([operatorChar]); // reserve as operator char

        // try to add the same char as selector
        Assert.That(() => settings.Parser.AddCustomOperatorChars([operatorChar]),
            Throws.ArgumentException.And.Message.Contains($"{operatorChar}"));
    }
}
