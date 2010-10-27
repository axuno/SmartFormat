using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Plugins;

namespace SmartFormat.Tests
{
    [TestFixture, Explicit]
    public class PerformanceTests
    {
        [Test, Explicit("This performance test takes like 45 seconds")]
        public void PerformanceTest_ComparedTo_StringFormat()
        {
            // Create the most basic formatter:
            //var Smart = new SmartFormatter();
            //Smart.AddPlugins(
            //    new DefaultFormatter(), 
            //    new DefaultSource()
            //);

            // Setup the test criteria:
            var tests = new[]{
                new { title = "Test - No Placeholders",
                      format = "No Placeholders",
                      expected = "No Placeholders",
                },
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
                new { title = "Test - Lengthy format string",
                      format = "Zero: '{0}'    One: {1:N2} / {1:000} / {1:###}    Two: {2:d} / {2:D} / {2:ddd, MMM d, ''yy} / {2:t}           Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ",
                      expected = "Zero: 'Zero'    One: 1.00 / 001 / 001    Two: 10/10/2010 / Sunday, October 10, 2010 / Sun, Oct 10, '10 / 12:00:00 am           Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ",
                },
            };
            var args = new object[] { "Zero", 1, new DateTime(2010, 10, 10) };


            FormatCache cache = null;
            FormatCache cache2 = null;
            var NoPlugins = new SmartFormatter();
            NoPlugins.AddPlugins(new DefaultFormatter(), new DefaultSource(NoPlugins));

            var formatters = new[] {
                new {
                    Title = "String.Format",
                    Function = new Func<string, object[], string>(string.Format),
                },
                new {
                    Title = "Smart.Format",
                    Function = new Func<string, object[], string>(Smart.Format),
                },
                new {
                    Title = "NoPlugins.Format",
                    Function = new Func<string, object[], string>(NoPlugins.Format),
                },
                new {
                    Title = "Smart.FormatWithCache",
                    Function = new Func<string, object[], string>((format, args2) => Smart.Default.FormatWithCache(ref cache, format, args2)),
                },
                new {
                    Title = "NoPlugins.FormatWithCache",
                    Function = new Func<string, object[], string>((format, args2) => NoPlugins.FormatWithCache(ref cache2, format, args2)),
                },
            };

            const int iterations = 100000;
            foreach (var test in tests)
            {
                cache = null;
                cache2 = null;
                var results = TestHelpers.PerformanceTest(formatters.Select(f => f.Function).ToArray(), test.format, args, iterations);

                // Compare the results:
                Console.WriteLine("{0} Results: \"{1}\" => \"{2}\"", test.title, test.format, test.expected);
                var baseSeconds = results[0].TotalSeconds;
                Console.WriteLine("Test Function        Ratio to String.Format  Actual time taken");
                for (int i = 0; i < formatters.Length; i++)
                {
                    var f = formatters[i];
                    var r = results[i];
                    Console.WriteLine("{0,-25}   1 : {3:N2}   {2:N1}µs per iteration {1:N2}s total)", f.Title, r.TotalSeconds, r.TotalSeconds * (double)1000000 / iterations, r.TotalSeconds / baseSeconds);
                }
                Console.WriteLine();
            }



        }
    }
}
