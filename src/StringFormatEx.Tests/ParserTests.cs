using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StringFormatEx.Core.Parsing;
using Common;

namespace StringFormatEx.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestParser()
        {
            var parser = new Parser();

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
        public void Test_Escaping()
        {
            var parser = new Parser();

            var tests = new[]{
                new { Title = "Test Opening Braces",
                      Format = "{{0}} - {{{0}}}",
                      Expected = "{0} - {Zero}",
                      Actual = new StringBuilder(),
                },
                new { Title = "Test Closing Braces",
                      Format = "{1:N0} }} - {1:N0}0}} - {1:N0}}0} - {1:N0}}}",
                      Expected = "1 } - 10} - N0}1 - N1}",
                      Actual = new StringBuilder(),
                },
            };
            var args = new object[] { "Zero", 1, 2.22M, new DateTime(2003, 3, 3), "Four".ToCharArray(), new TimeSpan(0, 5, 0, 5, 0) };


            foreach (var test in tests)
            {
                var parsed = parser.ParseFormat(test.Format);
                Console.WriteLine(parsed.ToString());
            }


            // Process all items:
            var errors = tests.TryAll(t => t.Actual.Append( string.Format( t.Format, args )));
            
            // Check all results:
            var moreErrors = tests.TryAll(t => Assert.AreEqual(t.Expected, t.Actual.ToString()));

            errors.AddRange(moreErrors);
            errors.ThrowIfNotEmpty();

        }

        [Test]
        public void Test_Splicing()
        {
            var parser = new Parser();
            parser.AddWatchedChar('|');
            var format = " a|aa {bbb.ccc: dd|d {eee} ff|f } gg|g ";
            var Format = parser.ParseFormat(format);
            Format = ((Placeholder)Format.Items[1]).Format;

            var allSplits = Format.GetWatchedCharacters('|');

            var allSplices = new List<Format>();
            for (int i = -1; i < allSplits.Count; i++)
            {
                Format splice;
                if (i == -1) {
                    splice = Format.Substring(Format.startIndex, allSplits[0]);
                } else if (i == allSplits.Count - 1) {
                    splice = Format.Substring(allSplits[i] + 1, Format.endIndex);
                } else {
                    splice = Format.Substring(allSplits[i] + 1, allSplits[i+1]);
                }
                allSplices.Add(splice);
            }

            allSplices.ForEach(Console.WriteLine);

        }
    }
}
