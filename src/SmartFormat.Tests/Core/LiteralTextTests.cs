using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SmartFormat.Tests.Core
{
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

            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ConvertCharacterStringLiterals = false;

            var result = formatter.Format(formatWithFileBehavior);
            Assert.AreEqual(string.Format(formatWithFileBehavior), result);

            result = formatter.Format(formatWithCodeBehavior);
            Assert.AreEqual(string.Format(formatWithCodeBehavior), result);
        }

        [Test]
        public void AllSupportedCharacterLiteralsAsUnicode()
        {
            const string formatWithFileBehavior = @"All supported literal characters: \' \"" \\ \a \b \f \n \r \t \v \0!";
            const string formatWithCodeBehavior = "All supported literal characters: \' \" \\ \a \b \f \n \r \t \v \0!";
            
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ConvertCharacterStringLiterals = true;

            var result = formatter.Format(formatWithFileBehavior);
            Assert.AreEqual(formatWithCodeBehavior, result);
        }

        [Test]
        public void UnsupportedCharacterLiteralEscapeSequence()
        {
            const string format = @"Not supported: \y - Supported: \a";
            
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ConvertCharacterStringLiterals = true;

            Assert.That( () => formatter.Format(format), Throws.ArgumentException.And.Message.Contains(@"\y"));
        }

        [TestCase(@"Some text \u2022 with the value {0}", "Some text \u2022 with the value 123")]
        [TestCase(@"\u2015 {0}", "\u2015 123")]
        [TestCase(@"\u2010 {0} \u2015", "\u2010 123 \u2015")]
        public void UnicodeEscapeSequenceIsParsed(string format, string expectedOutput)
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.ConvertCharacterStringLiterals = true;
            Assert.AreEqual(expectedOutput, formatter.Format(format, 123));
        }

        [TestCase(@"Some text {0} \uABCP")]
        [TestCase(@"Some text {0} \u-123")]
        [TestCase(@"\u")]
        [TestCase(@"Some text \uuuuu {0}")]
        public void InvalidUnicodeEscapeSequenceThrowsException(string text)
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.ConvertCharacterStringLiterals = true;
            Assert.Throws<ArgumentException>(() => formatter.Format(text, 123));
        }

        [Test]
        public void ConvertCharacterLiteralsToUnicodeWithListFormatter()
        {
            // a useful practical test case: separate members of a list by a new line
            var items = new[] { "one", "two", "three" };
            // Note the @ before the format string will switch off conversion of \n by the compiler
            Smart.Default.Settings.Parser.ConvertCharacterStringLiterals = true;
            var result = Smart.Default.Format(@"{0:list:{}|\n|\nand }", new object[] { items });
            Smart.Default.Settings.Parser.ConvertCharacterStringLiterals = false;
            Assert.AreEqual("one\ntwo\nand three", result);
        }
    }
}
