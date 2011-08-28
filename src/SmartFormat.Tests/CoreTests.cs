using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Tests.Common;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestParser()
        {
            var parser = new Parser();
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
        //[ExpectedException(typeof(ParsingErrors))]
        public void Parser_Throws_Exception()
        {
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
            var allErrors = new ExceptionCollection<ParsingErrors>();
            foreach (var format in invalidFormats)
            {
                allErrors.Try(()=>Smart.Default.Test(format, args, "Error"));
            }

            // Make sure that EVERY item had an error:
            Assert.AreEqual(invalidFormats.Length, allErrors.Count, "Not all items had exceptions!");
            
        }

        [Test]
        public void Test_Splicing()
        {
            var parser = new Parser();
            parser.AddAlphanumericSelectors();
            parser.AddOperators(".+");
            var format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
            var expected = new[] { " a", "aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg", "g " };

            var Format = parser.ParseFormat(format);
            var allSplices = new List<Format>();
            var startIndex = Format.startIndex;
            while (true)
            {
                var nextIndex = Format.IndexOf("|", startIndex);
                if (nextIndex == -1)
                {
                    allSplices.Add(Format.Substring(startIndex));
                    break;
                }
                else
                {
                    var splice = Format.Substring(startIndex, nextIndex);
                    allSplices.Add(splice);
                }
                startIndex = nextIndex + 1;
            }

            var actual = allSplices.Select(s => s.Text).ToArray();
            Assert.AreEqual(expected, actual);

        }

    }
}
