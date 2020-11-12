﻿using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class ValueTupleTests
    {
        [Test]
        public void Format_With_ValueTuples()
        {
            // With SmartObjects
            // * all objects used for Smart.Format can be collected in one place as the first argument
            // * the format string can be written like each object would be the first argument of Smart.Format
            // * there is no need to bother from which argument a value should come from 

            var addr = new Extensions.DictionaryFormatterTests.Address();
            var dict1 = new Dictionary<string, string> { {"dict1key", "dict1 Value"} };
            var dict2 = new Dictionary<string, string> { { "dict2key", "dict2 Value" } };

            // format for ValueTuples as argument 1 to Smart.Format
            const string format1 = "Name: {Person.FirstName} {Person.LastName}\n" +
                                  "Dictionaries: {dict1key}, {dict2key}";

            // format for ValueTuples as argument 2 to Smart.Format:
            // works although unnecessary and leading to argument references in the format string
            const string format2 = "Name: {1.Person.FirstName} {1.Person.LastName}\n" +
                                   "Dictionaries: {1.dict1key}, {1.dict2key}";

            var expected = $"Name: {addr.Person.FirstName} {addr.Person.LastName}\n" +
                           $"Dictionaries: {dict1["dict1key"]}, {dict2["dict2key"]}";

            var formatter = Smart.CreateDefaultSmartFormat();
            var result1 = formatter.Format(format1, (addr, dict1, dict2));
            var result2 = formatter.Format(format2, null, (addr, dict1, dict2));

            Assert.AreEqual(expected, result1);
            Assert.AreEqual(expected, result2);
        }

        [Test]
        public void Nested_Scope()
        {
            var clubOrMember = new { Member = new { Name = "Joe" }, Club = new { Name = "The Strikers" } };
            var clubNoMember = new { Member = default(object), Club = new { Name = "The Strikers" } };
            var say = new { Hello = "Good morning" };
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.ParseErrorAction = formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;

            var result = formatter.Format("{Member:choose(null):{Club.Name}|{Name}} - {Hello}", new SmartObjects(new object[] { clubOrMember, say }));
            Assert.AreEqual($"{clubOrMember.Member.Name} - {say.Hello}", result);

            result = formatter.Format("{Member:choose(null):{Club.Name}|{Name}} - {Hello}", new SmartObjects(new object[] { clubNoMember, say }));
            Assert.AreEqual($"{clubOrMember.Club.Name} - {say.Hello}", result);
        }

        [Test]
        public void Not_Invoked_With_FormattingInfo()
        {
            Assert.IsFalse(new ValueTupleSource(new SmartFormatter()).TryEvaluateSelector(new SelectorInfo()));
        }

        private class SelectorInfo : ISelectorInfo
        {
            public object CurrentValue { get; }
            public string SelectorText { get; }
            public int SelectorIndex { get; }
            public string SelectorOperator { get; }
            public object Result { get; set; }
            public Placeholder Placeholder { get; }
            public FormatDetails FormatDetails { get; }
        }
    }
}
