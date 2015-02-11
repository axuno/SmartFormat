using System;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Core
{
	[TestFixture]
	public class FormatterTests
	{
		private object[] errorArgs = new object[]{ new FormatDelegate(format => { throw new Exception("ERROR!"); } ) };

		[Test]
		public void Formatter_Throws_Exceptions()
		{
			var formatter = Smart.CreateDefaultSmartFormat();
			formatter.ErrorAction = ErrorAction.ThrowError;

			try
			{
				formatter.Test("--{0}--", errorArgs, "--ERROR!--ERROR!--");
				Assert.Fail("Formatter should have thrown an exception, but did not.");
			}
			catch (FormattingException ex)
			{
			}
		}

		[Test]
		public void Formatter_Outputs_Exceptions()
		{
			var formatter = Smart.CreateDefaultSmartFormat();
			formatter.ErrorAction = ErrorAction.OutputErrorInResult;

			formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--ERROR!--ERROR!--");
		}

		[Test]
		public void Formatter_Ignores_Exceptions()
		{
			var formatter = Smart.CreateDefaultSmartFormat();
			formatter.ErrorAction = ErrorAction.Ignore;

			formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "------");
		}

		[Test]
		public void Formatter_Maintains_Tokens()
		{
			var formatter = Smart.CreateDefaultSmartFormat();
			formatter.ErrorAction = ErrorAction.MaintainTokens;

			formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--{0}--{0:ZZZZ}--");
		}

		[Test]
		public void Formatter_Maintains_Object_Tokens()
		{
			var formatter = Smart.CreateDefaultSmartFormat();
			formatter.ErrorAction = ErrorAction.MaintainTokens;
			formatter.Test("--{Object.Thing}--", errorArgs, "--{Object.Thing}--");
		}

		[Test]
		public void Formatter_AlignNull()
		{
			string name = null;
			var obj = new { name = name };
			var str2 = Smart.Format("Name: {name,-10}| Column 2", obj);
			Assert.That(str2, Is.EqualTo("Name:           | Column 2"));
		}
	}
}