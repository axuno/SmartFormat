using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartFormat.Tests.Common;

namespace SmartFormat.Tests
{
	[TestFixture]
	public class ListFormatterTests
	{
		public object[] GetArgs()
		{
			var args = new object[] {
                "ABCDE".ToCharArray(),
                "One|Two|Three|Four|Five".Split('|'),
                TestFactory.GetPerson().Friends,
                "1/1/2000|10/10/2010|5/5/5555".Split('|').Select(s=>DateTime.ParseExact(s,"M/d/yyyy",System.Globalization.CultureInfo.CreateSpecificCulture("en-us"))),
                new []{1,2,3,4,5},
            };
			return args;
		}

		[Test]
		public void FormatTest()
		{
			var formats = new string[] {
                "{4}",
                "{4:|}",
                "{4:00|}",
                "{4:|,}",
                "{4:|, |, and }",
                "{4:N2|, |, and }",
            };
			var expected = new string[] {
                "System.Int32[]",
                "12345",
                "0102030405",
                "1,2,3,4,5",
                "1, 2, 3, 4, and 5",
                "1.00, 2.00, 3.00, 4.00, and 5.00",
            };

			var args = GetArgs();
			Smart.Default.Test(formats, args, expected);

		}
		[Test]
		public void NestedFormatTest()
		{
			var formats = new string[] {
                "{0:{}-|}",
                "{0:{}|-}",
                "{0:{}|-|+}",
                "{0:({})|, |, and }",
            };
			var expected = new string[] {
                "A-B-C-D-E-",
                "A-B-C-D-E",
                "A-B-C-D+E",
                "(A), (B), (C), (D), and (E)",
            };

			var args = GetArgs();
			Smart.Default.Test(formats, args, expected);
		}
		[Test]
		public void NestedArraysTest()
		{
			var formats = new string[] {
                "{2:{:{FirstName}}|, }",
                "{3:{:M/d/yyyy} |}",
                "{2:{:{FirstName}'s friends: {Friends:{FirstName}|, } }|; }",
            };
			var expected = new string[] {
                "Jim, Pam, Dwight",
                "1/1/2000 10/10/2010 5/5/5555 ",
                "Jim's friends: Dwight, Michael ; Pam's friends: Dwight, Michael ; Dwight's friends: Michael ",
            };

			var args = GetArgs();
			Smart.Default.Test(formats, args, expected);
		}
		[Test]
		public void TestIndex()
		{
			var formats = new string[] {
                "{0:{} = {Index}|, }", // Index holds the current index of the iteration
                "{1:{Index}: {ToCharArray:{} = {Index}|, }|; }", // Index can be nested
                "{0:{} = {1.Index}|, }", // Index is used to synchronize 2 lists
                "{Index}", // Index can be used out-of-context, but should always be -1
            };
			var expected = new string[] {
                "A = 0, B = 1, C = 2, D = 3, E = 4",
                "0: O = 0, n = 1, e = 2; 1: T = 0, w = 1, o = 2; 2: T = 0, h = 1, r = 2, e = 3, e = 4; 3: F = 0, o = 1, u = 2, r = 3; 4: F = 0, i = 1, v = 2, e = 3",
                "A = One, B = Two, C = Three, D = Four, E = Five",
                "-1",
                };

			var args = GetArgs();
			Smart.Default.Test(formats, args, expected);
		}
	}
}
