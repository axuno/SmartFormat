using System;
using System.Text;
using NUnit.Framework;
using SmartFormat.Tests.Common;

namespace SmartFormat.Tests.Extensions
{
	[TestFixture]
	public class StringBuilderTests
	{
		public object[] GetArgs()
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
			var formats = new string[] {
				"{0}",
				"{1}",
				"{2}",
				"{3}",
				"{4}",
				"{5}",
			};
			var expected = new string[] {
				"less than 1 second"+Environment.NewLine,
				"1 day 1 hour 1 minute 1 second"+Environment.NewLine,
				"2 hours 2 seconds"+Environment.NewLine,
				"3 days 3 seconds"+Environment.NewLine,
				"less than 1 second"+Environment.NewLine,
				"5 days"+Environment.NewLine,
			};
			var args = GetArgs();

			TestAppendLine(formats, args, expected);
		}

		[Test]
		public void Test_Append()
		{
			var formats = new string[] {
				"{0:noless}",
				"{1:hours}",
				"{1:hours minutes}",
				"{2:days milliseconds}",
				"{2:days milliseconds auto}",
				"{2:days milliseconds short}",
				"{2:days milliseconds fill}",
				"{2:days milliseconds full}",
				"{3:abbr}",
			};
			var expected = new string[] {
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

				string actual = null;

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

				string actual = null;

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

	}
}
