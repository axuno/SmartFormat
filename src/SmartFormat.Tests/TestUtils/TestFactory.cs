using System;
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
    }
}
