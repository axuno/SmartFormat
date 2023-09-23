using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class DictionarySourceTests
{
    private static object[] GetArgs()
    {
        var d = new Dictionary<string, object>
        {
            {"Numbers", new Dictionary<string, object>
            {
                {"One", 1},
                {"Two", 2},
                {"Three", 3},
            }},
            {"Letters", new Dictionary<string, object>
            {
                {"A", "a"},
                {"B", "b"},
                {"C", "c"},
            }},
            {"Object", new {
                Prop1 = "a",
                Prop2 = "b",
                Prop3 = "c",
            }},
        };

        return new object[] {
            d
        };
    }

    private static dynamic GetDynamicArgs()
    {
        dynamic d = new ExpandoObject();
        d.Numbers = new Dictionary<string, object> { { "One", 1 }, { "Two", 2 }, { "Three", 3 }, };
        d.Letters = new Dictionary<string, object> { { "A", "a" }, { "B", "b" }, { "C", "c" }, };
        d.Raw = new Dictionary<string, string> { { "X", "z" } };
        d.Object = new { Prop1 = "a", Prop2 = "b", Prop3 = "c", };

        return new object[] {
            d,
        };
    }

    [Test]
    public void Test_Dictionary()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        formatter.AddExtensions(new DictionarySource());

        var formats = new[]
        {
            "Chained: {0.Numbers.One} {Numbers.Two} {Letters.A} {Object.Prop1}",
            "Nested: {0:{Numbers:{One} {Two}}} {Letters:{A}} {Object:{Prop1}}"
        };
        var expected = new[]
        {
            "Chained: 1 2 a a",
            "Nested: 1 2 a a"
        };
        var args = GetArgs();
        formatter.Test(formats, args, expected);
    }

    [Test]
    public void Test_Dynamic()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        formatter.AddExtensions(new DictionarySource());

        var formats = new[]
        {
            "Chained: {0.Numbers.One} {Numbers.Two} {Letters.A} {Object.Prop1} {Raw.X}",
            "Nested: {0:{Numbers:{One} {Two}}} {Letters:{A}} {Object:{Prop1}} {Raw:{X}}"
        };
        var expected = new[]
        {
            "Chained: 1 2 a a z",
            "Nested: 1 2 a a z"
        };
        var args = (object[])GetDynamicArgs();
        formatter.Test(formats, args, expected);
    }

    [Test]
    public void Test_Dynamic_CaseInsensitive()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {CaseSensitivity = CaseSensitivityType.CaseInsensitive});
        formatter.AddExtensions(new DictionarySource());

        var formats = new string[]
        {
            "Chained: {0.Numbers.One} {Numbers.Two} {Letters.A} {Object.Prop1} {Raw.x}",
            "Nested: {0:{Numbers:{One} {Two}}} {Letters:{A}} {Object:{Prop1}} {Raw:{x}}"
        };
        var expected = new string[]
        {
            "Chained: 1 2 a a z",
            "Nested: 1 2 a a z"
        };
        var args = (object[])GetDynamicArgs();
        formatter.Test(formats, args, expected);
    }

    [Test]
    public void Dictionary_Dot_Notation()
    {
        // Process properties of a class instance type-safe and without the need for reflection
        // and with dot notation for dictionaries

        var addr = new Address();

        const string format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        var expected = $"Address: {addr.City?.ZipCode} {addr.City?.Name}, {addr.City?.AreaCode}\n" +
                       $"Name: {addr.Person.FirstName} {addr.Person.LastName}";

        var formatter = Smart.CreateDefaultSmartFormat();
        var result = formatter.Format(format, addr.ToDictionary());

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Dictionary_Dot_Notation_Nullable()
    {
        // Process properties of a class instance type-safe and without the need for reflection
        // and with nullable dot notation for dictionaries

        var addr = new Address {City = null};

        var addrDict = addr.ToDictionary();

        const string format = "Address: {City?.ZipCode} {City?.Name} {City?.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        var expected = $"Address: {addr.City?.ZipCode} {addr.City?.Name} {addr.City?.AreaCode}\n" +
                       $"Name: {addr.Person.FirstName} {addr.Person.LastName}";

        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), new DictionarySource())
            .AddExtensions(new DefaultFormatter());
            
        var result = smart.Format(format, addrDict);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Generic_Dictionary_String_String()
    {
        var dict = new Dictionary<string, string> { { "Name", "Joe" } };
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), new DictionarySource())
            .AddExtensions(new DefaultFormatter());
        var result = smart.Format("{Name}", dict);

        Assert.That(result, Is.EqualTo("Joe"));
    }

    [Test]
    public void IReadOnlyDictionary_With_IConvertible_Key()
    {
        var roDict = new CustomReadOnlyDictionary<IConvertible, object?>(new Dictionary<IConvertible, object?> { { 1, 1 }, { "Two", 2 }, { "Three", "three" }, });
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), new DictionarySource { IsIReadOnlyDictionaryEnabled = true })
            .AddExtensions(new DefaultFormatter());
        var result = smart.Format("{1}{Two}{Three}", roDict);

        Assert.That(result, Is.EqualTo("12three"));
    }

    [Test]
    public void IReadOnlyDictionary_With_String_Key()
    {
        var roDict = new CustomReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { { "One", 1 }, { "Two", 2 }, { "Three", "three" }, });
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), new DictionarySource { IsIReadOnlyDictionaryEnabled = true })
            .AddExtensions(new DefaultFormatter());
        var result = smart.Format("{One}{Two}{Three}", roDict);

        Assert.That(result, Is.EqualTo("12three"));
    }

    [Test]
    public void IReadOnlyDictionary_Cache_Should_Store_Types_It_Cannot_Handle()
    {
        var dictSource = new DictionarySource { IsIReadOnlyDictionaryEnabled = true };
        var kvp = new KeyValuePair<string, object?>("One", 1);
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), dictSource, new KeyValuePairSource())
            .AddExtensions(new DefaultFormatter());
        var result = smart.Format("{One}", kvp);

        Assert.That(result, Is.EqualTo("1"));
        Assert.That(dictSource.RoDictionaryTypeCache.Keys.Count, Is.EqualTo(1));
        Assert.That(dictSource.RoDictionaryTypeCache.Keys.First(), Is.EqualTo(typeof(KeyValuePair<string, object?>)));
        Assert.That(dictSource.RoDictionaryTypeCache.Values.First(), Is.Null);
    }

    public class CustomReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue?>
    {
        private readonly IDictionary<TKey, TValue?> _dictionary;

        public CustomReadOnlyDictionary(IDictionary<TKey, TValue?> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue?>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue? this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue?> Values => _dictionary.Values;
    }

    public class Address
    {
        public CityDetails? City { get; set; } = new();
        public PersonDetails Person { get; set; } = new();

        public Dictionary<string, object?> ToDictionary()
        {
            var d = new Dictionary<string, object?>
            {
                { nameof(City), City?.ToDictionary() },
                { nameof(Person), Person.ToDictionary() }
            };
            return d;
        }

        public class CityDetails
        {
            public string Name { get; set; } = "New York";
            public string ZipCode { get; set; } = "00501";
            public string AreaCode { get; set; } = "631";

            public Dictionary<string, string?> ToDictionary()
            {
                return new()
                {
                    {nameof(Name), Name},
                    {nameof(ZipCode), ZipCode},
                    {nameof(AreaCode), AreaCode}
                };
            }
        }

        public class PersonDetails
        {
            public string FirstName { get; set; } = "John";
            public string LastName { get; set; } = "Doe";
            public Dictionary<string, string> ToDictionary()
            {
                return new()
                {
                    {nameof(FirstName), FirstName},
                    {nameof(LastName), LastName},
                };
            }
        }
    }
}
