using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core
{
	[TestFixture]
	public class NamedFormatterTests_Custom
	{
		#region: Default Extensions :

		[Test]
		[TestCase("{0:conditional:zero|one|two}", 0, "zero")]
		[TestCase("{0:conditional:zero|one|two}", 1, "one")]
		[TestCase("{0:conditional:zero|one|two}", 2, "two")]
		[TestCase("{0:cond:zero|one|two}", 0, "zero")]
		[TestCase("{0:cond:zero|one|two}", 1, "one")]
		[TestCase("{0:cond:zero|one|two}", 2, "two")]
		
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
		[TestCase("{0:test1():}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0:test1:N2}", 5, "TestExtension1 Options: , Format: N2")]
		[TestCase("{0:test1():N2}", 5, "TestExtension1 Options: , Format: N2")]
		[TestCase("{0:test1(a,b,c):}", 5, "TestExtension1 Options: a,b,c, Format: ")]
		[TestCase("{0:test1(a,b,c):N2}", 5, "TestExtension1 Options: a,b,c, Format: N2")]

		[TestCase("{0:test2:}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:test2():}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:test2:N2}", 5, "TestExtension2 Options: , Format: N2")]
		[TestCase("{0:test2():N2}", 5, "TestExtension2 Options: , Format: N2")]
		[TestCase("{0:test2(a,b,c):}", 5, "TestExtension2 Options: a,b,c, Format: ")]
		[TestCase("{0:test2(a,b,c):N2}", 5, "TestExtension2 Options: a,b,c, Format: N2")]

		[TestCase("{0:default:}", 5, "5")]
		[TestCase("{0:default():}", 5, "5")]
		[TestCase("{0:default:N2}", 5, "5.00")]
		[TestCase("{0:default():N2}", 5, "5.00")]
		public void NamedFormatter_invokes_a_specific_formatter(string format, object arg0, string expectedResult)
		{
			var smart = GetCustomFormatter();
			var actualResult = smart.Format(format, arg0);
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		[TestCase("{0:invalid:}")]
		[TestCase("{0:invalid():}")]
		[TestCase("{0:invalid(___):___}")]
		[ExpectedException(typeof (FormattingException))]
		public void Unhandled_formats_throw(string format)
		{
			var smart = GetCustomFormatter();
			smart.Format(format, 99999);
		}

		[Test]
		[TestCase("{0:test2:}", 5, "TestExtension1 Options: , Format: ")]
		[TestCase("{0}", 5, "TestExtension2 Options: , Format: ")]
		[TestCase("{0:N2}", 5, "TestExtension2 Options: , Format: N2")]
		public void Implicit_formatters_require_an_empty_string(string format, object arg0, string expectedOutput)
		{
			var formatter = GetCustomFormatter();
			formatter.GetFormatterExtension<TestExtension1>().Names = new[] {"test2"};
			var actual = formatter.Format(format, arg0);
			Assert.AreEqual(expectedOutput, actual);
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
			private string[] names = { "test1", "t1", "" };
			public string[] Names { get { return names; } set { names = value; } }

			public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
			{
				var options = formattingInfo.FormatterOptions;
				var format = formattingInfo.Format;
				var formatString = format != null ? format.ToString() : "";
				formattingInfo.Write("TestExtension1 Options: " + options + ", Format: " + formatString);
				return true;
			}
		}
		private class TestExtension2 : IFormatter
		{
			private string[] names = { "test2", "t2", "" };
			public string[] Names { get { return names; } set { names = value; } }

			public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
			{
				var options = formattingInfo.FormatterOptions;
				var format = formattingInfo.Format;
				var formatString = format != null ? format.ToString() : "";
				formattingInfo.Write("TestExtension2 Options: " + options + ", Format: " + formatString);
				return true;
			}

		}

		#endregion

	}

}