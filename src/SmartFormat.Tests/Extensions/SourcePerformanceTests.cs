using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
#if NET462
using System.Runtime.Remoting.Messaging;
#endif
using NUnit.Framework;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class SourcePerformanceTests
    {
        [Test, Explicit("Performance tests should be run explicitly. This test will take about 10 seconds.")]
        public void Source_Performance()
        {
            var result = string.Empty;
            var addr = new Address();
            const string format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                                  "Name: {Person.FirstName} {Person.LastName}";

            var sw = new Stopwatch();

            // Direct member access:
            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
                result = $"Address: {addr.City.ZipCode} {addr.City.Name}, {addr.City.AreaCode}\n" +
                         $"Name: {addr.Person.FirstName} {addr.Person.LastName}";
            }
            sw.Stop();
            var directMemberTest = sw.ElapsedMilliseconds;
            sw.Reset();

            // Smart.Format with reflection:
            var formatter = new SmartFormatter();
            formatter.AddExtensions(
                new ReflectionSource(formatter),
                new DefaultSource(formatter)
                );
            formatter.AddExtensions(
                new DefaultFormatter()
                );

            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
                result = formatter.Format(format, addr);
            }
            sw.Stop();
            var reflectionMemberTest = sw.ElapsedMilliseconds;
            sw.Reset();

            // Smart.Format with Dictionary:
            formatter = new SmartFormatter();
            formatter.AddExtensions(
                new DictionarySource(formatter),
                new DefaultSource(formatter)
                );
            formatter.AddExtensions(
                new DefaultFormatter()
                );

            sw.Start();
            var dict = addr.ToDictionary(); // get class projection to dictionary hierarchy
            for (var i = 0; i < 100000; i++)
            {
                result = formatter.Format(format, dict);
            }
            sw.Stop();
            var dictionaryProjectionTest = sw.ElapsedMilliseconds;
            sw.Reset();

            // Smart.Format with JSON:
            formatter = new SmartFormatter();
            formatter.AddExtensions(
                new JsonSource(formatter),
                new DefaultSource(formatter)
            );
            formatter.AddExtensions(
                new DefaultFormatter()
            );

            sw.Start();
            var jObject = addr.ToJson(); // get class projection to JSON hierarchy
            for (var i = 0; i < 100000; i++)
            {
                result = formatter.Format(format, jObject);
            }
            sw.Stop();
            var jsonProjectionTest = sw.ElapsedMilliseconds;
            sw.Reset();

            Console.WriteLine("Test results as performance index:");
            Console.WriteLine("Direct Member Test: {0} ({1} ms)", directMemberTest/ directMemberTest, directMemberTest);
            Console.WriteLine("Dictionary Projection Test: {0} ({1} ms)", dictionaryProjectionTest / directMemberTest, dictionaryProjectionTest);
            Console.WriteLine("JSON Projection Test: {0} ({1} ms)", jsonProjectionTest / directMemberTest, jsonProjectionTest);
            Console.WriteLine("Reflection Test: {0} ({1} ms)", reflectionMemberTest / directMemberTest, reflectionMemberTest);

            /*
                Direct Member Test: 1 (34 ms)
                Dictionary Projection Test: 24 (848 ms)
                JSON Projection Test: 25 (875 ms)  -  Here the result running NUnit is unexplainable worse
                Reflection Test: 99 (3371 ms)
             */
        }

        public class Address
        {
            public CityDetails City { get; set; } = new CityDetails();
            public PersonDetails Person { get; set; } = new PersonDetails();

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>
                {
                    { nameof(City), City.ToDictionary() },
                    { nameof(Person), Person.ToDictionary() }
                };
                return d;
            }

            public JObject ToJson()
            {
                return JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(this));
            }

            public class CityDetails
            {
                public string Name { get; set; } = "New York";
                public string ZipCode { get; set; } = "00501";
                public string AreaCode { get; set; } = "631";

                public Dictionary<string, string> ToDictionary()
                {
                    return new Dictionary<string, string>
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
                    return new Dictionary<string, string>
                    {
                        {nameof(FirstName), FirstName},
                        {nameof(LastName), LastName}
                    };
                }
            }
        }
    }
}
