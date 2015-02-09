using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Core.Parsing;
using SmartFormat.Tests.Common;

namespace SmartFormat.Tests.Core
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

		[Test]
		public void Parser_UseAlternativeBraces()
		{
			var parser = GetRegularParser();
			parser.UseAlternativeBraces('[', ']');
			var format = "aaa [bbb] [ccc:ddd] {eee} [fff:{ggg}] [hhh:[iii:[] ] ] jjj";
			var parsed = parser.ParseFormat(format);

			Assert.AreEqual(9, parsed.Items.Count);

			Assert.AreEqual("bbb", ((Placeholder)parsed.Items[1]).Selectors[0].Text);
			Assert.AreEqual("ccc", ((Placeholder)parsed.Items[3]).Selectors[0].Text);
			Assert.AreEqual("ddd", ((Placeholder)parsed.Items[3]).Format.Items[0].Text);
			Assert.AreEqual(" {eee} ", ((LiteralText)parsed.Items[4]).Text);
			Assert.AreEqual("{ggg}", ((Placeholder)parsed.Items[5]).Format.Items[0].Text);
			Assert.AreEqual("iii", ((Placeholder)((Placeholder)parsed.Items[7]).Format.Items[0]).Selectors[0].Text);

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

		private Format Parse(string format)
		{
			return GetRegularParser().ParseFormat(format);
		}

		[Test]
		[TestCase("{0:name:format}", "name", "", "format")]
		[TestCase("{0:name():format}", "name", "", "format")]
		[TestCase("{0:name:}", "name", "", "")]
		[TestCase("{0:name():}", "name", "", "")]
		[TestCase("{0:name(1,2,3):format}", "name", "1,2,3", "format")]
		[TestCase("{0:name(1,2,3):}", "name", "1,2,3", "")]
		public void NamedFormatter_should_be_parsed_correctly(string format, string expectedName, string expectedOptions, string expectedFormat)
		{
			var placeholder = (Placeholder) Parse(format).Items[0];
			Assert.AreEqual(expectedName, placeholder.FormatterName);
			Assert.AreEqual(expectedOptions, placeholder.FormatterOptions);
			Assert.AreEqual(expectedFormat, placeholder.Format.ToString());
		}

		[Test]
		// Incomplete:
		[TestCase(@"{0:format}")]
		[TestCase(@"{0:format(}")]
		[TestCase(@"{0:format)}")]
		[TestCase(@"{0:(format)}")]
		// Escape sequences:
		[TestCase(@"{0:format\()}")]
		[TestCase(@"{0:format(\)}")]
		[TestCase(@"{0:format\:}")]
		[TestCase(@"{0:hh\:mm\:ss}")]
		// Has nesting:
		[TestCase(@"{0:format{}}")]
		// Empty:
		[TestCase(@"{0}")]
		[TestCase(@"{0:}")]
		[TestCase(@"{0::}")]
		[TestCase(@"{0:()}")]
		[TestCase(@"{0:():}")]
		[TestCase(@"{0:(1,2,3)}")]
		[TestCase(@"{0:(1,2,3):}")]
		public void NamedFormatter_should_be_null_when_empty_or_invalid_or_escaped(string format)
		{
			var parser = GetRegularParser();
			parser.UseAlternativeEscapeChar('\\');
			
			var placeholder = (Placeholder)parser.ParseFormat(format).Items[0];
			Assert.IsEmpty(placeholder.FormatterName);
			Assert.IsEmpty(placeholder.FormatterOptions);
		}

	}
}
