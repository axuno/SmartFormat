using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Tests.Common;

namespace SmartFormat.Tests
{
    [DebuggerNonUserCode]
    public static class TestHelpers
    {
        public static void Test(this SmartFormatter formatter, string format, object[] args, string expected)
        {

            string actual = null;
            try
            {
                actual = formatter.Format(format, args);
                Assert.AreEqual(expected, actual);
                Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: \"{0}\" => \"{1}\" - {2}", format, actual, ex.Message);
                throw;
            }
        }

        public static void Test(this SmartFormatter formatter, string format, object[][] bunchOfArgs, string[] bunchOfExpected)
        {
            var allErrors = new ExceptionCollection(); // We will defer all errors until the end.
            
            var numberOfTests = Math.Max(bunchOfArgs.Length, bunchOfExpected.Length);
            for (int i = 0; i < numberOfTests; i++)
            {
                var args = bunchOfArgs[i%bunchOfArgs.Length];
                var expected = bunchOfExpected[i%bunchOfExpected.Length];

                string actual = null;
                try
                {
                    actual = formatter.Format(format, args);
                    Assert.AreEqual(expected, actual);
                    Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: \"{0}\" => \"{1}\"", format, actual);
                    allErrors.Add(ex);
                }
            }

            allErrors.ThrowIfNotEmpty();
        }

        public static void Test(this SmartFormatter formatter, string[] bunchOfFormat, object[] args, string[] bunchOfExpected)
        {
            var allErrors = new ExceptionCollection();

            var numberOfTests = Math.Max(bunchOfFormat.Length, bunchOfExpected.Length);
            for (int i = 0; i < numberOfTests; i++)
            {
                var format = bunchOfFormat[i%bunchOfFormat.Length];
                var expected = bunchOfExpected[i%bunchOfExpected.Length];

                string actual = null;
                try
				{
					var specificCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-us");
					actual = formatter.Format(specificCulture, format, args);
                    Assert.AreEqual(expected, actual);
                    Console.WriteLine("Success: \"{0}\" => \"{1}\"", format, actual);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: \"{0}\" => \"{1}\"", format, actual);
                    allErrors.Add(ex);
                }
            }

            allErrors.ThrowIfNotEmpty();
        }

        public static TimeSpan[] PerformanceTest(Func<string, object[], string>[] doFormats, string format, object[] args, int iterations)
        {
            // Do a warmup and make sure output matches:
            string[] actuals = new string[doFormats.Length];
            for (int i = 0; i < doFormats.Length; i++)
            {
                actuals[i] = doFormats[i](format, args);
            }
            for (int i = 0; i < doFormats.Length - 1; i++)
            {
                Assert.AreEqual(actuals[i], actuals[i+1],"Results don't match.");
            }


            // Do all the performance tests:
            Stopwatch timer;
            string discard;
            TimeSpan[] results = new TimeSpan[doFormats.Length];
            for (int i = 0; i < doFormats.Length; i++)
            {
                var doFormat = doFormats[i];
                timer = new Stopwatch();
                timer.Start();
                for (int j = 0; j < iterations; j++)
                {
                    discard = doFormat(format, args);
                }
                timer.Stop();

                results[i] = timer.Elapsed;
            }
            //// Do one final (empty) control test:
            //timer = new Stopwatch();
            //timer.Start();
            //for (int j = 0; j < iterations; j++)
            //{
            //    discard = format;
            //}
            //timer.Stop();
            //results[doFormats.Length] = timer.Elapsed;


            // Return the results:
            return results;
        }
    }
}
