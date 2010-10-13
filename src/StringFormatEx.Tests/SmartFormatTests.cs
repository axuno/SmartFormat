using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using NUnit.Framework;
using StringFormatEx.Core;
using StringFormatEx.Core.Plugins;
using System.Diagnostics;
using StringFormatEx.Core.Parsing;

namespace StringFormatEx.Tests
{
    [TestFixture]
    public class SmartFormatTests
    {
        [Test]
        public void Test_Smart_Format()
        {
            var tests = new[]{
                new{ Title = "Test 0",
                     Format = "{0}",
                     Expected = "Zero",
                     Actual = new StringBuilder() // (anon props are readonly, so we'll use a stringbuilder for output)
                },
                new{ Title = "Test 1",
                     Format = "{1}",
                     Expected = "1",
                     Actual = new StringBuilder()
                },
                new{ Title = "Test 2 - Format",
                     Format = "{2:N3}",
                     Expected = "2.000",
                     Actual = new StringBuilder()
                },
                new{ Title = "Test 3 - Escaping",
                     Format = "{{{0}}} {{0}} {1:N0}}0}",
                     Expected = "{Zero} {0} N0}1",
                     Actual = new StringBuilder()
                },
                new{ Title = "Test 4 - Array (no plugins)",
                     Format = "{4}",
                     Expected = "System.Char[]",
                     Actual = new StringBuilder()
                },
                new{ Title = "Test 5 - Date format",
                     Format = "{5:ddd, MMMM d, yyyy}",
                     Expected = "Thu, May 5, 5555",
                     Actual = new StringBuilder()
                },
            };

            // Create some testing arguments:
            var args = new object[] { "Zero", 1, 2, "333", "Four".ToCharArray(), new DateTime(5555, 5, 5, 5, 5, 5), new TimeSpan(6, 6, 6, 6) };

            // Get all the results:
            var runtimeErrors = tests.TryAll(t => t.Actual.Append(Smart.Format(t.Format, args)));

            var resultErrors = tests.TryAll(t => Assert.AreEqual(t.Expected, t.Actual.ToString(),
                "Result failed for \"{0}\".", t.Title
                ));

            // Throw the errors:
            ExceptionCollection.Combine(runtimeErrors, resultErrors).ThrowIfNotEmpty();
        }

        [Test]
        public void PerformanceTest_ComparedTo_StringFormat()
        {
            // Create the most basic formatter:
            var Smart = new SmartFormat();
            Smart.AddFormatterPlugins(new DefaultFormatter());
            Smart.AddSourcePlugins(new DefaultSource());

            // Setup the test criteria:
            var tests = new[]{
                new { title = "Test - Very simple",
                      format = "Zero: '{0}'",
                      expected = "Zero: 'Zero'",
                },
                new { title = "Test - Format a number",
                      format = "One: {1:N2}",
                      expected = "One: 1.00",
                },
                new { title = "Test - Format a date",
                      format = "Two: {2:d}",
                      expected = "Two: 10/10/2010",
                },
                new { title = "Test - All Three",
                      format = "Zero: '{0}' One: {1:N2} Two: {2:d}",
                      expected = "Zero: 'Zero' One: 1.00 Two: 10/10/2010",
                },
                //new { title = "Test - Just Escapes",
                //      format = "Zero: {{0}} One: {{1}}",
                //      expected = "Zero: {0} One: {1}",
                //},
            };
            var args = new object[] { "Zero", 1, new DateTime(2010, 10, 10) };

            foreach (var test in tests)
            {
                // We'll compare String.Format to Smart.Format to a cached Smart.Format:
                Format cached = null;

                // Do a quick warm-up and output check:
                var stringActual = String.Format(test.format, args);
                var smartActual = Smart.Format(test.format, args);
                var cachedActual = Smart.FormatCache(ref cached, test.format, args);
                var oldActual = ExtendedStringFormatter.Default.FormatEx(test.format, args);
                Assert.AreEqual(test.expected, stringActual);
                Assert.AreEqual(test.expected, smartActual);
                Assert.AreEqual(test.expected, cachedActual);
                Assert.AreEqual(test.expected, oldActual);

                const int iterations = 1000000;

                string discard;

                // Performance for String.Format:
                var stringTimer = new Stopwatch();
                stringTimer.Start();
                for (int i = 0; i < iterations; i++)
                {
                    discard = String.Format(test.format, args);
                }
                stringTimer.Stop();

                // Performance for Smart.Format:
                var smartTimer = new Stopwatch();
                smartTimer.Start();
                for (int i = 0; i < iterations; i++)
                {
                    discard = Smart.Format(test.format, args);
                }
                smartTimer.Stop();

                // Performance for cached Smart.Format:
                var cachedTimer = new Stopwatch();
                cachedTimer.Start();
                for (int i = 0; i < iterations; i++)
                {
                    discard = Smart.FormatCache(ref cached, test.format, args);
                }
                cachedTimer.Stop();

                // Performance for old Smart.Format:
                var oldTimer = new Stopwatch();
                oldTimer.Start();
                for (int i = 0; i < iterations; i++)
                {
                    discard = ExtendedStringFormatter.Default.FormatEx(test.format, args);
                }
                oldTimer.Stop();


                // Compare the results:
                Console.WriteLine("Results for {0} - \"{1}\" => \"{2}\"", test.title, test.format, test.expected);
                Console.WriteLine("String.Format results: {0:N2} s taken ({1:N1} ns per iteration)", stringTimer.Elapsed.TotalSeconds, stringTimer.Elapsed.TotalMilliseconds * 1000 / iterations);
                Console.WriteLine("Smart.Format results: {0:N2} s taken ({1:N1} ns per iteration)", smartTimer.Elapsed.TotalSeconds, smartTimer.Elapsed.TotalMilliseconds * 1000 / iterations);
                Console.WriteLine("Cached Smart.Format results: {0:N2} s taken ({1:N1} ns per iteration)", cachedTimer.Elapsed.TotalSeconds, cachedTimer.Elapsed.TotalMilliseconds * 1000 / iterations);
                Console.WriteLine("Old Format results: {0:N2} s taken ({1:N1} ns per iteration)", oldTimer.Elapsed.TotalSeconds, oldTimer.Elapsed.TotalMilliseconds * 1000 / iterations);
                var ratioStringSmart = smartTimer.Elapsed.TotalMilliseconds / stringTimer.Elapsed.TotalMilliseconds;
                var ratioStringCached = cachedTimer.Elapsed.TotalMilliseconds / stringTimer.Elapsed.TotalMilliseconds;
                var ratioStringOld = oldTimer.Elapsed.TotalMilliseconds / stringTimer.Elapsed.TotalMilliseconds;
                Console.WriteLine("Ratio of String:Smart is 1:{0:N2}, String:Cached is 1:{1:N2}, String:Old is 1:{2:N2}", ratioStringSmart, ratioStringCached, ratioStringOld);
                Console.WriteLine();
            }
        }
    }
}
