using System;
using System.Collections.Generic;

using SmartFormat.Tests;

namespace SmartFormat.Tests
{
	public abstract class TestFactory
	{
		public static Person GetPerson()
		{
			var michael = new Person("Michael Scott", Gender.Male, new DateTime(1970, 3, 3), "333 Third St, Scranton, PA 18447");
			var jim = new Person("Jim Halpert", Gender.Male, new DateTime(1979, 1, 1), "111 First St, Scranton, PA 18447");
			var pam = new Person("Pam Halpert", Gender.Female, new DateTime(1980, 1, 1), "111 First St, Scranton, PA 18447");
			var dwight = new Person("Dwight K Schrute", Gender.Male, new DateTime(1978, 2, 2), "222 Second St, Scranton, PA 18447");

			michael.Friends.Add(jim);
			michael.Friends.Add(pam);
			michael.Friends.Add(dwight);
			jim.Spouse = pam;
			jim.Friends.Add(dwight);
			jim.Friends.Add(michael);
			pam.Spouse = jim;
			pam.Friends.Add(dwight);
			pam.Friends.Add(michael);
			dwight.Friends.Add(michael);

			return michael;
		}

		public static List<object> GetItems()
		{
			return new List<object>
				       {
					       new
						       {
							       Name = "Table",
							       Price = 119.99,
							       Components = new List<object> { new { Name = "Screws", Count = 25 }, new { Name = "Pillow", Count = 1 } }
						       },
					       new
						       {
							       Name = "Chair",
							       Price = 49,
							       Components = new List<object> { new { Name = "Screws", Count = 12 }, new { Name = "Leg", Count = 4 } }
						       }
				       };
		}
	}
}
