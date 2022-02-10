using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ListFormatterTests
    {
        public static object[] GetArgs()
        {
            var args = new object[] {
                "ABCDE".ToCharArray(),
                "One|Two|Three|Four|Five".Split('|'),
                TestFactory.GetPerson().Friends,
                "1/1/2000|10/10/2010|5/5/5555".Split('|').Select(s=>DateTime.ParseExact(s,"M/d/yyyy", new CultureInfo("en-US"))),
                new []{1,2,3,4,5},
            };
            return args;
        }

        [Test]
        public void Simple_List()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var items = new[] { "one", "two", "three" };
            var result = smart.Format("{0:list:{}|, |, and }", new object[] { items }); // important: not only "items" as the parameter
            Assert.AreEqual("one, two, and three", result);
        }

        [Test]
        public void Simple_List_Changed_SplitChar()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            // Set SplitChar from | to TAB, so we can use | for the output string
            smart.GetFormatterExtension<ListFormatter>()!.SplitChar = '\t';
            var items = new[] { "one", "two", "three" };
            var result = smart.Format("{0:list:{}\t|\t|}", new object[] { items }); // important: not only "items" as the parameter
            Assert.AreEqual("one|two|three", result);
        }

        [Test]
        public void Empty_List()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var items = Array.Empty<string>();
            var result = smart.Format("{0:list:{}|, |, and }", new object[] { items });
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void Null_List()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var result = smart.Format("{TheList?:list:{}|, |, and }", new { TheList = default(object)});
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void List_of_anonymous_types_and_enumerables()
        {
            var data = new[]
            {
                new { Name = "Person A", Gender = "M" },
                new { Name = "Person B", Gender = "F" },
                new { Name = "Person C", Gender = "M" }
            };

            var model = new
            {
                Persons = data.Where(p => p.Gender == "M")
            };
            
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            {
                StringFormatCompatibility = false, // mandatory for this test case because of consecutive curly braces
                Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError},
                Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}
            });

            // Note: it's faster to add the named formatter, than finding it implicitly by "trial and error".
            var result = smart.Format("{0:list:{Name}|, |, and }", new object[] { data }); // Person A, Person B, and Person C
            Assert.AreEqual("Person A, Person B, and Person C", result);
            result = smart.Format("{0:list:{Name}|, |, and }", model.Persons);  // Person A, and Person C
            Assert.AreEqual("Person A, and Person C", result);
            result = smart.Format("{0:list:{Name}|, |, and }", data.Where(p => p.Gender == "F"));  // Person B
            Assert.AreEqual("Person B", result);
            result = smart.Format("{0:{Persons:list:{Name}|, }}", model); // Person A, and Person C
            Assert.AreEqual("Person A, Person C", result);
        }

        [TestCase("{4:list}", "System.Int32[]")]
        [TestCase("{4:list:|}","12345")]
        [TestCase("{4:list:00|}","0102030405")]
        [TestCase("{4:list:|,}","1,2,3,4,5")]
        [TestCase("{4:list:|, |, and }","1, 2, 3, 4, and 5")]
        [TestCase("{4:list:N2|, |, and }","1.00, 2.00, 3.00, 4.00, and 5.00")]
        public void FormatTest(string format, string expected)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var args = GetArgs();
            smart.Test(new[] {format}, args, new[] {expected});

        }

        [TestCase("{0:list:{}-|}", "A-B-C-D-E-")]
        [TestCase("{0:list:{}|-}", "A-B-C-D-E")]
        [TestCase("{0:list:{}|-|+}", "A-B-C-D+E")]
        [TestCase("{0:list:({})|, |, and }", "(A), (B), (C), (D), and (E)")]
        public void NestedFormatTest(string format, string expected)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var args = GetArgs();
            smart.Test(new[] {format}, args, new[] {expected});
        }
        [TestCase("{2:list:{:{FirstName}}|, }", "Jim, Pam, Dwight")]
        [TestCase("{3:list:{:d:M/d/yyyy} |}", "1/1/2000 10/10/2010 5/5/5555 ")] // use the default formatter ("d") for nested dates
        [TestCase("{2:list:{:{FirstName}'s friends: {Friends:list:{FirstName}|, }}|; }", "Jim's friends: Dwight, Michael; Pam's friends: Dwight, Michael; Dwight's friends: Michael")]
        public void NestedArraysTest(string format, string expected)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var args = GetArgs();
            smart.Test(new[] {format}, args, new[] {expected});
        }

        [Test, Description("Format a list of lists")]
        public void List_Of_Lists_With_Element_Format()
        {
            var data = new List<int[]> {
                new[] { 1, 2, 3 },
                new[] { 4, 5, 6 },
                new[] { 7, 8, 9 }
            };

            var formatter = new SmartFormatter()
                .AddExtensions(new ListFormatter(), new DefaultSource())
                .AddExtensions(new DefaultFormatter());

            var expected = "001, 002, 003\n" + "004, 005, 006\n" + "007, 008, 009";
            var result = formatter.Format("{0:list:{:list:{:000}|, |, }|\n|\n}", data);
            //                                   |       |        | |       |      |
            //                                   |       |  element format  |      |
            //                                   |       |___ inner list ___|      |
            //                                   |___________ outer list __________|
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void WithThreadPool_ShouldNotMixUpCollectionIndex()
        {
            void ResetAllPools(bool goThreadSafe)
            {
                // get specialized pools (includes smart pools)
                foreach (dynamic p in PoolRegistry.Items.Values)
                {
                    p.Reset(goThreadSafe);
                }
            }

            // Switch to thread safety
            const bool currentThreadSafeMode = true;
            var savedIsThreadSafeMode = SmartSettings.IsThreadSafeMode;
            SmartSettings.IsThreadSafeMode = ListFormatter.IsThreadSafeMode = currentThreadSafeMode;
            ResetAllPools(currentThreadSafeMode);
            
            const string format = "wheres-Index={Index} - List: {0:{}| and }";
            const string expected = "wheres-Index=-1 - List: test1 and test2";

            // The same variable will be access from different threads
            var wheres = new List<string>{"test1", "test2"};
            var tasks = new List<Task<string>>();

            for (var i = 0; i < 10; ++i)
            {
                tasks.Add(Task.Factory.StartNew(val =>
                {
                    Thread.Sleep(5 * (int)(val ?? 100));
                    var smart = new SmartFormatter()
                        .AddExtensions(new StringSource(), new ListFormatter(), new DefaultSource())
                        .AddExtensions(new DefaultFormatter());
                    var ret = smart.Format(format, wheres);
                    
                    Thread.Sleep(5 * (int)(val ?? 100)); /* add some delay to force ThreadPool swapping */
                    return ret;
                }, i));
            }

            foreach (var t in tasks)
            {
                // Note: Using "[ThreadStatic] private static int CollectionIndex", the result will be as expected only with the first task
                if (expected == t.Result)
                    Console.WriteLine(@"Task {0} passed.", t.AsyncState);
                
                Assert.That(t.Result, Is.EqualTo(expected));
            }

            // Restore thread safety
            SmartSettings.IsThreadSafeMode = ListFormatter.IsThreadSafeMode = savedIsThreadSafeMode;
            ResetAllPools(savedIsThreadSafeMode);
        }

        [TestCase("{0:list:{} = {Index}|, }", "A = 0, B = 1, C = 2, D = 3, E = 4")] // Index holds the current index of the iteration
        [TestCase("{1:list:{Index}: {ToCharArray:list:{} = {Index}|, }|; }", "0: O = 0, n = 1, e = 2; 1: T = 0, w = 1, o = 2; 2: T = 0, h = 1, r = 2, e = 3, e = 4; 3: F = 0, o = 1, u = 2, r = 3; 4: F = 0, i = 1, v = 2, e = 3")] // Index can be nested, ToCharArray() requires StringSource or ReflectionSource
        [TestCase("{0:list:{} = {1.Index}|, }", "A = One, B = Two, C = Three, D = Four, E = Five")] // Index is used to synchronize 2 lists
        [TestCase("{Index}", "-1")] // Index can be used out-of-context, but should always be -1
        public void TestIndex(string format, string expected)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var args = GetArgs();
            smart.Test(new[] {format}, args, new[] {expected});
        }

        [Test]
        public void Objects_Implementing_IList_Are_Processed()
        {
            var items = new[] { "one", "two", "three" };

            var formatter = new SmartFormatter()
                .AddExtensions(new ListFormatter(), new DefaultSource())
                .AddExtensions(new DefaultFormatter());

            Assert.AreEqual("one, two, and three", formatter.Format("{0:list:{}|, |, and }", new object[] { items }));
        }

        [Test]
        public void Objects_Not_Implementing_IList_Are_Not_Processed()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.That(() => smart.Format("{0:list:{}|, |, and }", "not a list"),
                Throws.Exception.TypeOf<FormattingException>().And.Message
                    .Contains("IEnumerable"));
        }

        [TestCase("one", "one")]
        [TestCase(null, "")]
        public void Should_Return_Indexed_List_Element(string? listElement, string expected)
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ListFormatter(), new DefaultSource(), new ReflectionSource());
            smart.AddExtensions(new DefaultFormatter());
            
            var numbers = new List<string?> {"dummy", listElement};

            var data = new {Numbers = numbers};
            var indexResult1 = smart.Format(">{Numbers.1}<", data); // index method 1
            var indexResult2 = smart.Format(">{Numbers[1]}<", data); // index method 2
            
            Assert.That(indexResult1, Is.EqualTo($">{expected}<"));
            Assert.That(indexResult2, Is.EqualTo($">{expected}<"));
        }

        [Test]
        public void Null_IList_Nullable_Should_Return_Null()
        {
            var smart = new SmartFormatter()
                .AddExtensions(new ListFormatter(), new DefaultSource(), new ReflectionSource())
                .AddExtensions(new DefaultFormatter());
            
            var data = new {Numbers = default(IList<object>?)};
            // Numbers are marked as nullable
            var indexResult1 = smart.Format(">{Numbers?.0}<", data); // index method 1
            var indexResult2 = smart.Format(">{Numbers?[0]}<", data); // index method 2

            Assert.That(indexResult1, Is.EqualTo("><"));
            Assert.That(indexResult2, Is.EqualTo("><"));
        }

        [Test]
        public void Null_IList_Not_Nullable_Should_Throw()
        {
            var smart = new SmartFormatter()
                .AddExtensions(new ListFormatter(), new DefaultSource(), new ReflectionSource())
                .AddExtensions(new DefaultFormatter());
            
            var data = new {Numbers = default(IList<object>?)};

            // Numbers are NOT marked as nullable
            Assert.That(() => smart.Format(">{Numbers.0}<", data),
                Throws.Exception.TypeOf<FormattingException>().And.Message
                    .Contains("{Numbers.0}"));
            Assert.That(() => smart.Format(">{Numbers[0]}<", data),
                Throws.Exception.TypeOf<FormattingException>().And.Message
                    .Contains("{Numbers[0]}"));
        }
    }
}
