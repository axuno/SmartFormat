﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
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
				"1/1/2000|10/10/2010|5/5/5555".Split('|').Select(s=>DateTime.ParseExact(s,"M/d/yyyy",CultureInfo.CreateSpecificCulture("en-us"))),
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
		/* added due to problems with [ThreadStatic] see: https://github.com/scottrippey/SmartFormat.NET/pull/23,
		 * if this test doesn't fail with usage of [ThreadStatic], run it again, because this test depends on the underlying ThreadPool */
		public void WithThreadPool_ShouldNotMixupCollectionIndex()
		{
			const string format = "{wheres.Count::>0? where |}{wheres:{}| and }";

			List<string> wheres = new List<string>(){"test = @test"};

			List<Task<string>> tasks = new List<Task<string>>();
			for (int i = 0; i < 10; ++i)
			{
				tasks.Add(Task.Factory.StartNew(val =>
				{
					Thread.Sleep(5 * (int)val);
					string ret = Smart.Default.Format(format, new {wheres});
					Thread.Sleep(5 * (int)val); /* add some delay to force ThreadPool swapping */
					return ret;
				}, i));
			}

			foreach (Task<string> t in tasks)
			{
				Assert.AreEqual(" where test = @test", t.Result);
			}
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

        [Test]
        public void EnumerableTest()
        {
            var data = new[]{
				new {
					Name = "Person A",
					Gender = "M"
				},
				new {
					Name = "Person B",
					Gender = "F"
				},
				new {
					Name = "Person C",
					Gender = "M"
				}
		    };
            var model = new
            {
                Persons = data.Where(p => p.Gender == "M")  // Persons's type is System.Linq.Enumerable+WhereArrayIterator`1[<>f__AnonymousType0`2[System.String,System.String]]
            };

            Assert.That(Smart.Format("{Persons.0.Name}", model), Is.EqualTo("Person A")); 
        }
	}
}
