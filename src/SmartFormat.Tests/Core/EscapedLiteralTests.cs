using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Tests.Core
{
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

        [TestCase(@"\\ \' abc\\", @"\ ' abc\", false)] // included in look-up table
        [TestCase(@"\zabc\", @"\z", true)] // not included in look-up table
        public void UnEscapeCharLiterals_General_Test(string input, string expected, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.That( () => EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(),true), Throws.ArgumentException.And.Message.Contains(expected));
            }
            else
            {
                var result = EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(),false);
                Assert.That(result.ToString(), Is.EqualTo(expected));            }
        }

        [TestCase(@"\{ \( abc", @"{ ( abc", false)] // included in look-up table
        [TestCase(@"\zabc", @"\z", true)] // not included in look-up table
        public void UnEscapeCharLiterals_FormatterOption_Test(string input, string expected, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.That( () => EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(),true), Throws.ArgumentException.And.Message.Contains(expected));
            }
            else
            {
                var result = EscapedLiteral.UnEscapeCharLiterals('\\', input.AsSpan(),true);
                Assert.That(result.ToString(), Is.EqualTo(expected));
            }
        }

        [TestCase(@"abc", @"abc")] // not to escape
        [TestCase("\'\"\\\n", @"\'\""\\\n")] // to escape
        public void EscapeCharLiterals_General_Test(string input, string expected)
        {
            var result = EscapedLiteral.EscapeCharLiterals('\\', input, false).ToString();
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(@"abc", @"abc")] // not to escape
        [TestCase("(){}:", @"\(\)\{\}\:")] // to escape
        public void EscapeCharLiterals_FormatOption_Test(string input, string expected)
        {
            var result = EscapedLiteral.EscapeCharLiterals('\\', input, true).ToString();
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(@"\(([^\)]*)\)")] // escaped parenthesis
        [TestCase(@"Lon(?=don)")] // parenthesis
        [TestCase(@"<[^<>]+>")] // square and pointed brackets
        [TestCase(@"\d{3,}")] // curly braces
        [TestCase(@"^.{5,}:,$")] // dot, colon, comma
        public void UnEscape_Escaped_Special_Characters(string pattern)
        {
            var optionsEscaped = new string(EscapedLiteral.EscapeCharLiterals('\\', pattern, true).ToArray());
            Assert.That(EscapedLiteral.UnEscapeCharLiterals('\\', optionsEscaped.AsSpan(), true).ToString(), Is.EqualTo(pattern));
        }
    }
}
