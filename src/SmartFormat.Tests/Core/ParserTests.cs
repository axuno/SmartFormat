using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.Common;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestParser()
        {
            var parser = new SmartFormatter() {Settings = { ParseErrorAction = ErrorAction.ThrowError}}.Parser;
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
            var results = formats.Select(f => new { format = f, parsed = parser.ParseFormat(f, new[] { Guid.NewGuid().ToString("N") }) }).ToArray();

            // Verify that the reconstructed formats
            // match the original ones:

            results.TryAll(r => Assert.AreEqual(r.format, r.parsed.ToString())).ThrowIfNotEmpty();
        }

        [Test]
        public void Parser_Throws_Exceptions()
        {
            // Let's set the "ErrorAction" to "Throw":
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.ParseErrorAction = ErrorAction.ThrowError;

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
                Assert.Throws<ParsingErrors>(() => formatter.Test(format, args, "Error"));
            }
        }

        [Test]
        public void Parser_Exception_ErrorDescription()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.ParseErrorAction = ErrorAction.ThrowError;

            foreach (var format in new[] { "{.}", "{.}{0.}" })
            {
                try
                {
                    formatter.Format(format);
                }
                catch (ParsingErrors e)
                {
                    Assert.IsTrue(e.HasIssues);
                    if (e.Issues.Count == 1)
                    {
                        Assert.IsTrue(e.Message.Contains("has 1 issue"));
                        Assert.IsTrue(e.MessageShort.Contains("has 1 issue"));
                    }
                    else
                    {
                        Assert.IsTrue(e.Message.Contains($"has {e.Issues.Count} issues"));
                        Assert.IsTrue(e.MessageShort.Contains($"has {e.Issues.Count} issues"));
                    }
                }
            }
        }

        [Test]
        public void Parser_Ignores_Exceptions()
        {
            var parser = new SmartFormatter() { Settings = { ParseErrorAction = ErrorAction.Ignore } }.Parser;
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
                var discard = parser.ParseFormat(format, new[] { Guid.NewGuid().ToString("N") });
            }
        }

        [Test]
        public void Parser_UseAlternativeBraces()
        {
            var parser = GetRegularParser();
            parser.UseAlternativeBraces('[', ']');
            var format = "aaa [bbb] [ccc:ddd] {eee} [fff:{ggg}] [hhh:[iii:[] ] ] jjj";
            var parsed = parser.ParseFormat(format, new[] { Guid.NewGuid().ToString("N") });

            Assert.AreEqual(9, parsed.Items.Count);

            Assert.AreEqual("bbb", ((Placeholder)parsed.Items[1]).Selectors[0].RawText);
            Assert.AreEqual("ccc", ((Placeholder)parsed.Items[3]).Selectors[0].RawText);
            Assert.AreEqual("ddd", ((Placeholder)parsed.Items[3]).Format.Items[0].RawText);
            Assert.AreEqual(" {eee} ", ((LiteralText)parsed.Items[4]).RawText);
            Assert.AreEqual("{ggg}", ((Placeholder)parsed.Items[5]).Format.Items[0].RawText);
            Assert.AreEqual("iii", ((Placeholder)((Placeholder)parsed.Items[7]).Format.Items[0]).Selectors[0].RawText);

        }
        
        private static Parser GetRegularParser()
        {
            var parser = new SmartFormatter() { Settings = { ParseErrorAction = ErrorAction.ThrowError } }.Parser;
            parser.AddAlphanumericSelectors();
            parser.AddOperators(".");
            return parser;
        }

        [Test]
        public void Test_Format_Substring()
        {
            var parser = GetRegularParser();
            var formatString = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";

            var format = parser.ParseFormat(formatString, new[] { Guid.NewGuid().ToString("N") });

            // Extract the substrings of literal text:
            Assert.That(format.Substring( 1, 3).ToString(), Is.EqualTo("a|a"));
            Assert.That(format.Substring(41, 4).ToString(), Is.EqualTo("gg|g"));

            // Extract a substring that overlaps into the placeholder:
            Assert.That(format.Substring(4, 3).ToString(), Is.EqualTo("a {bbb: ccc dd|d {:|||} {eee} ff|f }"));
            Assert.That(format.Substring(20, 23).ToString(), Is.EqualTo("{bbb: ccc dd|d {:|||} {eee} ff|f } gg"));

            // Make sure a invalid values are caught:
            Assert.That(() => format.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(100), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(10, 100), Throws.TypeOf<ArgumentOutOfRangeException>());


            // Now, test nested format strings:
            var placeholder = (Placeholder)format.Items[1];
            format = placeholder.Format;
            Assert.That(format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(format.Substring(5, 4).ToString(), Is.EqualTo("dd|d"));
            Assert.That(format.Substring(8, 3).ToString(), Is.EqualTo("d {:|||}"));
            Assert.That(format.Substring(8, 10).ToString(), Is.EqualTo("d {:|||} {eee}"));
            Assert.That(format.Substring(8, 16).ToString(), Is.EqualTo("d {:|||} {eee} f"));

            // Make sure invalid values are caught:
            Assert.That(() => format.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(30), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(25, 5), Throws.TypeOf<ArgumentOutOfRangeException>());

        }

        [Test]
        public void Test_Format_Alignment()
        {
            var parser = GetRegularParser();
            var formatString = "{0}";

            var format = parser.ParseFormat(formatString, new[] { Guid.NewGuid().ToString("N") });
            var placeholder = (Placeholder) format.Items[0];
            Assert.AreEqual(formatString, placeholder.ToString());
            placeholder.Alignment = 10;
            Assert.AreEqual($"{{0,{placeholder.Alignment}}}", placeholder.ToString());
        }

        [Test]
        public void Test_Formatter_Name_And_Options()
        {
            var parser = GetRegularParser();
            var formatString = "{0}";

            var format = parser.ParseFormat(formatString, new[] { Guid.NewGuid().ToString("N") });
            var placeholder = (Placeholder)format.Items[0];
            placeholder.FormatterName = "test";
            Assert.AreEqual($"{{0:{placeholder.FormatterName}}}", placeholder.ToString());
            placeholder.FormatterOptions = "options";
            Assert.AreEqual($"{{0:{placeholder.FormatterName}({placeholder.FormatterOptions})}}", placeholder.ToString());
        }

        [Test]
        public void Test_Format_IndexOf()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var Format = parser.ParseFormat(format, new[] { Guid.NewGuid().ToString("N") });

            Assert.That(Format.IndexOf('|'), Is.EqualTo(2));
            Assert.That(Format.IndexOf('|', 3), Is.EqualTo(43));
            Assert.That(Format.IndexOf('|', 44), Is.EqualTo(-1));
            Assert.That(Format.IndexOf('#'), Is.EqualTo(-1));

            // Test nested formats:
            var placeholder = (Placeholder) Format.Items[1];
            Format = placeholder.Format;
            Assert.That(Format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(Format.IndexOf('|'), Is.EqualTo(7));
            Assert.That(Format.IndexOf('|', 8), Is.EqualTo(25));
            Assert.That(Format.IndexOf('|', 26), Is.EqualTo(-1));
            Assert.That(Format.IndexOf('#'), Is.EqualTo(-1));
        }

        [Test]
        public void Test_Format_Split()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var Format = parser.ParseFormat(format, new[] { Guid.NewGuid().ToString("N") });
            var splits = Format.Split('|');

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" a"));
            Assert.That(splits[1].ToString(), Is.EqualTo("aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            Assert.That(splits[2].ToString(), Is.EqualTo("g "));

            // Test nested formats:
            var placeholder = (Placeholder) Format.Items[1];
            Format = placeholder.Format;
            Assert.That(Format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));
            splits = Format.Split('|');

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" ccc dd"));
            Assert.That(splits[1].ToString(), Is.EqualTo("d {:|||} {eee} ff"));
            Assert.That(splits[2].ToString(), Is.EqualTo("f "));
        }

        private Format Parse(string format, string[] formatterExentionNames )
        {
            return GetRegularParser().ParseFormat(format, formatterExentionNames);
        }

        [TestCase("{0:name:}", "name", "", "")]
        [TestCase("{0:name()}", "name", "", "")]
        [TestCase("{0:name(1|2|3)}", "name", "1|2|3", "")]
        [TestCase("{0:name:format}", "name", "", "format")]
        [TestCase("{0:name():format}", "name", "", "format")]
        [TestCase("{0:name():}", "name", "", "")]
        [TestCase("{0:name(1|2|3):format}", "name", "1|2|3", "format")]
        [TestCase("{0:name(1|2|3):}", "name", "1|2|3", "")]
        public void Name_of_registered_NamedFormatter_will_be_parsed(string format, string expectedName, string expectedOptions, string expectedFormat)
        {
            // The parser will only find names of named formatters which are registered. Names are case-sensitive.
            var formatterExtensions = new[] { "name" };
            
            // Named formatters will only be recognized by the parser, if their name occurs in one of FormatterExtensions.
            // If the name of the formatter does not exists, the string is treaded as format for the DefaultFormatter.
            var placeholder = (Placeholder) Parse(format, formatterExtensions).Items[0];
            Assert.AreEqual(expectedName, placeholder.FormatterName);
            Assert.AreEqual(expectedOptions, placeholder.FormatterOptions);
            Assert.AreEqual(expectedFormat, placeholder.Format.ToString());
        }

        [Test]
        public void Name_of_unregistered_NamedFormatter_will_not_be_parsed()
        {
            // find formatter formattername, which does not exist in the (empty) list of formatter extensions
            var placeholderWithNonExistingName = (Placeholder)Parse("{0:formattername:}", new string[] {} ).Items[0];
            Assert.AreEqual("formattername:", placeholderWithNonExistingName.Format.ToString()); // name is only treaded as a literal
        }

        // Incomplete:
        [TestCase(@"{0:format}")]
        [TestCase(@"{0:format(}")]
        [TestCase(@"{0:format)}")]
        [TestCase(@"{0:(format)}")]
        // Invalid:
        [TestCase(@"{0:format()stuff}")]
        [TestCase(@"{0:format() :}")]
        [TestCase(@"{0:format(s)|stuff}")]
        [TestCase(@"{0:format(s)stuff:}")]
        [TestCase(@"{0:format(:}")]
        [TestCase(@"{0:format):}")]
        // Escape sequences:
        [TestCase(@"{0:format\()}")]
        [TestCase(@"{0:format(\)}")]
        [TestCase(@"{0:format\:}")]
        [TestCase(@"{0:hh\:mm\:ss}")]
        // Empty:
        [TestCase(@"{0:}")]
        [TestCase(@"{0::}")]
        [TestCase(@"{0:()}")]
        [TestCase(@"{0:():}")]
        [TestCase(@"{0:(1|2|3)}")]
        [TestCase(@"{0:(1|2|3):}")]
        public void NamedFormatter_should_be_null_when_empty_or_invalid_or_escaped(string format)
        {
            var expectedLiteralText = format.Substring(3, format.Length - 3 - 1);
            AssertNoNamedFormatter(format, expectedLiteralText);
        }

        [TestCase(@"{0:format{}}", "format")]
        [TestCase(@"{0:{}}", "")]
        [TestCase(@"{0:{0:nested():}}", "")]
        [TestCase(@"{0:for{}mat}", "format")]
        [TestCase(@"{0:for{}mat()}", "format()")]
        [TestCase(@"{0:for(){}mat}", "for()mat")]
        public void NamedFormatter_should_be_null_when_has_nesting(string format, string expectedLiteralText)
        {
            AssertNoNamedFormatter(format, expectedLiteralText);
        }

        private void AssertNoNamedFormatter(string format, string expectedLiteralText)
        {
            var parser = GetRegularParser();
            parser.UseAlternativeEscapeChar('\\');
            parser.Settings.ConvertCharacterStringLiterals = false;

            var placeholder = (Placeholder) parser.ParseFormat(format, new[] { Guid.NewGuid().ToString("N") }).Items[0];
            Assert.IsEmpty(placeholder.FormatterName);
            Assert.IsEmpty(placeholder.FormatterOptions);
            var literalText = placeholder.Format.GetLiteralText();
            Assert.AreEqual(expectedLiteralText, literalText);
        }

        [Test]
        public void Parser_NotifyParsingError()
        {
            ParsingErrors parsingError = null;
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.Ignore;
            formatter.Settings.ParseErrorAction = ErrorAction.Ignore;
            formatter.Parser.OnParsingFailure += (o, args) => parsingError = args.Errors;
            var res = formatter.Format("{NoName {Other} {Same", default(object));
            Assert.That(parsingError.Issues.Count == 3);
            Assert.That(parsingError.Issues[2].Issue == new Parser.ParsingErrorText()[Parser.ParsingError.MissingClosingBrace]);
        }

        [Test]
        public void Nested_format_with_alternative_escaping()
        {
            var parser = GetRegularParser();
            // necessary because of the consecutive }}}, which would otherwise be escaped as }} and lead to "missing brace" exception:
            parser.UseAlternativeEscapeChar('\\'); 
            var placeholders = parser.ParseFormat("{c1:{c2:{c3}}}", new[] {Guid.NewGuid().ToString("N")});

            var c1 = (Placeholder) placeholders.Items[0];
            var c2 = (Placeholder) c1.Format.Items[0];
            var c3 = (Placeholder) c2.Format.Items[0];
            Assert.AreEqual("c1", c1.Selectors[0].RawText);
            Assert.AreEqual("c2", c2.Selectors[0].RawText);
            Assert.AreEqual("c3", c3.Selectors[0].RawText);
        }
    }
}
