using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core;

/// <summary>
/// Settings.ConvertCharacterStringLiterals is required, if the format string does not
/// come from code but from a file (e.g. a resource):
/// While in case of code all character literals are converted to Unicode by the compiler,
/// SmartFormat can do the conversion in case of strings coming from a file.
/// </summary>
[TestFixture]
public class LiteralTextTests
{
    [Test]
    public void FormatCharacterLiteralsAsString()
    {
        const string formatWithFileBehavior = @"No carriage return\r, no line feed\n";
        const string formatWithCodeBehavior = "With carriage return\r, and with line feed\n";

        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = false}});

        var result = formatter.Format(formatWithFileBehavior);
        Assert.That(result, Is.EqualTo(string.Format(formatWithFileBehavior)));

        result = formatter.Format(formatWithCodeBehavior);
        Assert.That(result, Is.EqualTo(string.Format(formatWithCodeBehavior)));
    }

    [Test]
    public void AllSupportedCharacterLiteralsAsUnicode()
    {
        const string formatWithFileBehavior = @"All supported literal characters: \\ \a \b \f \n \r \t \v \0 \u2022!";
        const string formatWithCodeBehavior = "All supported literal characters: \\ \a \b \f \n \r \t \v \0 \u2022!";

        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = true}});

        var result = formatter.Format(formatWithFileBehavior);
        Assert.That(result, Is.EqualTo(formatWithCodeBehavior));
    }

    [TestCase(@"Some text \u2022 with the value {0}", "Some text \u2022 with the value 123")]
    [TestCase(@"\u2015 {0}", "\u2015 123")]
    [TestCase(@"\u2010 {0} \u2015", "\u2010 123 \u2015")]
    public void UnicodeEscapeSequenceIsParsed(string format, string expectedOutput)
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = true}});
        Assert.That(formatter.Format(format, 123), Is.EqualTo(expectedOutput));
    }

    [TestCase(@"Some text {0} \uABCP", @"\uABCP")]
    [TestCase(@"Some text {0} \u-123", @"\u-123")]
    [TestCase(@"\u", @"\u")]
    [TestCase(@"\y", @"\y")]
    [TestCase(@"Some text \uuuuu {0}", @"\uuuuu")]
    public void UnsupportedCharacterLiteralEscapeSequence(string format, string exMessage)
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = true}});

        Assert.That( () => formatter.Format(format, 123), Throws.ArgumentException.And.Message.Contains(exMessage));
    }

    [Test]
    public void ConvertCharacterLiteralsToUnicodeWithListFormatter()
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = true}});
        // a useful practical test case: separate members of a list by a new line
        var items = new[] { "one", "two", "three" };
        // Note the @ before the format string will switch off conversion of \n by the compiler
        var result = smart.Format(@"{0:list:{}|\n|\nand }", new object[] { items });

        Assert.That(result, Is.EqualTo("one\ntwo\nand three"));
    }

    [Test, Description("Illegal escape sequence should always throw")]
    public void IllegalEscapeSequenceThrowsException()
    {
        var smart = new SmartFormatter().AddExtensions(new DefaultSource()).AddExtensions(new DefaultFormatter());
        Assert.That(code: () => {
            smart.Format(@"\z Illegal escape sequence at the beginning of the text");
        },Throws.ArgumentException.And.Message.Contains("escape sequence"));

        Assert.That(code: () => {
            smart.Format(@"Illegal escape sequence \z somewhere in text");
        },Throws.ArgumentException.And.Message.Contains("escape sequence"));

        Assert.That(code: () => {
            smart.Format(@"Illegal escape sequence at end of line = \z");
        }, Throws.ArgumentException.And.Message.Contains("escape sequence"));

        Assert.That(code: () => {
            smart.Format(@"Illegal escape sequence starts at end of line = \");
        },Throws.ArgumentException.And.Message.Contains("escape sequence"));
    }
}