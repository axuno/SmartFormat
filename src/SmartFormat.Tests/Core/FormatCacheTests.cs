using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    class FormatCacheTests
    {
        private SmartFormatter GetSimpleFormatter()
        {
            var formatter = new SmartFormatter(); 
            formatter.FormatterExtensions.Add(new DefaultFormatter());
            formatter.SourceExtensions.Add(new ReflectionSource(formatter));
            formatter.SourceExtensions.Add(new DefaultSource(formatter));
            return formatter;
        }

        [Test]
        public void Create_Cache()
        {
            var sf = new SmartFormatter();
            var format = new Format(sf.Settings, "the base string");
            var fc = new FormatCache(format);
            Assert.AreEqual(format, fc.Format);
            Assert.IsAssignableFrom<Dictionary<string,object>>(fc.CachedObjects);
            fc.CachedObjects.Add("key", "value");
            Assert.IsTrue(fc.CachedObjects["key"].ToString() == "value");
        }

        [Test]
        public void Format_WithCache()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString);
            var cache = new FormatCache(format);
            Assert.That(formatter.FormatWithCache(ref cache, formatString, data), Is.EqualTo($"{data.Name}, {data.City}"));
        }

        [Test]
        public void Format_WithCache_Into()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString);
            var cache = new FormatCache(format);
            var output = new StringOutput();
            formatter.FormatWithCacheInto(ref cache, output, formatString, data);
            Assert.That(output.ToString(), Is.EqualTo($"{data.Name}, {data.City}"));
        }
    }
}
