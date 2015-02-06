using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Tests.Core
{
	[TestFixture]
	public class NestingTests
	{
		private Address address = new Address("123 Main St", "San Diego", "CA", "92000");

		[Test]
		public void Simple_Nesting()
		{
			var actual = Smart.Format("{Address:{StreetAddress}, {City}, {StateAbbreviation} {Zip}}", new { Address = address });
			Assert.AreEqual("123 Main St, San Diego, CA 92000", actual);
		}
		[Test]
		public void Nesting_With_Root()
		{
			var actual = Smart.Format("{Address1:{City}, {0.Address2.City}}", new {
				Address1 = address,
				Address2 = address
			});
			Assert.AreEqual("San Diego, San Diego", actual);
		}
	}
}
