using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class ReflectionSourceTests
{
    private static SmartFormatter GetFormatter(SmartSettings? settings = null)
    {
        var smart = new SmartFormatter(settings ?? new SmartSettings())
            .AddExtensions(new DefaultSource(), new ReflectionSource())
            .AddExtensions(new DefaultFormatter());
        return smart;
    }

    private static object[] GetArgs()
    {
        return new object[] {
            "Zero",
            111,
            new DateTime(2222,2,2,2,2,2),
            new { Value = 3, Anon = true },
            TestFactory.GetPerson(),
        };
    }

#if NET
    [Test]
    public void DateOnly_TimeOnly()
    {
        var date = new DateOnly(2023, 12, 31);
        var time = new TimeOnly(23, 45, 56);

        var result = Smart.Format("{0:dd.MM.yyyy} - {1:HH\\:mm\\:ss}", date, time);

        Assert.That(result, Is.EqualTo("31.12.2023 - 23:45:56"));
    }
#endif

    [TestCase("{0} {0.Length} {Length}", "Zero 4 4")]
    [TestCase("{2.Year} {2.Month:00}-{2.Day:00}", "2222 02-02")]
    [TestCase("{3.Value} {3.Anon}", "3 True")]
    [TestCase("Chained: {4.FirstName} {4.FirstName.Length} {4.Address.City} {4.Address.State}", "Chained: Michael 7 Scranton Pennsylvania")]
    [TestCase("Nested: {4:{FirstName:{} {Length} }{Address:{City} {State}}}", "Nested: Michael 7 Scranton Pennsylvania")]
    public void Test_Properties_CaseSensitive(string format, string expected)
    {
        var formatter = GetFormatter(new SmartSettings
            { CaseSensitivity = CaseSensitivityType.CaseInsensitive });
        // Length property for a string comes from StringSource
        formatter.AddExtensions(new StringSource());

        var args = GetArgs();
        var actual = formatter.Format(format, args);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("{0} {0.lenGth} {length}", "Zero 4 4")]
    [TestCase("{2.YEar} {2.MoNth:00}-{2.daY:00}", "2222 02-02")]
    [TestCase("{3.Value} {3.AnoN}", "3 True")]
    [TestCase("Chained: {4.fIrstName} {4.Firstname.Length} {4.Address.City} {4.aDdress.StAte}", "Chained: Michael 7 Scranton Pennsylvania")]
    [TestCase("Nested: {4:{FirstName:{} {Length} }{Address:{City} {StaTe}}}", "Nested: Michael 7 Scranton Pennsylvania")]
    public void Test_Properties_CaseInsensitive(string format, string expected)
    {
        var formatter = GetFormatter(new SmartSettings
            {CaseSensitivity = CaseSensitivityType.CaseInsensitive});
        // Length property for a string comes from StringSource
        formatter.AddExtensions(new StringSource());

        var args = GetArgs();
        var actual = formatter.Format(format, args);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Members_CaseInsensitive_With_Same_OrdinalIgnoreCase_Name()
    {
        var formatter = GetFormatter(new SmartSettings
            { CaseSensitivity = CaseSensitivityType.CaseInsensitive });
        
        // The first property matches the case-insensitive name
        var args = new { Data = new { Value = 123, VALUE = "456" } };

        var actual = formatter.Format("{Data.Value} and {Data.VALUE} with type {Data.VALUE.GetType}", args);
        Assert.That(actual, Is.EqualTo("123 and 123 with type System.Int32"));
    }

    /// <summary>
    /// System.String methods are processed by <see cref="StringSource"/> since v3.0
    /// </summary>
    [Test]
    public void Test_Parameterless_Methods()
    {
        var format = "{0} {0.ToLower} {ToLower} {ToUpper}";
        //var expected = "Zero zero zero ZERO";

        var smart = GetFormatter();

        var args = GetArgs();
        Assert.That(() => smart.Format(format, args), Throws.Exception.TypeOf<FormattingException>().And.Message.Contains("ToLower"));
    }

    /// <summary>
    /// System.String methods are processed by <see cref="StringSource"/> since v3.0
    /// </summary>
    [Test]
    public void Test_Methods_CaseInsensitive()
    {
        var smart = GetFormatter(new SmartSettings { CaseSensitivity = CaseSensitivityType.CaseInsensitive });

        var format = "{0} {0.ToLower} {toloWer} {touPPer}";
        //var expected = "Zero zero zero ZERO";
        var args = GetArgs();
        Assert.That(() => smart.Format(format, args), Throws.Exception.TypeOf<FormattingException>().And.Message.Contains("ToLower"));
    }

    [Test]
    public void Void_Methods_Should_Just_Be_Ignored()
    {
        var smart = GetFormatter();
        Assert.That(() => smart.Format("{0.Clear}", smart.SourceExtensions), Throws.Exception.TypeOf<FormattingException>().And.Message.Contains("Clear"));
    }

    [Test]
    public void Methods_With_Parameter_Should_Just_Be_Ignored()
    {
        var smart = GetFormatter();
        Assert.That(() => smart.Format("{0.Add}", smart.SourceExtensions), Throws.Exception.TypeOf<FormattingException>().And.Message.Contains("Add"));
    }

    [Test]
    public void Properties_With_No_Getter_Should_Just_Be_Ignored()
    {
        var smart = GetFormatter();
        Assert.That(() => smart.Format("{Misc.OnlySetterProperty}", new { Misc = new MiscObject() }), Throws.Exception.TypeOf<FormattingException>().And.Message.Contains("OnlySetterProperty"));
    }
        

    [Test]
    public void Test_Fields()
    {
        var formats = new string[] {
            "{Field}"
        };
        var expected = new string[] {
            "Field"
        };
        var args = new object[] {
            new MiscObject(),
        };
        var smart = GetFormatter();
        smart.Test(formats, args, expected);
    }

    [Test]
    public void Test_Fields_CaseInsensitive()
    {
        var formatter = GetFormatter(new SmartSettings{ CaseSensitivity = CaseSensitivityType.CaseInsensitive });
            
        var formats = new string[] { "{field}" };
        var expected = new string[] { "Field" };
        var args = new object[] { new MiscObject(), };
        formatter.Test(formats, args, expected);
    }

    [Test]
    public void Test_Get_Property_From_Base_Class()
    {
        var derived = new DerivedMiscObject();
        var formatter = GetFormatter(new SmartSettings{ CaseSensitivity = CaseSensitivityType.CaseInsensitive });

        Assert.Multiple(() =>
        {
            Assert.That(formatter.Format("{Property}", derived), Is.EqualTo(string.Format($"{derived.Property}")));
            Assert.That(formatter.Format("{ReadonlyProperty}", derived), Is.EqualTo(string.Format($"{derived.ReadonlyProperty}")));
        });
    }

    [Test]
    public void Test_With_TypeCaching_And_Method_Call()
    {
        // Note: Cached Property values work the same as cached methods

        var formatter = GetFormatter();
            
        var typeCache = ReflectionSource.TypeCache;
        var obj = new {Obj = new MiscObject { MethodReturnValue = "The Method Value"}};
        (System.Reflection.FieldInfo? field, System.Reflection.MethodInfo? method) found = default;
        Assert.Multiple(() =>
        {

            // Invoke formatter 1st time
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(typeCache!.TryGetValue((typeof(MiscObject), nameof(MiscObject.Method)), out found), Is.True);
        });
        if (found.method != null) Assert.That(found.method.Invoke(obj.Obj, Array.Empty<object>()), Is.EqualTo(obj.Obj.MethodReturnValue));

        // Invoke formatter 2nd time
        obj.Obj.MethodReturnValue = "Another Method Value";
        Assert.Multiple(() =>
        {
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Method)), out found), Is.True);
            Assert.That(found.method?.Invoke(obj.Obj, Array.Empty<object>()), Is.EqualTo(obj.Obj.MethodReturnValue));
        });
    }
        
    [Test]
    public void Test_With_TypeCaching_And_Property_Value()
    {
        var formatter = GetFormatter();
        var typeCache = ReflectionSource.TypeCache;
        var obj = new {Obj = new MiscObject { Field = "The Field Value"}};
        (System.Reflection.FieldInfo? field, System.Reflection.MethodInfo? method) found = default;

        Assert.Multiple(() =>
        {

            // Invoke formatter 1st time
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));
        });

        // Invoke formatter 2nd time
        obj.Obj.Field = "Another Field Value";
        Assert.Multiple(() =>
        {
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));
        });
    }

    [Test]
    public void Test_With_TypeCaching_Disabled()
    {
        var formatter = GetFormatter();
        var reflectionSource = formatter.GetSourceExtension<ReflectionSource>()!;
        ReflectionSource.TypeCache.Clear();
        reflectionSource.IsTypeCacheEnabled = false;
        var obj = new {Obj = new MiscObject { Field = "The Field Value", MethodReturnValue = "The Method Value"}};
        Assert.Multiple(() =>
        {
            // Invoke formatter, expecting results, but empty cache
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(ReflectionSource.TypeCache, Is.Empty);
        });
    }

    [Test]
    public void Nullable_Property_Should_Return_Empty_String()
    {
        var smart = GetFormatter();
        var data = new {Person = new Person()};

        var result = smart.Format("{Person.Address?.City}", data);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Keep_Cache_Within_Limit_Of_MaxCacheSize()
    {
        var formatter = GetFormatter();
        ReflectionSource.MaxCacheSize = 1;

        // These args would create 2 items in the cache, which is more than the limit
        var args = new { Data = new { SomeValue = 123 } };
        _ = formatter.Format("{Data.SomeValue}", args);

        Assert.Multiple(() =>
        {
            Assert.That(ReflectionSource.TypeCache, Has.Count.EqualTo(ReflectionSource.MaxCacheSize));
#if !NET6_0_OR_GREATER
            Assert.That(ReflectionSource.KeyList, Has.Count.EqualTo(ReflectionSource.TypeCache.Count));
    #endif
            // Item2 of the value tuple is the name of the field or method
            Assert.That(ReflectionSource.TypeCache.First().Key.Item2, Is.EqualTo("SomeValue"), "Last added item is kept in cache");
        });

        // Reset to default
        ReflectionSource.MaxCacheSize = ReflectionSource.DefaultCacheSize;
    }

    [Test]
    public void Parallel_Smart_Format()
    {
        // Switch to thread safety - otherwise the test would throw
        var savedMode = ThreadSafeMode.SwitchOn();
            
        var results = new ConcurrentDictionary<long, string>();
        var threadIds = new ConcurrentDictionary<int, int>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 100 };
        ReflectionSource.TypeCache.Clear();

        // Use a single instance of SmartFormatter that is thread-safe
        var smart = new SmartFormatter()
            .AddExtensions(new DefaultSource(), new ReflectionSource())
            .AddExtensions(new DefaultFormatter());

        Assert.That(code: () =>
            Parallel.For(0L, 1000, options, (i, loopState) =>
            {
                var args = new { Data = new { Value = i} };

                // register unique thread ids
                threadIds.TryAdd(Environment.CurrentManagedThreadId, Environment.CurrentManagedThreadId);
                results.TryAdd(i, smart.Format("{Data.Value}", args));
            }), Throws.Nothing);

        Assert.Multiple(() =>
        {
            Assert.That(ReflectionSource.TypeCache,
                Is.InstanceOf<ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>>());
            // Both 'Data' and 'Value' are cached
            Assert.That(ReflectionSource.TypeCache, Has.Count.EqualTo(2));
            Assert.That(results.All(kv => kv.Key.ToString() == kv.Value), Is.True);
            Assert.That(threadIds, Has.Count.AtLeast(2)); // otherwise the test is not significant
        });

        // Restore to saved value
        ThreadSafeMode.SwitchTo(savedMode);
    }

    public class MiscObject
    {
        private string _onlySetterProperty;
        public MiscObject()
        {
            Field = "Field";
            ReadonlyProperty = "ReadonlyProperty";
            MethodReturnValue = "Method";
            _onlySetterProperty = string.Empty;
            _ = _onlySetterProperty;
        }
        public string Field;
        public string OnlySetterProperty
        {
            set => _onlySetterProperty = value;
        }
        public string ReadonlyProperty { get; private set; }
        public virtual string Property { get; set; } = "Property";
        public string Method()
        {
            return MethodReturnValue;
        }
        public string MethodReturnValue { get; set; }
    }

    public class DerivedMiscObject : MiscObject
    {
    }

    internal class Address
    {
        public readonly string Country = string.Empty;
        public readonly string City = string.Empty;
    }

    internal class Person
    {
        public string FirstName = "first";
        public string LastName = "last";
        public Address? Address = null;
    }
}
