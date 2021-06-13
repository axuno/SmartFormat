using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class NullFormatterTests
    {
        private SmartFormatter GetFormatter()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ISource[] { new ListFormatter(smart), new DefaultSource(smart), new ReflectionSource(smart) });
            smart.AddExtensions(new IFormatter[] { new NullFormatter(), new ListFormatter(smart), new DefaultFormatter()});
            return smart;
        }

        [Test]
        public void Null_Without_Format_Should_Be_Empty_String()
        {
            var smart = GetFormatter();
            smart.Settings.StringFormatCompatibility = false;
            Assert.That(smart.Format("{JustNull}", new { JustNull = default(object)} ), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Null_Without_Format_Should_Be_Empty_String_CompatibilityMode()
        {
            var smart = GetFormatter();
            smart.Settings.StringFormatCompatibility = true;
            Assert.That(smart.Format("{JustNull}", new { JustNull = default(object)} ), Is.EqualTo(string.Empty));
        }

        [TestCase("")] // empty format
        [TestCase("nothing")]
        public void Null_With_Format_Should_Be_Format_String(string format)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format($"{{JustNull:{format}}}", new { JustNull = default(object)} ), Is.EqualTo(format));
        }

        [TestCase("")] // empty format
        [TestCase("something")]
        public void NotNull_Should_Be_Empty_String(string format)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format($"{{NotNull:{format}}}", new { NotNull = ""} ), Is.EqualTo(string.Empty));
        }

        [TestCase(null)]
        [TestCase("a string")]
        [TestCase(new long[] {5, 6, 7})]
        public void Format_Should_Only_Be_Output_If_Argument_Is_Null(object? value)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format("{TheValue:isnull:}", new {TheValue = value}), Is.EqualTo(string.Empty));
            Assert.That(smart.Format("{TheValue:isnull:nothing}", new {TheValue = value}),
                value == null ? Is.EqualTo("nothing") : Is.EqualTo(string.Empty));
        }

        [TestCase(null, "Argument is null")]
        [TestCase("Argument has this value", "Argument has this value")]
        public void Combine_NullFormatter_With_Other_Formatter(object value, string expected)
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new ISource[] { new DefaultSource(smart) });
            smart.AddExtensions(new IFormatter[] { new NullFormatter(), new DefaultFormatter()});
            
            // NullFormatter will output only, if value is null
            // DefaultFormatter will render null as string.Empty
            var result = smart.Format($"{{0:isnull:{expected}}}{{0}}", value!);
            
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
