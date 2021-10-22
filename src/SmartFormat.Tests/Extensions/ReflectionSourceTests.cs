using System;
using System.Reflection;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ReflectionSourceTests
    {
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

        [Test]
        public void Test_Properties()
        {
            // Length property for a string comes from StringSource
            var formats = new[] {
                "{0} {0.Length} {Length}",
                "{2.Year} {2.Month:00}-{2.Day:00}",
                "{3.Value} {3.Anon}",
                "Chained: {4.FirstName} {4.FirstName.Length} {4.Address.City} {4.Address.State}",
                "Nested: {4:{FirstName:{} {Length} }{Address:{City} {State}}}"
            };
            var expected = new[] {
                "Zero 4 4",
                "2222 02-02",
                "3 True",
                "Chained: Michael 7 Scranton Pennsylvania",
                "Nested: Michael 7 Scranton Pennsylvania"
            };
            var args = GetArgs();
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(formats, args, expected);
        }

        [Test]
        public void Test_Properties_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings
                {CaseSensitivity = CaseSensitivityType.CaseInsensitive});
            
            // Length property for a string comes from StringSource
            var formats = new []
                {
                    "{0} {0.lenGth} {length}", "{2.YEar} {2.MoNth:00}-{2.daY:00}", "{3.Value} {3.AnoN}",
                    "Chained: {4.fIrstName} {4.Firstname.Length} {4.Address.City} {4.aDdress.StAte}",
                    "Nested: {4:{FirstName:{} {Length} }{Address:{City} {StaTe}}}",
                    // Due to double-brace escaping, the spacing in this nested format is irregular
                };
            var expected = new []
                {
                    "Zero 4 4", "2222 02-02", "3 True", "Chained: Michael 7 Scranton Pennsylvania",
                    "Nested: Michael 7 Scranton Pennsylvania",
                };
            var args = GetArgs();
            formatter.Test(formats, args, expected);
        }

        /// <summary>
        /// system.string methods are processed by <see cref="StringSource"/> since v3.0
        /// </summary>
        [Test]
        public void Test_Parameterless_Methods()
        {
            var format = "{0} {0.ToLower} {ToLower} {ToUpper}";
            //var expected = "Zero zero zero ZERO";

            var smart = new SmartFormatter();
            smart.AddExtensions(new ReflectionSource(), new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());

            var args = GetArgs();
            Assert.That(() => smart.Format(format, args), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("ToLower"));
        }

        /// <summary>
        /// system.string methods are processed by <see cref="StringSource"/> since v3.0
        /// </summary>
        [Test]
        public void Test_Methods_CaseInsensitive()
        {
            var smart = new SmartFormatter(new SmartSettings{ CaseSensitivity = CaseSensitivityType.CaseInsensitive });
            smart.AddExtensions(new ReflectionSource());
            smart.AddExtensions(new DefaultFormatter());

            var format = "{0} {0.ToLower} {toloWer} {touPPer}";
            //var expected = "Zero zero zero ZERO";
            var args = GetArgs();
            Assert.That(() => smart.Format(format, args), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("ToLower"));
        }

        [Test]
        public void Void_Methods_Should_Just_Be_Ignored()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ReflectionSource(), new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());
            Assert.That(() => smart.Format("{0.Clear}", smart.SourceExtensions), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("Clear"));
        }

        [Test]
        public void Methods_With_Parameter_Should_Just_Be_Ignored()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ReflectionSource(), new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());
            Assert.That(() => smart.Format("{0.Add}", smart.SourceExtensions), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("Add"));
        }

        [Test]
        public void Properties_With_No_Getter_Should_Just_Be_Ignored()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ReflectionSource(), new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());
            Assert.That(() => smart.Format("{Misc.OnlySetterProperty}", new { Misc = new MiscObject() }), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("OnlySetterProperty"));
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
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Test(formats, args, expected);
        }

        [Test]
        public void Test_Fields_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{ CaseSensitivity = CaseSensitivityType.CaseInsensitive });
            
            var formats = new string[] { "{field}" };
            var expected = new string[] { "Field" };
            var args = new object[] { new MiscObject(), };
            formatter.Test(formats, args, expected);
        }

        [Test]
        public void Test_Get_Property_From_Base_Class()
        {
            var derived = new DerivedMiscObject();
            var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{ CaseSensitivity = CaseSensitivityType.CaseInsensitive });

            Assert.AreEqual(string.Format($"{derived.Property}"), formatter.Format("{Property}", derived));
            Assert.AreEqual(string.Format($"{derived.ReadonlyProperty}"), formatter.Format("{ReadonlyProperty}", derived));
        }

        [Test]
        public void Test_With_TypeCaching_And_Method_Call()
        {
            // Note: Cached Property values work the same as cached methods

            var formatter = Smart.CreateDefaultSmartFormat();
            var reflectionSource = formatter.GetSourceExtension<ReflectionSource>()!;
            var typeCache =
                GetInstanceField(typeof(ReflectionSource), reflectionSource, TestReflectionSource.TypeCacheFieldName) as System.Collections.Concurrent.ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>;

            var obj = new {Obj = new MiscObject { MethodReturnValue = "The Method Value"}};
            
            // Invoke formatter 1st time
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(typeCache!.TryGetValue((typeof(MiscObject), nameof(MiscObject.Method)), out var found), Is.True);
            if(found.method != null) Assert.That(found.method.Invoke(obj.Obj, Array.Empty<object>()), Is.EqualTo(obj.Obj.MethodReturnValue));

            // Invoke formatter 2nd time
            obj.Obj.MethodReturnValue = "Another Method Value";
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Method)), out found), Is.True);
            Assert.That(found.method?.Invoke(obj.Obj, Array.Empty<object>()), Is.EqualTo(obj.Obj.MethodReturnValue));
        }
        
        [Test]
        public void Test_With_TypeCaching_And_Property_Value()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            var reflectionSource = formatter.GetSourceExtension<ReflectionSource>()!;
            var typeCache =
                GetInstanceField(typeof(ReflectionSource), reflectionSource, TestReflectionSource.TypeCacheFieldName) as System.Collections.Concurrent.ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>;

            var obj = new {Obj = new MiscObject { Field = "The Field Value"}};
            
            // Invoke formatter 1st time
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache!.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out var found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));
            
            // Invoke formatter 2nd time
            obj.Obj.Field = "Another Field Value";
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));
        }

        [Test]
        public void Test_With_TypeCaching_Disabled()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            var reflectionSource = formatter.GetSourceExtension<ReflectionSource>()!;
            reflectionSource.IsTypeCacheEnabled = false;
            var typeCache =
                GetInstanceField(typeof(ReflectionSource), reflectionSource, TestReflectionSource.TypeCacheFieldName) as System.Collections.Concurrent.ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>;

            var obj = new {Obj = new MiscObject { Field = "The Field Value", MethodReturnValue = "The Method Value"}};
            
            // Invoke formatter, expecting results, but empty cache
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(formatter.Format("{Obj.Method}", obj), Is.EqualTo(obj.Obj.MethodReturnValue));
            Assert.That(typeCache!.Count, Is.EqualTo(0));
        }

        [Test]
        public void Nullable_Property_Should_Return_Empty_String()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new DefaultSource(), new ReflectionSource());
            smart.AddExtensions(new DefaultFormatter());
            var data = new {Person = new Person()};

            var result = smart.Format("{Person.Address?.City}", data);
            Assert.That(result, Is.Empty);
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

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        internal static object? GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                              | BindingFlags.Static;
            var field = type.GetField(fieldName, bindingFlags);
            return field?.GetValue(instance);
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

        internal class TestReflectionSource : ReflectionSource
        {
            public static string TypeCacheFieldName => nameof(TypeCache);
        }
    }
}
