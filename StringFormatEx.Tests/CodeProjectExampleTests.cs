using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;



namespace StringFormatEx.Tests
{
    [TestFixture]
    public class CodeProjectExampleTests
    {
        private Person MakeQuentin()
        {
            var p = new Person() {
                                     Name = "Quentin",
                                     Age = 29,
                                     Friends = new List<Person>() {
                                                                      new Person() { Name = "John Smith", Age = 32},
                                                                      new Person() { Name = "Bob Johnson", Age = 53},
                                                                      new Person() { Name = "Mary Meyer", Age = 20},
                                                                      new Person() { Name = "Dr. Jamal Cornucopia", Age = 37}
                                                                  }
                                 };

            return p;
        }


        [Test]
        public void BasicSyntax()
        {
            var p = MakeQuentin();

            var formatString = "{0} is {1} years old and has {2:N2} friends.";
            var expectedOutput = "Quentin is 29 years old and has 4.00 friends.";

            var actualOutput = ExtendedStringFormatter.Default.FormatEx(formatString, p.Name, p.Age, p.Friends.Count);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicReflection()
        {
            var p = MakeQuentin();

            var formatString = "{Name} is {Age} years old and has {Friends.Count:N2} friends.";
            var expectedOutput = "Quentin is 29 years old and has 4.00 friends.";

            var actualOutput = ExtendedStringFormatter.Default.FormatEx(formatString, p);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicConditional()
        {

            var formatString = "There {0:is|are} {0} item{0:|s} remaining...";
            var expectedOutput = "There are 5 items remaining...";

            var actualOutput = ExtendedStringFormatter.Default.FormatEx(formatString, 5);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicArray()
        {
            var data = new DateTime[] {
                                          new DateTime(1999, 12, 31),
                                          new DateTime(2010, 10, 10),
                                          new DateTime(3000, 1, 1),
                                      };

            var formatString = "All dates: {0:M/d/yyyy| and }.";
            var expectedOutput = "All dates: 12/31/1999 and 10/10/2010 and 1/1/3000.";

            var actualOutput = ExtendedStringFormatter.Default.FormatEx(formatString, data);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


    }
}
