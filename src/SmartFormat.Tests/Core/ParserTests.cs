using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class ParserTests
    {
        private static Parser GetRegularParser()
        {
            var parser = new SmartFormatter { Settings = { StringFormatCompatibility = false, Parser = {ErrorAction = ParseErrorAction.ThrowError }}}.Parser;
            return parser;
        }

        [Test]
        public void Basic_Parser_Test()
        {
            var parser = GetRegularParser();

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

        [TestCase("{")]
        [TestCase("{0")]
        [TestCase("}")]
        [TestCase("0}")]
        [TestCase("{{{")]
        [TestCase("}}}")]
        [TestCase("{.}")]
        [TestCase("{.:}")]
        [TestCase("{..}")]
        [TestCase("{..:}")]
        [TestCase("{0.}")]
        [TestCase("{0.:}")]
        public void Parser_Throws_Exceptions(string format)
        {
            // Let's set the "ErrorAction" to "Throw":
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

            var args = new object[] { TestFactory.GetPerson() };
            Assert.Throws<ParsingErrors>(() => formatter.Test(format, args, "Error"));
        }

        [Test]
        public void Parser_Exception_ErrorDescription()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

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
            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.Ignore;

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
                _ = parser.ParseFormat(format);
            }
        }

        [Test]
        public void Parser_Error_Action_Ignore()
        {
            //                     | Literal  | Erroneous     | | Okay |  
            var invalidTemplate = "Hello, I'm {Name from {City} {Street}";

            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.Ignore;
            var parsed = parser.ParseFormat(invalidTemplate);
            
            Assert.That(parsed.Items.Count, Is.EqualTo(4), "Number of parsed items");
            Assert.That(parsed.Items[0].RawText, Is.EqualTo("Hello, I'm "), "Literal text");
            Assert.That(parsed.Items[1].RawText, Is.EqualTo(string.Empty), "Erroneous placeholder");
            Assert.That(parsed.Items[2].RawText, Is.EqualTo(" "));
            Assert.That(parsed.Items[3], Is.TypeOf(typeof(Placeholder)));
            Assert.That(parsed.Items[3].RawText, Does.Contain("{Street}"), "Correct placeholder");
        }

        //         | Literal  | Erroneous     | | Okay |  
        //         Hello, I'm {Name from {City} {Street}
        [TestCase("Hello, I'm {Name from {City} {Street}", true)]
        //         | Literal  | Erroneous     | | Erroneous
        //         Hello, I'm {Name from {City} {Street
        [TestCase("Hello, I'm {Name from {City} {Street", false)]
        public void Parser_Error_Action_MaintainTokens(string invalidTemplate, bool lastItemIsPlaceholder)
        {
            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.MaintainTokens;
            var parsed = parser.ParseFormat(invalidTemplate);

            Assert.That(parsed.Items.Count, Is.EqualTo(4), "Number of parsed items");
            Assert.That(parsed.Items[0].RawText, Is.EqualTo("Hello, I'm "));
            Assert.That(parsed.Items[1].RawText, Is.EqualTo("{Name from {City}"));
            Assert.That(parsed.Items[2].RawText, Is.EqualTo(" "));
            if (lastItemIsPlaceholder)
            {
                Assert.That(parsed.Items[3], Is.TypeOf(typeof(Placeholder)), "Last item should be Placeholder");
                Assert.That(parsed.Items[3].RawText, Does.Contain("{Street}"));
            }
            else
            {
                Assert.That(parsed.Items[3], Is.TypeOf(typeof(LiteralText)), "Last item should be LiteralText");
                Assert.That(parsed.Items[3].RawText, Does.Contain("{Street"));
            }
        }

        [Test]
        public void Parser_Error_Action_OutputErrorInResult()
        {
            //                     | Literal  | Erroneous     |
            var invalidTemplate = "Hello, I'm {Name from {City}";
            
            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.OutputErrorInResult;
            var parsed = parser.ParseFormat(invalidTemplate);

            Assert.That(parsed.Items.Count, Is.EqualTo(1));
            Assert.That(parsed.Items[0].RawText, Does.StartWith("The format string has 3 issues"));
        }

        /// <summary>
        /// SmartFormat is not designed for processing JavaScript because of interfering usage of {}[].
        /// This example shows that even a comment can lead to parsing will work or not.
        /// </summary>
        [TestCase("/* The comment with this '}{' makes it fail */", "############### {TheVariable} ###############", false)]
        [TestCase("", "############### {TheVariable} ###############", true)]
        public void Parse_JavaScript_May_Succeed_Or_Fail(string var0, string var1, bool shouldSucceed)
        {
            var js = @"
(function(exports) {
  'use strict';
  /**
   * Searches for specific element in a given array using
   * the interpolation search algorithm.<br><br>
   * Time complexity: O(log log N) when elements are uniformly
   * distributed, and O(N) in the worst case
   *
   * @example
   *
   * var search = require('path-to-algorithms/src/searching/'+
   * 'interpolation-search').interpolationSearch;
   * console.log(search([1, 2, 3, 4, 5], 4)); // 3
   *
   * @public
   * @module searching/interpolation-search
   * @param {Array} sortedArray Input array.
   * @param {Number} seekIndex of the element which index should be found.
   * @returns {Number} Index of the element or -1 if not found.
   */
  function interpolationSearch(sortedArray, seekIndex) {
    let leftIndex = 0;
    let rightIndex = sortedArray.length - 1;

    while (leftIndex <= rightIndex) {
      const rangeDiff = sortedArray[rightIndex] - sortedArray[leftIndex];
      const indexDiff = rightIndex - leftIndex;
      const valueDiff = seekIndex - sortedArray[leftIndex];

      if (valueDiff < 0) {
        return -1;
      }

      if (!rangeDiff) {
        return sortedArray[leftIndex] === seekIndex ? leftIndex : -1;
      }

      const middleIndex =
        leftIndex + Math.floor((valueDiff * indexDiff) / rangeDiff);

      if (sortedArray[middleIndex] === seekIndex) {
        return middleIndex;
      }

      if (sortedArray[middleIndex] < seekIndex) {
        leftIndex = middleIndex + 1;
      } else {
        rightIndex = middleIndex - 1;
      }
    }
    " + var0 + @"
    /* " + var1 + @" */
    return -1;
  }
  exports.interpolationSearch = interpolationSearch;
})(typeof window === 'undefined' ? module.exports : window);
";
            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.MaintainTokens;
            var parsed = parser.ParseFormat(js);

            // No characters should get lost compared to the format string,
            // no matter if a Placeholder can be identified or not
            Assert.That(parsed.Items.Sum(i => i.RawText.Length), Is.EqualTo(js.Length), "No characters lost");

            if (shouldSucceed)
            {
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(1),
                    "One placeholder");
                Assert.That(parsed.Items.First(i => i.GetType() == typeof(Placeholder)).RawText,
                    Is.EqualTo("{TheVariable}"));
            }
            else
            {
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(0),
                    "NO placeholder");
            }
        }
        
        /// <summary>
        /// SmartFormat is not designed for processing CSS because of interfering usage of {}[].
        /// This example shows that even a comment can lead to parsing will work or not.
        /// </summary>
        [TestCase("", "############### {TheVariable} ###############", false)]
        [TestCase("/* This '}' in the comment makes it succeed */", "############### {TheVariable} ###############", true)]
        public void Parse_Css_May_Succeed_Or_Fail(string var0, string var1, bool shouldSucceed)
        {
            var css = @"
.media {
  display: grid;
  grid-template-columns: 1fr 3fr;
}

.media .content {
  font-size: .8rem;
}

.comment img {
  border: 1px solid grey;  " + var0 + @"
  anything: '" + var1 + @"'
}

.list-item {
  border-bottom: 1px solid grey;
} 
";
            var parser = GetRegularParser();
            parser.Settings.Parser.ErrorAction = ParseErrorAction.MaintainTokens;
            var parsed = parser.ParseFormat(css);

            // No characters should get lost compared to the format string,
            // no matter if a Placeholder can be identified or not
            Assert.That(parsed.Items.Sum(i => i.RawText.Length), Is.EqualTo(css.Length), "No characters lost");

            if (shouldSucceed)
            {
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(1),
                    "One placeholders");
                Assert.That(parsed.Items.First(i => i.GetType() == typeof(Placeholder)).RawText,
                    Is.EqualTo("{TheVariable}"));
            }
            else
            {
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(0),
                    "NO placeholder");
            }
        }

        [Test]
        public void Test_Format_Substring()
        {
            var parser = GetRegularParser();
            var formatString = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";

            var format = parser.ParseFormat(formatString);

            // Extract the substrings of literal text:
            Assert.That(format.Substring( 1, 3).ToString(), Is.EqualTo("a|a"));
            Assert.That(format.Substring(41, 4).ToString(), Is.EqualTo("gg|g"));

            // Extract a substring that overlaps into the placeholder:
            Assert.That(format.Substring(4, 3).ToString(), Is.EqualTo("a {bbb: ccc dd|d {:|||} {eee} ff|f }"));
            Assert.That(format.Substring(20, 23).ToString(), Is.EqualTo("{bbb: ccc dd|d {:|||} {eee} ff|f } gg"));

            // Make sure a invalid values are caught:
            var format1 = format;
            Assert.That(() => format1.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format1.Substring(100), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format1.Substring(10, 100), Throws.TypeOf<ArgumentOutOfRangeException>());


            // Now, test nested format strings:
            var placeholder = (Placeholder)format.Items[1];
            format = placeholder.Format!;
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
        public void Test_Format_Set_Alignment_Property()
        {
            var parser = GetRegularParser();
            var formatString = "{0}";

            var format = parser.ParseFormat(formatString);
            var placeholder = (Placeholder) format.Items[0];
            Assert.AreEqual(formatString, placeholder.ToString());
            placeholder.Alignment = 10;
            Assert.AreEqual($"{{0,{placeholder.Alignment}}}", placeholder.ToString());
        }

        [Test]
        public void Test_Format_With_Alignment()
        {
            var parser = GetRegularParser();
            var formatString = "{0,-10}";

            var format = parser.ParseFormat(formatString);
            var placeholder = (Placeholder) format.Items[0];
            Assert.That(placeholder.ToString(), Is.EqualTo(formatString));
            Assert.That(placeholder.Selectors.Count, Is.EqualTo(2));
            Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(","));
            Assert.That(placeholder.Selectors[1].RawText, Is.EqualTo("-10"));
        }

        [Test]
        public void Test_Formatter_Name_And_Options()
        {
            var parser = GetRegularParser();
            var formatString = "{0}";

            var format = parser.ParseFormat(formatString);
            var placeholder = (Placeholder)format.Items[0];
            placeholder.FormatterName = "test";
            Assert.AreEqual($"{{0:{placeholder.FormatterName}}}", placeholder.ToString());
            placeholder.FormatterOptionsRaw = "options";
            Assert.AreEqual($"{{0:{placeholder.FormatterName}({placeholder.FormatterOptions})}}", placeholder.ToString());
        }

        [Test]
        public void Test_Format_IndexOf()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var result = parser.ParseFormat(format);

            Assert.That(result.IndexOf('|'), Is.EqualTo(2));
            Assert.That(result.IndexOf('|', 3), Is.EqualTo(43));
            Assert.That(result.IndexOf('|', 44), Is.EqualTo(-1));
            Assert.That(result.IndexOf('#'), Is.EqualTo(-1));

            // Test nested formats:
            var placeholder = (Placeholder) result.Items[1];
            result = placeholder.Format!;
            Assert.That(result.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(result.IndexOf('|'), Is.EqualTo(7));
            Assert.That(result.IndexOf('|', 8), Is.EqualTo(25));
            Assert.That(result.IndexOf('|', 26), Is.EqualTo(-1));
            Assert.That(result.IndexOf('#'), Is.EqualTo(-1));
        }

        [Test]
        public void Test_Format_Split()
        {
            var parser = GetRegularParser();
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var parsedFormat = parser.ParseFormat(format);
            var splits = parsedFormat.Split('|');

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" a"));
            Assert.That(splits[1].ToString(), Is.EqualTo("aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            Assert.That(splits[2].ToString(), Is.EqualTo("g "));

            // Test nested formats:
            var placeholder = (Placeholder) parsedFormat.Items[1];
            parsedFormat = placeholder.Format!;
            Assert.That(parsedFormat.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));
            splits = parsedFormat.Split('|');

            Assert.That(splits.Count, Is.EqualTo(3));
            Assert.That(splits[0].ToString(), Is.EqualTo(" ccc dd"));
            Assert.That(splits[1].ToString(), Is.EqualTo("d {:|||} {eee} ff"));
            Assert.That(splits[2].ToString(), Is.EqualTo("f "));
        }

        private Format Parse(string format, string[] formatterExentionNames )
        {
            return GetRegularParser().ParseFormat(format);
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
            var parser = GetRegularParser();
            
            // Named formatters will only be recognized by the parser, if their name occurs in one of FormatterExtensions.
            // If the name of the formatter does not exists, the string is treaded as format for the DefaultFormatter.
            var placeholder = (Placeholder) parser.ParseFormat(format).Items[0];
            Assert.AreEqual(expectedName, placeholder.FormatterName);
            Assert.AreEqual(expectedOptions, placeholder.FormatterOptions);
            Assert.AreEqual(expectedFormat, placeholder.Format!.ToString());
        }

        [Test]
        public void Name_of_unregistered_NamedFormatter_will_be_parsed()
        {
            // find formatter formatter name, which does not exist in the (empty) list of formatter extensions
            var parser = GetRegularParser();
            var placeholderWithNonExistingName = (Placeholder)parser.ParseFormat("{0:formattername:}").Items[0];
            Assert.AreEqual("formattername", placeholderWithNonExistingName.FormatterName);
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
            
            var parser = GetRegularParser();
            parser.Settings.Parser.ConvertCharacterStringLiterals = false;

            var placeholder = (Placeholder) parser.ParseFormat(format).Items[0];
            var literalText = placeholder.Format?.GetLiteralText();

            Assert.That(placeholder.FormatterName, Is.Empty);
            Assert.That(placeholder.FormatterOptions, Is.Empty);
            Assert.That(literalText, Is.EqualTo(expectedLiteralText));
        }

        [TestCase(@"{0:format{}}", "format")]
        [TestCase(@"{0:{}}", "")]
        [TestCase(@"{0:{0:nested():}}", "")]
        [TestCase(@"{0:for{}mat}", "format")]
        [TestCase(@"{0:for{}mat()}", "format()")]
        [TestCase(@"{0:for(){}mat}", "for()mat")]
        public void NamedFormatter_should_be_null_when_has_nesting(string format, string expectedLiteralText)
        {
            var parser = GetRegularParser();
            parser.Settings.Parser.ConvertCharacterStringLiterals = false;

            var placeholder = (Placeholder) parser.ParseFormat(format).Items[0];
            var literalText = placeholder.Format?.GetLiteralText();

            Assert.That(placeholder.FormatterName, Is.Empty);
            Assert.That(placeholder.FormatterOptions, Is.Empty);
            Assert.That(literalText, Is.EqualTo(expectedLiteralText));
        }
        
        [Test]
        public void Parser_NotifyParsingError()
        {
            ParsingErrors? parsingError = null;
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.Ignore;
            formatter.Settings.Parser.ErrorAction = ParseErrorAction.Ignore;
            
            formatter.Parser.OnParsingFailure += (o, args) => parsingError = args.Errors;
            var res = formatter.Format("{NoName {Other} {Same", default(object)!);
            
            Assert.That(parsingError!.Issues.Count, Is.EqualTo(3));
            Assert.That(parsingError.Issues[2].Issue,  Is.EqualTo(new Parser.ParsingErrorText()[SmartFormat.Core.Parsing.Parser.ParsingError.MissingClosingBrace]));
        }

        [Test]
        public void Missing_Curly_Brace_Should_Throw()
        {
            var parser = GetRegularParser();
            var format = "{0:yyyy/MM/dd HH:mm:ss";

            Assert.That(() => parser.ParseFormat(format),
                Throws.Exception.InstanceOf(typeof(ParsingErrors)).And.Message
                    .Contains(new Parser.ParsingErrorText()[Parser.ParsingError.MissingClosingBrace]));
        }
        
        [Test]
        public void Literal_Escaping_In_Literal()
        {
            var parser = GetRegularParser();
            parser.Settings.StringFormatCompatibility = false;
            Assert.That(parser.ParseFormat("\\{\\}").ToString(), Is.EqualTo("{}"));
        }

        [Test]
        public void StringFormat_Escaping_In_Literal()
        {
            var parser = GetRegularParser();
            parser.Settings.StringFormatCompatibility = true;
            Assert.That(parser.ParseFormat("{{}}").ToString(), Is.EqualTo("{}"));
        }


        [Test]
        public void Nested_format_with_literal_escaping()
        {
            var parser = GetRegularParser();
            // necessary because of the consecutive }}}, which would otherwise be escaped as }} and lead to "missing brace" exception:
            var placeholders = parser.ParseFormat("{c1:{c2:{c3}}}");

            var c1 = (Placeholder) placeholders.Items[0];
            var c2 = (Placeholder) c1.Format?.Items[0]!;
            var c3 = (Placeholder) c2.Format?.Items[0]!;
            Assert.AreEqual("c1", c1.Selectors[0].RawText);
            Assert.AreEqual("c2", c2.Selectors[0].RawText);
            Assert.AreEqual("c3", c3.Selectors[0].RawText);
        }

        [Test]
        public void Parse_Options()
        {
            var parser = GetRegularParser();
            var selector = "0";
            var formatterName = "c";
            // Not escaped special chars {}:() must finish parsing of format options.
            // Unescaped operators []., are fine.
            // Options can store any chars, as long as special chars are escaped.
            var options = "\\{.\\)\\:,_][1|2|3"; 
            // The literal may also contain escaped characters
            var literal = "one|two|th\\} \\{ree|other";

            var format = parser.ParseFormat($"{{{selector}:{formatterName}({options}):{literal}}}");
            var placeholder = (Placeholder) format.Items[0];
            
            Assert.That(format.Items.Count, Is.EqualTo(1));
            Assert.That(placeholder.Selectors.First().RawText, Is.EqualTo(selector));
            Assert.That(format.HasNested, Is.True);
            Assert.That(placeholder.FormatterName, Is.EqualTo(formatterName));
            Assert.That(placeholder.FormatterOptions, Is.EqualTo(options.Replace("\\", "")));
            Assert.That(placeholder.Format?.Items.Count, Is.EqualTo(3));
            Assert.That(string.Concat(placeholder.Format?.Items[0].ToString(), placeholder.Format?.Items[1].ToString(), placeholder.Format?.Items[2].ToString()), Is.EqualTo(literal.Replace("\\", "")));
        }

        [TestCase(@"\u1234", @"\u1234", 0, true)]
        [TestCase(@"\u1234abc", @"\u1234", 0, true)]
        [TestCase(@"abc\u1234", @"\u1234", 1, true)]
        [TestCase(@"abc\u1234def", @"\u1234", 1, true)]
        [TestCase(@"\uwxyz", @"\uwxyz", 0, false)] // Parser accepts illegal sequence
        [TestCase(@"\uw", @"\uw", 0, false)] // Parser accepts illegal sequence, even if shorter than 4 characters
        public void Parse_Unicode(string formatString, string unicodeLiteral, int itemIndex, bool isLegal)
        {
            var parser = GetRegularParser();
            var result = parser.ParseFormat(formatString);

            var literal = result.Items[itemIndex];
            Assert.That(literal, Is.TypeOf(typeof(LiteralText)));
            Assert.That(literal.BaseString.Substring(literal.StartIndex, literal.Length), Is.EqualTo(unicodeLiteral));
            
            if(isLegal) 
                Assert.That(literal.ToString()[0], Is.EqualTo((char) int.Parse(unicodeLiteral.Substring(2), System.Globalization.NumberStyles.HexNumber)));
            else
                Assert.That(() => literal.ToString(), Throws.ArgumentException.And.Message.Contains(unicodeLiteral));
        }

        [TestCase("{A }", ' ')]
        [TestCase("{B§}", '§')]
        [TestCase("{%C}", '%')]
        public void Selector_With_Custom_Selector_Character(string formatString, char customChar)
        {
            var parser = GetRegularParser();
            parser.Settings.Parser.AddCustomSelectorChars(new[]{customChar});
            var result = parser.ParseFormat(formatString);

            var placeholder = result.Items[0] as Placeholder;
            Assert.That(placeholder, Is.Not.Null);
            Assert.That(placeholder!.Selectors.Count, Is.EqualTo(1));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1,2)));
        }

        [TestCase("{a b}", ' ')]
        [TestCase("{a°b}", '°')]
        public void Selectors_With_Custom_Operator_Character(string formatString, char customChar)
        {
            var parser = GetRegularParser();
            parser.Settings.Parser.AddCustomOperatorChars(new[]{customChar});
            var result = parser.ParseFormat(formatString);

            var placeholder = result.Items[0] as Placeholder;
            Assert.That(placeholder, Is.Not.Null);
            Assert.That(placeholder!.Selectors.Count, Is.EqualTo(2));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1,1)));
            Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(formatString.Substring(3, 1)));
            Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(formatString.Substring(2,1)));
        }

        [TestCase("{A?.B}")]
        [TestCase("{Selector0?.Selector1}")]
        [TestCase("{A?[1].B}")]
        [TestCase("{List?[123].Selector}")]
        public void Selector_With_Nullable_Operator_Character(string formatString)
        {
            // contiguous operator characters are parsed as "ONE operator string"

            var regex = new Regex(@"\{(?<Sel_0>[a-zA-Z0-9]+)(?<Sel_1_Op>[\?\.|\[]+)(?<Sel_1>\d*)(?<Sel_2_Op>[\?|\.|\]]+)(?<Sel_2>[a-zA-Z0-9]+)\}",
                RegexOptions.None);
            var reMatches = regex.Matches(formatString);
            var numOfSelectors = (reMatches[0].Groups["Sel_0"].Value == string.Empty ? 0 : 1) +
                (reMatches[0].Groups["Sel_1"].Value == string.Empty ? 0 : 1) +
                (reMatches[0].Groups["Sel_2"].Value == string.Empty ? 0 : 1);

            var parser = GetRegularParser();
            var result = parser.ParseFormat(formatString);

            var placeholder = result.Items[0] as Placeholder;
            Assert.That(placeholder, Is.Not.Null);
            Assert.That(placeholder!.Selectors.Count, Is.EqualTo(numOfSelectors));
            if (numOfSelectors == 2)
            {
                Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_0"].Value));
                // Group Sel_1 is empty
                Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_2"].Value));
                // Concatenate because of regex simplification for 2 selectors
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value + reMatches[0].Groups["Sel_2_Op"].Value));
            }
            else
            {
                Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_0"].Value));
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value));
                Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_1"].Value));
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value));
                Assert.That(placeholder.Selectors[2].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_2"].Value));
            }
        }

        [TestCase("{A?.B}", '.')] // with "nullable" operator
        [TestCase("{C%.D}", '%')] 
        [TestCase("{C..D}", '.')] 
        public void Selector_With_Other_Contiguous_Operator_Characters(string formatString, char customChar)
        {
            // contiguous operator characters are parsed as "ONE operator string"

            var parser = GetRegularParser();
            // adding '.' is ignored, as it's a standard operator
            parser.Settings.Parser.AddCustomOperatorChars(new[]{customChar});
            var result = parser.ParseFormat(formatString);

            var placeholder = result.Items[0] as Placeholder;
            Assert.That(placeholder, Is.Not.Null);
            Assert.That(placeholder!.Selectors.Count, Is.EqualTo(2));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1,1)));
            Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(formatString.Substring(4,1)));
            Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(formatString.Substring(2,2)));
        }
    }
}
