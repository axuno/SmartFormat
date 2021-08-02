using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class SmartExtensionsTests
    {
        #region : StringBuilderTests :

        public static object[] GetArgs()
        {
            return new object[] {
                TimeSpan.Zero,
                new TimeSpan(1,1,1,1,1),
                new TimeSpan(0,2,0,2,0),
                new TimeSpan(3,0,0,3,0),
                new TimeSpan(0,0,0,0,4),
                new TimeSpan(5,0,0,0,0),
            };
        }

        [Test]
        public void Test_AppendLine()
        {
            var formats = new[]
            {
                "{0:time:}",
                "{1:time:}",
                "{2:time:}",
                "{3:time:}",
                "{4:time:}",
                "{5:time:}"
            };
            var expected = new[]
            {
                "less than 1 second" + Environment.NewLine,
                "1 day 1 hour 1 minute 1 second" + Environment.NewLine,
                "2 hours 2 seconds" + Environment.NewLine,
                "3 days 3 seconds" + Environment.NewLine,
                "less than 1 second" + Environment.NewLine,
                "5 days" + Environment.NewLine
            };
            var args = GetArgs();

            TestAppendLine(formats, args, expected);
        }

        [Test]
        public void Test_Append()
        {
            var formats = new [] {
                "{0:time:noless}",
                "{1:time:hours}",
                "{1:time:hours minutes}",
                "{2:time:days milliseconds}",
                "{2:time:days milliseconds auto}",
                "{2:time:days milliseconds short}",
                "{2:time:days milliseconds fill}",
                "{2:time:days milliseconds full}",
                "{3:time:abbr}",
            };
            var expected = new [] {
                "0 seconds",
                "25 hours",
                "25 hours 1 minute",
                "2 hours 2 seconds",
                "2 hours 2 seconds",
                "2 hours",
                "2 hours 0 minutes 2 seconds 0 milliseconds",
                "0 days 2 hours 0 minutes 2 seconds 0 milliseconds",
                "3d 3s",
            };
            var args = GetArgs();

            TestAppend(formats, args, expected);
        }

        public static void TestAppend(string[] bunchOfFormat, object[] args, string[] bunchOfExpected)
        {
            var allErrors = new ExceptionCollection();

            var numberOfTests = Math.Max(bunchOfFormat.Length, bunchOfExpected.Length);
            for (int i = 0; i < numberOfTests; i++)
            {
                var format = bunchOfFormat[i % bunchOfFormat.Length];
                var expected = bunchOfExpected[i % bunchOfExpected.Length];

                string actual = string.Empty;

                try
                {
                    var builder = new StringBuilder();
                    builder.AppendSmart(format, args);

                    actual = builder.ToString();

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

        public static void TestAppendLine(string[] bunchOfFormat, object[] args, string[] bunchOfExpected)
        {
            var allErrors = new ExceptionCollection();

            var numberOfTests = Math.Max(bunchOfFormat.Length, bunchOfExpected.Length);
            for (int i = 0; i < numberOfTests; i++)
            {
                var format = bunchOfFormat[i % bunchOfFormat.Length];
                var expected = bunchOfExpected[i % bunchOfExpected.Length];

                string? actual = null;

                try
                {
                    var builder = new StringBuilder();
                    builder.AppendLineSmart(format, args);

                    actual = builder.ToString();

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

        #endregion

        #region : TextWriterTests :

        [Test]
        public void WriteSmartTest()
        {
            var sb = new StringBuilder();
            var text = "abc";
            var fmt = "{0}";
            var sw = new StringWriter(sb);
            sw.WriteSmart("{0}", text);
            sw.Flush();
            sw.Close();
            Assert.AreEqual(string.Format(fmt, text), sb.ToString());
        }

        [Test]
        public void WriteLineSmartTest()
        {
            var sb = new StringBuilder();
            var text = "abc";
            var fmt = "{0}";
            var sw = new StringWriter(sb);
            sw.WriteLineSmart("{0}", text);
            sw.Flush();
            sw.Close();
            Assert.AreEqual(string.Format(fmt, text) + Environment.NewLine, sb.ToString());
        }

        #endregion

        #region : StringExtensionTests :

        [Test]
        public void StringFormatSmartTest()
        {
            var text = "abc";
            var fmt = "{0}";
            var result = fmt.FormatSmart(text);

            Assert.AreEqual(string.Format(fmt, text), result);
        }

        #endregion

        #region : StringOutputTests :

        [Test]
        public void StringOutputTest()
        {
            var so = new StringOutput();
            so.Write("text", 0, 2, null!);
            Assert.AreEqual("te", so.ToString());
        }

        #endregion
    }
}
