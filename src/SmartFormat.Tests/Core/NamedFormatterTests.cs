using System;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core
{
	[TestFixture]
	public class NamedFormatterTests_Custom
	{
		#region: Default Extensions :

		[Test]
		[TestCase("{0:choose:zero|one|two}", 0, "zero")]
		[TestCase("{0:choose:zero|one|two}", 1, "one")]
		[TestCase("{0:choose:zero|one|two}", 2, "two")]
		[TestCase("{0:c:zero|one|two}", 0, "zero")]
		[TestCase("{0:c:zero|one|two}", 1, "one")]
		[TestCase("{0:c:zero|one|two}", 2, "two")]
		
		[TestCase("{0:plural:one|many}", 1, "one")]
		[TestCase("{0:plural:one|many}", 2, "many")]
		[TestCase("{0:p:one|many}", 1, "one")]
		[TestCase("{0:p:one|many}", 2, "many")]

		[TestCase("{0:list:+{}|, |, and }", new []{ 1, 2, 3 }, "+1, +2, and +3")]
		[TestCase("{0:l:+{}|, |, and }", new []{ 1, 2, 3 }, "+1, +2, and +3")]
		
		[TestCase("{0:default()}", 5, "5")]
		[TestCase("{0:default:N2}", 5, "5.00")]

		public void Invoke_extensions_by_name_or_shortname(string format, object arg0, string expectedResult)
		{
			var actualResult = Smart.Format(format, arg0);
			Assert.AreEqual(expectedResult, actualResult);
		}

		#endregion

		#region: Custom Extensions :

		[Test]
		[TestCase("{0}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0:N2}", 5, "TestExtension1 Options: , Format: N2")]
		public void Without_NamedFormatter_extensions_are_invoked_in_order(string format, object arg0, string expectedResult)
		{
			var smart = GetCustomFormatter();
			var actualResult = smart.Format(format, arg0);
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		[TestCase("{0:test1:}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0:test1()}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0:test1():}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0:test1:N2}", 5, "TestExtension1 Options: , Format: N2")]
		[TestCase("{0:test1():N2}", 5, "TestExtension1 Options: , Format: N2")]
		[TestCase("{0:test1(a,b,c):N2}", 5, "TestExtension1 Options: a,b,c, Format: N2")]
		[TestCase("{0:test1(a,b,c)}", 5, "TestExtension1 Options: a,b,c, Format: ")]

		[TestCase("{0:test2:}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:test2()}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:test2():}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:test2:N2}", 5, "TestExtension2 Options: , Format: N2")]
		[TestCase("{0:test2():N2}", 5, "TestExtension2 Options: , Format: N2")]
		[TestCase("{0:test2(a,b,c):N2}", 5, "TestExtension2 Options: a,b,c, Format: N2")]
		[TestCase("{0:test2(a,b,c)}", 5, "TestExtension2 Options: a,b,c, Format: ")]

		[TestCase("{0:default:}", 5, "5")]
		[TestCase("{0:default()}", 5, "5")]
		[TestCase("{0:default():}", 5, "5")]
		[TestCase("{0:default:N2}", 5, "5.00")]
		[TestCase("{0:default():N2}", 5, "5.00")]
		public void NamedFormatter_invokes_a_specific_formatter(string format, object arg0, string expectedResult)
		{
			var smart = GetCustomFormatter();
			var actualResult = smart.Format(format, arg0);
			Assert.AreEqual(expectedResult, actualResult);
		}

		private SmartFormatter GetCustomFormatter()
		{
			var testFormatter = new SmartFormatter();
			testFormatter.AddExtensions(new TestExtension1(), new TestExtension2(), new DefaultFormatter());
			testFormatter.AddExtensions(new DefaultSource(testFormatter));
			return testFormatter;
		}

		private class TestExtension1 : IFormatter
		{
            private string[] names = { "test1", "t1" };
            public string[] Names { get { return names; } set { names = value; } }

			public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
			{
				var options = formatDetails.FormatterOptions;
				var formatString = format != null ? format.ToString() : "";
				output.Write("TestExtension1 Options: " + options + ", Format: " + formatString, formatDetails);
				handled = true;
			}
		}
		private class TestExtension2 : IFormatter
		{
            private string[] names = { "test2", "t2" };
            public string[] Names { get { return names; } set { names = value; } }

            public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
			{
				var options = formatDetails.FormatterOptions;
				var formatString = format != null ? format.ToString() : "";
				output.Write("TestExtension2 Options: " + options + ", Format: " + formatString, formatDetails);
				handled = true;
			}

		}

		#endregion

	}

}