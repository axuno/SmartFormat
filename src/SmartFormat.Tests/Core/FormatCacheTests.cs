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
            formatter.AddExtensions(new DefaultFormatter());
            formatter.AddExtensions(new ReflectionSource(), new DefaultSource());
            return formatter;
        }

        [Test]
        public void Format_WithCache()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString);
            Assert.That(formatter.Format(format, data), Is.EqualTo($"{data.Name}, {data.City}"));
        }

        [Test]
        public void Format_WithCache_Into()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString);
            var output = new StringOutput();
            formatter.FormatInto(output, format, data);
            Assert.That(output.ToString(), Is.EqualTo($"{data.Name}, {data.City}"));
        }
    }
}
