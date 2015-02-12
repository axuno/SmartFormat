using System;
using System.Drawing.Text;
using NUnit.Framework;

namespace SmartFormat.Tests.Core
{
	[TestFixture]
	public class NestingTests
	{
		public NestingTests()
		{
			var data = new {
				Address1 = new Address("123 Main St", "San Diego", "CA", "92000"),
				Address2 = new Address("987 Second St", "Los Angeles", "CA", "90210"),
				Person1 = new Person("Dwight Schrute", Gender.Male, new DateTime(), ""),
				Person2 = new Person("Michael Scott", Gender.Male, new DateTime(), ""),
			};
			data.Person1.Address = data.Address1;
			data.Person2.Address = data.Address2;

			this.data = data;
		}
		private object data;

		[Test]
		[TestCase("{Address1:{StreetAddress}, {City}, {StateAbbreviation} {Zip}}", "123 Main St, San Diego, CA 92000")]
		[TestCase("{Address1:{City}, {0.Address2.City}}", "San Diego, Los Angeles")]
		public void Nesting_can_access_outer_scope_via_number(string format, string expectedOutput)
		{
			var actual = Smart.Format(format, data);
			Assert.AreEqual(expectedOutput, actual);
		}
	}
}
