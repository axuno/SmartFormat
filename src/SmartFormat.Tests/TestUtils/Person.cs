using System;
using System.Collections.Generic;

namespace SmartFormat.Tests.TestUtils;

public class Person
{
    public Person()
    {
        Friends = new List<Person>();
    }
    public Person(string newName, Gender gender, DateTime newBirthday, string newAddress, params Person[] newFriends)
    {
        FullName = newName;
        Gender = gender;
        Birthday = newBirthday;
        if (!string.IsNullOrEmpty(newAddress))
            Address = Address.Parse(newAddress);
        Friends = new List<Person>(newFriends);
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FullName {
        get {
            if (string.IsNullOrEmpty(MiddleName)) {
                return FirstName + " " + LastName;
            } else {
                return FirstName + " " + MiddleName + " " + LastName;
            }
        }
        set {
            string[] names = value.Split(' ');
            FirstName = names[0];
            if (names.Length == 2) {
                LastName = names[1];
            } else if (names.Length == 3) {
                MiddleName = names[1];
                LastName = names[2];
            } else {
                MiddleName = names[1];
                LastName = string.Join(" ", names, 2, names.Length - 2);
            }
        }
    }
    public string Name => FirstName + " " + LastName;

    public DateTime Birthday { get; set; }
    public int Age
    {
        get
        {
            if (Birthday.Month < DateTime.Now.Month || (Birthday.Month == DateTime.Now.Month && Birthday.Day <= DateTime.Now.Day))
            {
                return DateTime.Now.Year - Birthday.Year;
            }
            else
            {
                return DateTime.Now.Year - 1 - Birthday.Year;
            }
        }
    }
    public Address? Address { get; set; }

    public List<Person> Friends { get; set; }
    public int NumberOfFriends => Friends.Count;

    public override string ToString()
    {
        return LastName + ", " + FirstName;
    }

    public Person? Spouse { get; set; }

    public Gender Gender { get; set; }
}

public enum Gender
{
    Male = 0,
    Female = 1,
}