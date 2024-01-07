using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class EscapedLiteralTests
{
    [TestCase('\\', '\\')] // included in look-up table
    [TestCase('9', '\0')] // not included in look-up table
    public void TryGetChar_General_Test(char testChar, char testCharResult)
    {
        var found = EscapedLiteral.TryGetChar(testChar, out var result, false);
        if (found) Assert.That(result, Is.EqualTo(testCharResult));
        else Assert.That(found, Is.False);
    }

    [TestCase('{', '{')] // included in look-up table
    [TestCase('9', '\0')] // not included in look-up table
    public void TryGetChar_FormatterOption_Test(char testChar, char testCharResult)
    {
        var found = EscapedLiteral.TryGetChar(testChar, out var result, true);
        if (found) Assert.That(result, Is.EqualTo(testCharResult));
        else Assert.That(found, Is.False);
    }

    [TestCase(@"\\ \{ \} abc\\", @"\ { } abc\", false)] // included in look-up table
    [TestCase(@"\zabc\", @"\z", true)] // not included in look-up table
    public void UnEscapeCharLiterals_General_Test(string input, string expected, bool shouldThrow)
    {
        var resultBuffer = new Span<char>(new char[input.Length]);
        if (shouldThrow)
        {
            try
            {
                EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(0, input.Length), false, resultBuffer);
                Assert.Fail("Failure expected.");
            }
            catch (Exception e)
            {
                Assert.That( () => throw e, Throws.ArgumentException.And.Message.Contains(expected));
            }
        }
        else
        {
            var result = EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(0, input.Length), false, resultBuffer);
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }

    [TestCase(@"\{ \( abc", @"{ ( abc", false)] // included in look-up table
    [TestCase(@"\zabc", @"\z", true)] // not included in look-up table
    public void UnEscapeCharLiterals_FormatterOption_Test(string input, string expected, bool shouldThrow)
    {
        var resultBuffer = new Span<char>(new char[input.Length]);
        if (shouldThrow)
        {
            try
            {
                EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(0, input.Length), true, resultBuffer);
                Assert.Fail("Failure expected.");
            }
            catch (Exception e)
            {
                Assert.That( () => throw e, Throws.ArgumentException.And.Message.Contains(expected));
            }
        }
        else
        {
            var result = EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(0, input.Length), true, resultBuffer);
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }

    [TestCase(@"abc", @"abc")] // not to escape
    //[TestCase("\'\"\\\n", @"\'\""\\\n")] // to escape
    [TestCase("{}\\\n", @"\{\}\\\n")] // to escape
    public void EscapeCharLiterals_General_Test(string input, string expected)
    {
        var result = new string(EscapedLiteral.EscapeCharLiterals('\\', input, 0, input.Length, false).ToArray());
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(@"abc", @"abc")] // not to escape
    [TestCase("(){}:", @"\(\)\{\}\:")] // to escape
    public void EscapeCharLiterals_FormatOption_Test(string input, string expected)
    {
        var result = new string(EscapedLiteral.EscapeCharLiterals('\\', input, 0, input.Length, true).ToArray());
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(@"\(([^\)]*)\)")] // escaped parenthesis
    [TestCase(@"Lon(?=don)")] // parenthesis
    [TestCase(@"<[^<>]+>")] // square and pointed brackets
    [TestCase(@"\d{3,}")] // curly braces
    [TestCase(@"^.{5,}:,$")] // dot, colon, comma
    public void UnEscape_Escaped_Special_Characters(string pattern)
    {
        var resultBuffer = new Span<char>(new char[pattern.Length]);
        var optionsEscaped = new string(EscapedLiteral.EscapeCharLiterals('\\', pattern, 0, pattern.Length, true).ToArray());
        Assert.That(EscapedLiteral.UnEscapeCharLiterals('\\', optionsEscaped.AsSpan(0, optionsEscaped.Length), true, resultBuffer).ToString(), Is.EqualTo(pattern));
    }

    [Test]
    public void UnEscape_With_StartIndex_not_zero()
    {
        var full = "abc(de";
        var startIndex = 3;
        var resultBuffer = new Span<char>(new char[full.Length]);
        Assert.That(EscapedLiteral.UnEscapeCharLiterals('\\', full.AsSpan(startIndex, full.Length - startIndex), true, resultBuffer).ToString(), Is.EqualTo("(de"));
    }
}
