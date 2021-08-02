using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ValueTupleTests
    {
        [Test]
        public void Format_With_ValueTuples()
        {
            var addr = new DictionaryFormatterTests.Address();
            var dict1 = new Dictionary<string, string> { {"dict1key", "dict1 Value"} };
            var dict2 = new Dictionary<string, string> { { "dict2key", "dict2 Value" } };

            // format for ValueTuples as argument 1 to Smart.Format
            const string format = "Name: {Person.FirstName} {Person.LastName}\n" +
                                  "Dictionaries: {dict1key}, {dict2key}";

            var expected = $"Name: {addr.Person.FirstName} {addr.Person.LastName}\n" +
                           $"Dictionaries: {dict1["dict1key"]}, {dict2["dict2key"]}";

            var formatter = Smart.CreateDefaultSmartFormat();
            var result = formatter.Format(format, (addr, dict1, dict2));

            Assert.AreEqual(expected, result);
        }

        [TestCase("Name: {Person.FirstName} {City?.AreaCode}", true)]
        [TestCase("Name: {Person.FirstName} {City.AreaCode}", false)]
        public void Format_With_Null_Values_In_ValueTuples(string format, bool shouldSucceed)
        {
            var addr = new DictionaryFormatterTests.Address {City = null};

            var dict1 = new Dictionary<string, string> { {"dict1key", "dict1 Value"} };
            var dict2 = new Dictionary<string, string> { { "dict2key", "dict2 Value" } };

            var expected = $"Name: {addr.Person.FirstName} {addr.City?.AreaCode}";

            var formatter = Smart.CreateDefaultSmartFormat();

            if (shouldSucceed)
            {
                var result = formatter.Format(format, (dict1, dict2, addr));
                Assert.That(result, Is.EqualTo(expected));
            }
            else
            {
                Assert.That(() => formatter.Format(format, (dict1, dict2, addr)),
                    Throws.Exception.TypeOf(typeof(FormattingException)));
            }
        }

        [Test]
        public void Format_With_ValueTuples_2nd_Argument()
        {
            var addr = new DictionaryFormatterTests.Address();
            var dict1 = new Dictionary<string, string> { {"dict1key", "dict1 Value"} };
            var dict2 = new Dictionary<string, string> { { "dict2key", "dict2 Value" } };

            // format for ValueTuples as argument 2 to Smart.Format:
            // works although unnecessary and leading to argument references in the format string
            const string format = "Name: {1.Person.FirstName} {1.Person.LastName}\n" +
                                   "Dictionaries: {1.dict1key}, {1.dict2key}";

            var expected = $"Name: {addr.Person.FirstName} {addr.Person.LastName}\n" +
                           $"Dictionaries: {dict1["dict1key"]}, {dict2["dict2key"]}";

            var formatter = Smart.CreateDefaultSmartFormat();
            var result = formatter.Format(format, null!, (addr, dict1, dict2));

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test_Nested_Tuples()
        {
            var child = (Child: "The child", ChildName: "Child name");
            var mainWithChild = (Main: "Main", child);
            var expected = new List<object?> { mainWithChild.Item1, child.Child, child.ChildName};

            Assert.That(mainWithChild.GetValueTupleItemObjectsFlattened().ToList(), Is.EqualTo(expected));
        }

        [Test]
        public void Nested_Scope()
        {
            var clubOrMember = new { Member = new { Name = "Joe" }, Club = new { Name = "The Strikers" } };
            var clubNoMember = new { Member = default(object), Club = new { Name = "The Strikers" } };
            var say = new { Hello = "Good morning" };
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;

            var result = formatter.Format("{Member:choose(null):{Club.Name}|{Name}} - {Hello}", (clubOrMember, say));
            Assert.AreEqual($"{clubOrMember.Member.Name} - {say.Hello}", result);

            result = formatter.Format("{Member:choose(null):{Club.Name}|{Name}} - {Hello}", (clubNoMember, say));
            Assert.AreEqual($"{clubOrMember.Club.Name} - {say.Hello}", result);
        }

        [Test]
        public void Not_Invoked_With_FormattingInfo()
        {
            Assert.IsFalse(new ValueTupleSource().TryEvaluateSelector(new PureSelectorInfo()));
        }

        private class PureSelectorInfo : ISelectorInfo
        {
            public object? CurrentValue { get; }
            public string SelectorText { get; } = string.Empty;
            public int SelectorIndex { get; }
            public string SelectorOperator { get; } = string.Empty;
            public object? Result { get; set; }
            public Placeholder? Placeholder { get; }
            public FormatDetails FormatDetails { get; } = (FormatDetails) null!; // dummy
        }
    }
}
