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

        [TestCase(@"\\ \' abc\\", @"\ ' abc\")] // included in look-up table
        [TestCase(@"\zabc\", @"\zabc\")] // not included in look-up table
        public void UnescapeCharLiterals_General_Test(string input, string expected)
        {
            var result = EscapedLiteral.UnescapeCharLiterals('\\', input.AsSpan(),false);
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }

        [TestCase(@"\{ \( abc", @"{ ( abc")] // included in look-up table
        [TestCase(@"\zabc", @"\zabc")] // not included in look-up table
        public void UnescapeCharLiterals_FormatterOption_Test(string input, string expected)
        {
            var result = EscapedLiteral.UnescapeCharLiterals('\\', input.AsSpan(),true);
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }
}
