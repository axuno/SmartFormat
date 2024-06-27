using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class ParserTests
{
    private static Parser GetRegularParser(SmartSettings? settings = null)
    {
        var parser = new Parser(settings ?? new SmartSettings
            {StringFormatCompatibility = false, Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}});
        return parser;
    }

    [TestCase("{City}{PostalCode}{Gender}{FirstName}{LastName}")]
    [TestCase("Address: {City.ZipCode} {City.Name}, {City.AreaCode}\nName: {Person.FirstName} {Person.LastName}")]
    [TestCase("{a.b.c.d}")]
    [TestCase(" aaa {bbb.ccc: ddd {eee} fff } ggg ")]
    [TestCase("{aaa} {bbb}")]
    [TestCase("{}")]
    [TestCase("{a:{b:{c:{d}}}}")]
    [TestCase("{a}")]
    [TestCase(" aaa {bbb_bbb.CCC} ddd ")]
    public void Basic_Parser_Test(string format)
    {
        var parser = GetRegularParser();

        // Verify that the reconstructed formats
        // match the original ones:
        using var fmt = parser.ParseFormat(format);

        // The same BaseString reference should be passed around
        Assert.That(fmt.Items[0].BaseString, Is.SameAs(fmt.BaseString));
        if(fmt.Items[0] is Placeholder ph && ph.Selectors.Count > 0)
            Assert.That(ph.Selectors[0].BaseString, Is.SameAs(fmt.BaseString));

        Assert.That(fmt.ToString(), Is.EqualTo(format));
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
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings
            {Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}});

        var args = new object[] { TestFactory.GetPerson() };
        Assert.Throws<ParsingErrors>(() => formatter.Test(format, args, "Error"));
    }

    [TestCase("{V(LU)}")] // braces are illegal
    [TestCase("{V LU }")] // blanks are illegal
    [TestCase("{VĀLUĒ}")] // 0x100 and 0x112 are illegal chars
    public void Parser_Throws_On_Illegal_Selector_Chars(string format)
    {
        var parser = GetRegularParser();
        try
        {
            parser.ParseFormat(format);
            throw new InvalidOperationException("The parser should have thrown an exception.");
        }
        catch (Exception e)
        {
            Assert.Multiple(() =>
            {
                // Throws, because selector contains 2 illegal characters
                Assert.That(e, Is.InstanceOf<ParsingErrors>());
                Assert.That(((ParsingErrors) e).Issues, Has.Count.EqualTo(2));
            });
        }
    }

    [Test]
    public void Parser_Exception_ErrorDescription()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings
            {Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}});

        foreach (var format in new[] { "{.}", "{.}{0.}" })
        {
            try
            {
                formatter.Format(format);
            }
            catch (ParsingErrors e)
            {
                Assert.That(e.HasIssues, Is.True);
                if (e.Issues.Count == 1)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(e.Message, Does.Contain("has 1 issue"));
                        Assert.That(e.MessageShort, Does.Contain("has 1 issue"));
                    });
                }
                else
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(e.Message, Does.Contain($"has {e.Issues.Count} issues"));
                        Assert.That(e.MessageShort, Does.Contain($"has {e.Issues.Count} issues"));
                    });
                }
            }
        }
    }

    [Test]
    public void Parser_Ignores_Exceptions()
    {
        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ErrorAction = ParseErrorAction.Ignore}});

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
            Assert.That(del: () => _ = parser.ParseFormat(format), Throws.Nothing);
        }
    }

    [Test]
    public void Parser_Error_Action_Ignore()
    {
        //                     | Literal  | Erroneous     | | Okay |  
        var invalidTemplate = "Hello, I'm {Name from {City} {Street}";

        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ErrorAction = ParseErrorAction.Ignore}});
        using var parsed = parser.ParseFormat(invalidTemplate);
            
        Assert.That(parsed.Items, Has.Count.EqualTo(4), "Number of parsed items");
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Items[0].RawText, Is.EqualTo("Hello, I'm "), "Literal text");
            Assert.That(parsed.Items[1].RawText, Is.EqualTo(string.Empty), "Erroneous placeholder");
            Assert.That(parsed.Items[2].RawText, Is.EqualTo(" "));
            Assert.That(parsed.Items[3], Is.TypeOf(typeof(Placeholder)));
        });
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
        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ErrorAction = ParseErrorAction.MaintainTokens}});
        using var parsed = parser.ParseFormat(invalidTemplate);

        Assert.That(parsed.Items, Has.Count.EqualTo(4), "Number of parsed items");
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Items[0].RawText, Is.EqualTo("Hello, I'm "));
            Assert.That(parsed.Items[1].RawText, Is.EqualTo("{Name from {City}"));
            Assert.That(parsed.Items[2].RawText, Is.EqualTo(" "));
        });
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
            
        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ErrorAction = ParseErrorAction.OutputErrorInResult}});
        using var parsed = parser.ParseFormat(invalidTemplate);

        Assert.That(parsed.Items, Has.Count.EqualTo(1));
        Assert.That(parsed.Items[0].RawText, Does.StartWith("The format string has 3 issues"));
    }

    [Test]
    public void Test_Format_Substring()
    {
        var parser = GetRegularParser();
        var formatString = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";

        var format = parser.ParseFormat(formatString);

        Assert.Multiple(() =>
        {
            // Extract the substrings of literal text:
            Assert.That(format.Substring(1, 3).ToString(), Is.EqualTo("a|a"));
            Assert.That(format.Substring(41, 4).ToString(), Is.EqualTo("gg|g"));

            // Extract a substring that overlaps into the placeholder:
            Assert.That(format.Substring(4, 3).ToString(), Is.EqualTo("a {bbb: ccc dd|d {:|||} {eee} ff|f }"));
            Assert.That(format.Substring(20, 23).ToString(), Is.EqualTo("{bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
        });

        // Make sure a invalid values are caught:
        var format1 = format;
        Assert.That(() => format1.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => format1.Substring(100), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => format1.Substring(10, 100), Throws.TypeOf<ArgumentOutOfRangeException>());


        // Now, test nested format strings:
        var placeholder = (Placeholder)format.Items[1];
        format = placeholder.Format!;
        Assert.Multiple(() =>
        {
            Assert.That(format.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(format.Substring(5, 4).ToString(), Is.EqualTo("dd|d"));
            Assert.That(format.Substring(8, 3).ToString(), Is.EqualTo("d {:|||}"));
            Assert.That(format.Substring(8, 10).ToString(), Is.EqualTo("d {:|||} {eee}"));
            Assert.That(format.Substring(8, 16).ToString(), Is.EqualTo("d {:|||} {eee} f"));

            // Make sure invalid values are caught:
            Assert.That(() => format.Substring(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(30), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => format.Substring(25, 5), Throws.TypeOf<ArgumentOutOfRangeException>());
        });

    }

    [Test]
    public void Test_Format_With_Alignment()
    {
        var parser = GetRegularParser();
        var formatString = "{0,-10}";

        using var format = parser.ParseFormat(formatString);
        var placeholder = (Placeholder) format.Items[0];
        Assert.Multiple(() =>
        {
            Assert.That(placeholder.ToString(), Is.EqualTo(formatString));
            Assert.That(placeholder.Selectors, Has.Count.EqualTo(2));
        });
        Assert.Multiple(() =>
        {
            Assert.That(placeholder.Selectors[1].Operator[0], Is.EqualTo(','));
            Assert.That(placeholder.Selectors[1].RawText, Is.EqualTo("-10"));
        });
    }

    [Test]
    public void Test_Format_IndexOf()
    {
        var parser = GetRegularParser();
        var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
        var result = parser.ParseFormat(format);

        Assert.Multiple(() =>
        {
            Assert.That(result.IndexOf('|'), Is.EqualTo(2));
            Assert.That(result.IndexOf('|', 3), Is.EqualTo(43));
            Assert.That(result.IndexOf('|', 44), Is.EqualTo(-1));
            Assert.That(result.IndexOf('#'), Is.EqualTo(-1));
        });

        // Test nested formats:
        var placeholder = (Placeholder) result.Items[1];
        result = placeholder.Format!;
        Assert.Multiple(() =>
        {
            Assert.That(result.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));

            Assert.That(result.IndexOf('|'), Is.EqualTo(7));
            Assert.That(result.IndexOf('|', 8), Is.EqualTo(25));
            Assert.That(result.IndexOf('|', 26), Is.EqualTo(-1));
            Assert.That(result.IndexOf('#'), Is.EqualTo(-1));
        });
    }

    [Test]
    public void Test_Format_Split()
    {
        var parser = GetRegularParser();
        var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
        var parsedFormat = parser.ParseFormat(format);
        var splits = parsedFormat.Split('|');

        Assert.That(splits, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(splits[0].ToString(), Is.EqualTo(" a"));
            Assert.That(splits[1].ToString(), Is.EqualTo("aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            Assert.That(splits[2].ToString(), Is.EqualTo("g "));
        });

        // Test nested formats:
        var placeholder = (Placeholder) parsedFormat.Items[1];
        parsedFormat = placeholder.Format!;
        Assert.That(parsedFormat.ToString(), Is.EqualTo(" ccc dd|d {:|||} {eee} ff|f "));
        splits = parsedFormat.Split('|');

        Assert.That(splits, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(splits[0].ToString(), Is.EqualTo(" ccc dd"));
            Assert.That(splits[1].ToString(), Is.EqualTo("d {:|||} {eee} ff"));
            Assert.That(splits[2].ToString(), Is.EqualTo("f "));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(placeholder.FormatterName.ToString(), Is.EqualTo(expectedName));
            Assert.That(placeholder.FormatterOptions.ToString(), Is.EqualTo(expectedOptions));
            Assert.That(placeholder.Format!.ToString(), Is.EqualTo(expectedFormat));
        });
    }

    [Test]
    public void Name_of_unregistered_NamedFormatter_will_be_parsed()
    {
        // find formatter formatter name, which does not exist in the (empty) list of formatter extensions
        var parser = GetRegularParser();
        var placeholderWithNonExistingName = (Placeholder)parser.ParseFormat("{0:formattername:}").Items[0];
        Assert.That(placeholderWithNonExistingName.FormatterName.ToString(), Is.EqualTo("formattername"));
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

        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = false}});

        var placeholder = (Placeholder) parser.ParseFormat(format).Items[0];
        var literalText = placeholder.Format?.GetLiteralText();

        Assert.Multiple(() =>
        {
            Assert.That(placeholder.FormatterName.ToString(), Is.Empty);
            Assert.That(placeholder.FormatterOptions.ToString(), Is.Empty);
            Assert.That(literalText, Is.EqualTo(expectedLiteralText));
        });
    }

    [TestCase(@"{0:format{}}", "format")]
    [TestCase(@"{0:{}}", "")]
    [TestCase(@"{0:{0:nested():}}", "")]
    [TestCase(@"{0:for{}mat}", "format")]
    [TestCase(@"{0:for{}mat()}", "format()")]
    [TestCase(@"{0:for(){}mat}", "for()mat")]
    public void NamedFormatter_should_be_null_when_has_nesting(string format, string expectedLiteralText)
    {
        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ConvertCharacterStringLiterals = false}});

        var placeholder = (Placeholder) parser.ParseFormat(format).Items[0];
        var literalText = placeholder.Format?.GetLiteralText();

        Assert.Multiple(() =>
        {
            Assert.That(placeholder.FormatterName, Is.Empty);
            Assert.That(placeholder.FormatterOptions.ToString(), Is.Empty);
            Assert.That(literalText, Is.EqualTo(expectedLiteralText));
        });
    }
        
    [Test]
    public void Parser_NotifyParsingError()
    {
        ParsingErrors? parsingError = null;
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings
        {
            Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.Ignore},
            Parser = new ParserSettings {ErrorAction = ParseErrorAction.Ignore}
        });
            
        formatter.Parser.OnParsingFailure += (o, args) => parsingError = args.Errors;
        var res = formatter.Format("{NoName {Other} {Same", default(object)!);
        Assert.Multiple(() =>
        {
            Assert.That(parsingError!.Issues, Has.Count.EqualTo(3));
            Assert.That(parsingError.Issues[2].Issue, Is.EqualTo(new Parser.ParsingErrorText()[SmartFormat.Core.Parsing.Parser.ParsingError.MissingClosingBrace]));
        });
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
        var parser = GetRegularParser(new SmartSettings {StringFormatCompatibility = false});
        Assert.That(parser.ParseFormat("\\{\\}").ToString(), Is.EqualTo("{}"));
    }

    [Test]
    public void StringFormat_Escaping_In_Literal()
    {
        var parser = GetRegularParser(new SmartSettings {StringFormatCompatibility = true});
        Assert.That(parser.ParseFormat("{{}}").ToString(), Is.EqualTo("{}"));
    }


    [Test]
    public void Nested_format_with_literal_escaping()
    {
        var parser = GetRegularParser();
        var placeholders = parser.ParseFormat("{c1:{c2:{c3}}}");

        var c1 = (Placeholder) placeholders.Items[0];
        var c2 = (Placeholder) c1.Format?.Items[0]!;
        var c3 = (Placeholder) c2.Format?.Items[0]!;
        Assert.Multiple(() =>
        {
            Assert.That(c1.Selectors[0].RawText, Is.EqualTo("c1"));
            Assert.That(c2.Selectors[0].RawText, Is.EqualTo("c2"));
            Assert.That(c3.Selectors[0].RawText, Is.EqualTo("c3"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(format.Items, Has.Count.EqualTo(1));
            Assert.That(placeholder.Selectors[0].RawText, Is.EqualTo(selector));
            Assert.That(format.HasNested, Is.True);
            Assert.That(placeholder.FormatterName.ToString(), Is.EqualTo(formatterName));
            Assert.That(placeholder.FormatterOptions.ToString(), Is.EqualTo(options.Replace("\\", "")));
            Assert.That(placeholder.Format?.Items.Count, Is.EqualTo(3));
            Assert.That(string.Concat(placeholder.Format?.Items[0].ToString(), placeholder.Format?.Items[1].ToString(), placeholder.Format?.Items[2].ToString()), Is.EqualTo(literal.Replace("\\", "")));
        });
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
        var settings = new SmartSettings();
        settings.Parser.AddCustomSelectorChars(new[]{customChar});
        var parser = GetRegularParser(settings);
        var result = parser.ParseFormat(formatString);

        var placeholder = result.Items[0] as Placeholder;
        Assert.That(placeholder, Is.Not.Null);
        Assert.That(placeholder!.Selectors, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(placeholder!.Selectors, Has.Count.EqualTo(placeholder!.GetSelectors().Count));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1, 2)));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(placeholder!.Selectors, Has.Count.EqualTo(2));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1, 1)));
            Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(formatString.Substring(3, 1)));
            Assert.That(placeholder.Selectors[1].Operator.ToString(), Is.EqualTo(formatString.Substring(2, 1)));
        });
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
        Assert.That(placeholder!.Selectors, Has.Count.EqualTo(numOfSelectors));
        if (numOfSelectors == 2)
        {
            Assert.Multiple(() =>
            {
                Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_0"].Value));
                // Group Sel_1 is empty
                Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_2"].Value));
                // Concatenate because of regex simplification for 2 selectors
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value + reMatches[0].Groups["Sel_2_Op"].Value));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_0"].Value));
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value));
                Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_1"].Value));
            });
            Assert.Multiple(() =>
            {
                Assert.That(placeholder.Selectors[1].Operator, Is.EqualTo(reMatches[0].Groups["Sel_1_Op"].Value));
                Assert.That(placeholder.Selectors[2].ToString(), Is.EqualTo(reMatches[0].Groups["Sel_2"].Value));
            });
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
        Assert.Multiple(() =>
        {
            Assert.That(placeholder!.Selectors, Has.Count.EqualTo(2));
            Assert.That(placeholder.Selectors[0].ToString(), Is.EqualTo(formatString.Substring(1, 1)));
            Assert.That(placeholder.Selectors[1].ToString(), Is.EqualTo(formatString.Substring(4, 1)));
            Assert.That(placeholder.Selectors[1].Operator.ToString(), Is.EqualTo(formatString.Substring(2, 2)));
        });
    }

    // Tags which will be skipped by method Parser.ParseHtmlTags()
    [TestCase("</>")] // nonsense
    [TestCase("<>")] // nonsense
    [TestCase("<br/>")] // self-closing tag
    [TestCase("<p>")] // no closing tag
    // Tags which will be analyzed by method Parser.ParseHtmlTags()
    [TestCase("<script />")] // self-closing script
    [TestCase("<style />")] // self-closing style
    [TestCase("</script>")] // end script tag without start, won't parse
    [TestCase("</style>")] // end style tag without start, won't parse
    [TestCase("<style> something")] // open tag without closing
    [TestCase("<style></style>")] // start and end tag
    [TestCase("<script></script>")] // start and end tag
    [TestCase("<STYLE></style>")] // start and end tag should be case-insensitive
    [TestCase("<script></SCRIPT>")] // start and end tag should be case-insensitive
    [TestCase("<script></script")] // end tag not closed correctly
    [TestCase("<script><illegal/></script>")] // containing a child element of script tag
    [TestCase("<style><illegal/></style>")] // containing a child element of style tag
    [TestCase("Something <style></style>! nice")] // tags surrounded by literals
    [TestCase("Something <script></script>! nice")] // tags surrounded by tags and literals
    [TestCase("Something <script>{const a='</script>';}</script>! nice")] // assign a variable
    [TestCase("<script src=\"</script>.js\"></script>")] // script with src-attribute
    [TestCase("<script>{NoPlaceholder}</script>")] // should not parse as placeholder
    [TestCase("<style>{NoPlaceholder}</style>")] // should not parse as placeholder
    [TestCase("Something <style>h1 {color:red;}</style>! nice")]
    [TestCase("Something <style illegal=\"</style>\"></style>! nice")] // include an HTML attribute
    public void ParseInputAsHtml(string input)
    {
        var parser = GetRegularParser(new SmartSettings
        {
            StringFormatCompatibility = false,
            Parser = new ParserSettings { ErrorAction = ParseErrorAction.ThrowError, ParseInputAsHtml = true }
        });

        var result = parser.ParseFormat(input);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        var literalText = result.Items[0] as LiteralText;
        Assert.That(literalText, Is.Not.Null);
        Assert.That(literalText!.RawText, Is.EqualTo(input));
    }

    [TestCase("<script>{Placeholder}</script>", false)] // should parse a placeholder
    [TestCase("<style>{Placeholder}</style>", false)] // should parse a placeholder
    [TestCase("Something <style>h1 { color : #000; }</style>! nice", true)] // illegal selector chars
    [TestCase("Something <script>{const a = '</script>';}</script>! nice", true)] // illegal selector chars
    public void ParseHtmlInput_Without_ParserSetting_IsHtml(string input, bool shouldThrow)
    {
        var parser = GetRegularParser(new SmartSettings
        {
            StringFormatCompatibility = false,
            Parser = new ParserSettings { ErrorAction = ParseErrorAction.ThrowError, ParseInputAsHtml = false }
        });

        switch (shouldThrow)
        {
            case true:
                Assert.That(() => _ = parser.ParseFormat(input), Throws.TypeOf<ParsingErrors>());
                break;
            case false:
            {
                var result = parser.ParseFormat(input);
                Assert.That(result.Items, Has.Count.EqualTo(3));
                break;
            }
        }
    }

    /// <summary>
    /// SmartFormat is able to parse script tags, if <see cref="ParserSettings.ParseInputAsHtml"/> is <see langword="true"/>
    /// </summary>
    [TestCase(false, true)]
    [TestCase(true, false)]
    public void ScriptTags_Can_Be_Parsed_Without_Failure(bool inputIsHtml, bool shouldFail)
    {
        var js = @"
<script type=""text/javascript"">
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
    return -1;
  }
  exports.interpolationSearch = interpolationSearch;
})(typeof window === 'undefined' ? module.exports : window);
/* Comment '}{' which mixes up the parser without ParserSettings.ParseInputAsHtml = true */
</script>
<p>############### {TheVariable} ###############</p>
";
        var parsingFailures = 0;
        var parser = GetRegularParser(new SmartSettings {Parser = new ParserSettings {ErrorAction = ParseErrorAction.MaintainTokens, ParseInputAsHtml = inputIsHtml}});
        parser.OnParsingFailure += (o, args) => parsingFailures++;
        var parsed = parser.ParseFormat(js);
            
        // No characters should get lost compared to the format string,
        // no matter if ParsingErrors occur or not
        Assert.That(parsed.Items.Sum(i => i.RawText.Length), Is.EqualTo(js.Length), "No characters lost");

        switch (shouldFail)
        {
            case true:
                Assert.That(parsingFailures, Is.GreaterThan(0));
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(0),
                    "NO placeholder");
                break;
            case false:
                Assert.That(parsingFailures, Is.EqualTo(0));
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(1),
                    "One placeholder");
                Assert.That(parsed.Items.First(i => i.GetType() == typeof(Placeholder)).RawText,
                    Is.EqualTo("{TheVariable}"));
                break;
        }
    }
        
    /// <summary>
    /// SmartFormat is able to parse style tags, if <see cref="ParserSettings.ParseInputAsHtml"/> is <see langword="true"/>
    /// </summary>
    [TestCase(false, true)]
    [TestCase(true, false)]
    public void StyleTags_Can_Be_Parsed_Without_Failure(bool inputIsHtml, bool shouldFail)
    {
        var styles = @"
<style type='text/css'>
.media {
  display: grid;
  grid-template-columns: 1fr 3fr;
}

.media .content {
  font-size: .8rem;
}

.comment img {
  border: 1px solid grey;
  anything: 'xyz'
}

.list-item {
  border-bottom: 1px solid grey;
}
/* Comment: { which mixes up the parser without ParserSettings.ParseInputAsHtml = true */
</style>
<p>############### {TheVariable} ###############</p>
";
        var parsingFailures = 0;
        var parser = GetRegularParser(new SmartSettings
        {
            Parser = new ParserSettings { ErrorAction = ParseErrorAction.MaintainTokens, ParseInputAsHtml = inputIsHtml }
        });
        parser.OnParsingFailure += (o, args) => parsingFailures++;
        var parsed = parser.ParseFormat(styles);

        // No characters should get lost compared to the format string,
        // no matter if ParsingErrors occur or not
        Assert.That(parsed.Items.Sum(i => i.RawText.Length), Is.EqualTo(styles.Length), "No characters lost");

        if (shouldFail)
        {
            Assert.Multiple(() =>
            {
                Assert.That(parsingFailures, Is.GreaterThan(0));
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(0),
                    "NO placeholder");
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(parsingFailures, Is.EqualTo(0));
                Assert.That(parsed.Items.Count(i => i.GetType() == typeof(Placeholder)), Is.EqualTo(1),
                    "One placeholders");
                Assert.That(parsed.Items.First(i => i.GetType() == typeof(Placeholder)).RawText,
                    Is.EqualTo("{TheVariable}"));
            });
        }
    }

    [Test]
    public void ParsingErrors_Serialization()
    {
        using var stream = new MemoryStream();
        var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(ParsingErrors));
        ser.WriteObject(stream, new ParsingErrors());
        stream.Position = 0;

        var exception = ser.ReadObject(stream) as ParsingErrors;
        Assert.That(exception, Is.TypeOf<ParsingErrors>());
    }

    [Test]
    public void Initialize_Format()
    {
#pragma warning disable 618
        Assert.That(() => { _ = new Format().Initialize(new SmartSettings(), string.Empty, 0, 0, false); },
            Throws.Nothing, "Overload is marked as obsolete");
#pragma warning restore 618
    }
}
