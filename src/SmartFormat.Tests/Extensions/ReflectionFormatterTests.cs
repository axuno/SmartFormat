using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ReflectionFormatterTests
    {
        public object[] GetArgs()
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
            var formats = new string[] {
                "{0} {0.Length} {Length}",
                "{2.Year} {2.Month:00}-{2.Day:00}",
                "{3.Value} {3.Anon}",
                "Chained: {4.FirstName} {4.FirstName.Length} {4.Address.City} {4.Address.State}  ",
                "Nested: {4:{FirstName:{} {Length} }{Address:{City} {State} } }", // Due to double-brace escaping, the spacing in this nested format is irregular
            };
            var expected = new string[] {
                "Zero 4 4",
                "2222 02-02",
                "3 True",
                "Chained: Michael 7 Scranton Pennsylvania  ",
                "Nested: Michael 7 Scranton Pennsylvania  ",
            };
            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_Properties_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            
            var formats = new string[]
                {
                    "{0} {0.lenGth} {length}", "{2.YEar} {2.MoNth:00}-{2.daY:00}", "{3.Value} {3.AnoN}",
                    "Chained: {4.fIrstName} {4.Firstname.Length} {4.Address.City} {4.aDdress.StAte}  ",
                    "Nested: {4:{FirstName:{} {Length} }{Address:{City} {StaTe} } }",
                    // Due to double-brace escaping, the spacing in this nested format is irregular
                };
            var expected = new string[]
                {
                    "Zero 4 4", "2222 02-02", "3 True", "Chained: Michael 7 Scranton Pennsylvania  ",
                    "Nested: Michael 7 Scranton Pennsylvania  ",
                };
            var args = GetArgs();
            formatter.Test(formats, args, expected);
        }

        [Test]
        public void Test_Methods()
        {
            var formats = new string[] {
                "{0} {0.ToLower} {ToLower} {ToUpper}",
            };
            var expected = new string[] {
                "Zero zero zero ZERO",
            };
            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_Methods_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

            var formats = new string[] { "{0} {0.ToLower} {toloWer} {touPPer}", };
            var expected = new string[] { "Zero zero zero ZERO", };
            var args = GetArgs();
            formatter.Test(formats, args, expected);
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
            Smart.Default.Test(formats, args, expected);
        }

        [Test]
        public void Test_Fields_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            
            var formats = new string[] { "{field}" };
            var expected = new string[] { "Field" };
            var args = new object[] { new MiscObject(), };
            formatter.Test(formats, args, expected);
        }

        [Test]
        public void Test_Get_Property_From_Base_Class()
        {
            var derived = new DerivedMiscObject();
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

            Assert.AreEqual(string.Format($"{derived.Property}"), formatter.Format("{Property}", derived));
            Assert.AreEqual(string.Format($"{derived.ReadonlyProperty}"), formatter.Format("{ReadonlyProperty}", derived));
        }

        [Test]
        public void Test_With_TypeCaching_And_Method_Call()
        {
            // Note: Cached Property values work the same as cached methods

            var formatter = Smart.CreateDefaultSmartFormat();
            var reflectionSource = formatter.SourceExtensions.First(ext => ext.GetType() == typeof(ReflectionSource));
            var typeCache =
                GetInstanceField(typeof(ReflectionSource), reflectionSource, "_typeCache") as System.Collections.Generic
                    .Dictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>;

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
            var reflectionSource = formatter.SourceExtensions.First(ext => ext.GetType() == typeof(ReflectionSource));
            var typeCache =
                GetInstanceField(typeof(ReflectionSource), reflectionSource, "_typeCache") as System.Collections.Generic
                    .Dictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>;

            var obj = new {Obj = new MiscObject { Field = "The Field Value"}};
            
            // Invoke formatter 1st time
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache!.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out var found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));

            // Invoke formatter 2nd time
            obj.Obj.Field = "Another Field Value";
            Assert.That(formatter.Format("{Obj.Field}", obj), Is.EqualTo(obj.Obj.Field));
            Assert.That(typeCache.TryGetValue((typeof(MiscObject), nameof(MiscObject.Field)), out found), Is.True);
            Assert.That(found.field?.GetValue(obj.Obj), Is.EqualTo(obj.Obj.Field));       }

        public class MiscObject
        {
            public MiscObject()
            {
                Field = "Field";
                ReadonlyProperty = "ReadonlyProperty";
                MethodReturnValue = "Method";
            }
            public string Field;
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
    }
}
