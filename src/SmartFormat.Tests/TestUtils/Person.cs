using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SmartFormat.Tests
{
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
                if (string.IsNullOrEmpty(this.MiddleName)) {
                    return this.FirstName + " " + this.LastName;
                } else {
                    return this.FirstName + " " + this.MiddleName + " " + this.LastName;
                }
            }
            set {
                string[] names = value.Split(' ');
                this.FirstName = names[0];
                if (names.Length == 2) {
                    this.LastName = names[1];
                } else if (names.Length == 3) {
                    this.MiddleName = names[1];
                    this.LastName = names[2];
                } else {
                    this.MiddleName = names[1];
                    this.LastName = string.Join(" ", names, 2, names.Length - 2);
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
        public int NumberOfFriends => this.Friends.Count;

        public override string ToString()
        {
            return LastName + ", " + FirstName;
        }

        public Person? Spouse { get; set; }

        public Gender Gender { get; set; }

        public int Random()
        {
            return DateTime.Now.Second % 3;
        }
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
    }
}