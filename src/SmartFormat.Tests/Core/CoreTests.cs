using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Core.Parsing;
using SmartFormat.Tests.Common;
using SmartFormat.Utilities;
using FormatException = SmartFormat.Core.FormatException;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestParser()
        {
            var parser = new Parser(ErrorAction.ThrowError);
            parser.AddAlphanumericSelectors();
            parser.AddAdditionalSelectorChars("_");
            parser.AddOperators(".");

            var formats = new[]{
                " aaa {bbb.ccc: ddd {eee} fff } ggg ",
                "{aaa} {bbb}",
                "{}",
                "{a:{b:{c:{d} } } }",
                "{a}",
                " aaa {bbb_bbb.CCC} ddd ",
            };
            var results = formats.Select(f => new { format = f, parsed = parser.ParseFormat(f) }).ToArray();

            // Verify that the reconstructed formats
            // match the original ones:

            results.TryAll(r => Assert.AreEqual(r.format, r.parsed.ToString())).ThrowIfNotEmpty();
        }

        [Test]
        public void Parser_Throws_Exceptions()
        {
            // Let's set the "ErrorAction" to "Throw":
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Parser.ErrorAction = ErrorAction.ThrowError;

            var args = new object[] { TestFactory.GetPerson() };
            var invalidFormats = new[] {
                "{",
                "{0",
                "}",
                "0}",
                "{{{",
                "}}}",
                "{.}",
                "{.:}",
                "{..}",
                "{..:}",
                "{0.}",
                "{0.:}",
            };
            foreach (var format in invalidFormats)
            {
                try
                {
                    formatter.Test(format, args, "Error");
                    // Make sure that EVERY item has an error:
                    Assert.Fail("Parsing \"{0}\" should have failed but did not.", format);
                }
                catch (ParsingErrors ex)
                {
                }
            }

        }

        [Test]
        public void Parser_Ignores_Exceptions()
        {
            var parser = new Parser(ErrorAction.Ignore);
            var invalidFormats = new[] {
                "{",
                "{0",
                "}",
                "0}",
                "{{{",
                "}}}",
                "{.}",
                "{.:}",
                "{..}",
                "{..:}",
                "{0.}",
                "{0.:}",
            };
            foreach (var format in invalidFormats)
            {
                var discard = parser.ParseFormat(format);
            }
        }



        private static Parser GetRegularParser()
        {
            var parser = new Parser(ErrorAction.ThrowError);
            parser.AddAlphanumericSelectors();
            parser.AddOperators(".");
            return parser;
        }
        
        [Test]
        public void Test_Format_Substring()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";

            var Format = parser.ParseFormat(format);

            // Extract the substrings of literal text:
            Assert.That(Format.Substring( 1, 3).ToString(), Is.EqualTo("a|a"));
            Assert.That(Format.Substring(41, 4).ToString(), Is.EqualTo("gg|g"));

            // Extract a substring that overlaps into the placeholder:
            Assert.That(Format.Substring(4, 3).ToString(), Is.EqualTo("a {bbb: ccc dd|d {:|||} {eee} ff|f }"));
            Assert.That(Format.Substring(20, 23).ToString(), Is.EqualTo("{bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            
            // Make sure a invalid values are caught:
            Assert.That(() => Format.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Format.Substring(100), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Format.Substring(10, 100), Throws.TypeOf<ArgumentOutOfRangeException>());


            // Now, test nested format strings:
            var placeholder = (Placeholder)Format.Items[1];
            Format = placeholder.Format;
            Assert.That(Format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(Format.Substring(5, 4).ToString(), Is.EqualTo("dd|d"));
            Assert.That(Format.Substring(8, 3).ToString(), Is.EqualTo("d {:|||}"));
            Assert.That(Format.Substring(8, 10).ToString(), Is.EqualTo("d {:|||} {eee}"));
            Assert.That(Format.Substring(8, 16).ToString(), Is.EqualTo("d {:|||} {eee} f"));

            // Make sure invalid values are caught:
            Assert.That(() => Format.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Format.Substring(30), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Format.Substring(25, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
            
        }

        [Test]
        public void Test_Format_IndexOf()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var Format = parser.ParseFormat(format);

            Assert.That(Format.IndexOf("|"), Is.EqualTo(2));
            Assert.That(Format.IndexOf("|", 3), Is.EqualTo(43));
            Assert.That(Format.IndexOf("|", 44), Is.EqualTo(-1));
            Assert.That(Format.IndexOf("#"), Is.EqualTo(-1));
            Assert.That(Format.IndexOf("eee"), Is.EqualTo(-1));

            // Test nested formats:
            var placeholder = (Placeholder) Format.Items[1];
            Format = placeholder.Format;
            Assert.That(Format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(Format.IndexOf("|"), Is.EqualTo(7));
            Assert.That(Format.IndexOf("|", 8), Is.EqualTo(25));
            Assert.That(Format.IndexOf("|", 26), Is.EqualTo(-1));
            Assert.That(Format.IndexOf("#"), Is.EqualTo(-1));
            Assert.That(Format.IndexOf("eee"), Is.EqualTo(-1));
        }

        [Test]
        public void Test_Format_Split()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var Format = parser.ParseFormat(format);
            var splits = Format.Split("|");

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" a"));
            Assert.That(splits[1].ToString(), Is.EqualTo("aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            Assert.That(splits[2].ToString(), Is.EqualTo("g "));

            // Test nested formats:
            var placeholder = (Placeholder) Format.Items[1];
            Format = placeholder.Format;
            Assert.That(Format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));
            splits = Format.Split("|");

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" ccc dd"));
            Assert.That(splits[1].ToString(), Is.EqualTo("d {:|||} {eee} ff"));
            Assert.That(splits[2].ToString(), Is.EqualTo("f "));
        }

    }

    [TestFixture]
    public class FormatterTests
    {
        private object[] errorArgs = new object[]{ new FormatDelegate(format => { throw new Exception("ERROR!"); } ) };

        [Test]
        public void Formatter_Throws_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.ErrorAction = ErrorAction.ThrowError;

            try
            {
                formatter.Test("--{0}--", errorArgs, "--ERROR!--ERROR!--");
                Assert.Fail("Formatter should have thrown an exception, but did not.");
            }
            catch (FormatException ex)
            {
            }
        }

        [Test]
        public void Formatter_Outputs_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.ErrorAction = ErrorAction.OutputErrorInResult;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--ERROR!--ERROR!--");
        }

        [Test]
        public void Formatter_Ignores_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.ErrorAction = ErrorAction.Ignore;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "------");
        }

        [Test]
        public void Formatter_Maintains_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.ErrorAction = ErrorAction.MaintainTokens;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--{0}--{0:ZZZZ}--");
        }

        [Test]
        public void Formatter_Maintains_Object_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.ErrorAction = ErrorAction.MaintainTokens;
            formatter.Test("--{Object.Thing}--", errorArgs, "--{Object.Thing}--");
        }

        [Test]
        public void Formatter_AlignNull()
        {
            string name = null;
            var obj = new { name = name };
            var str2 = Smart.Format("Name: {name,-10}| Column 2", obj);
            Assert.That(str2, Is.EqualTo("Name:           | Column 2"));
        }
    }
}
