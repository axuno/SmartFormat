using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class NullFormatterTests
    {
        private static SmartFormatter GetFormatter(SmartSettings? settings = null)
        {
            var smart = new SmartFormatter(settings ?? new SmartSettings());
            smart.AddExtensions(new ListFormatter(), new DefaultSource(), new ReflectionSource());
            smart.AddExtensions(new NullFormatter(), new ListFormatter(), new DefaultFormatter());
            return smart;
        }

        [Test]
        public void Null_Without_Format_Should_Be_Empty_String()
        {
            var smart = GetFormatter(new SmartSettings {StringFormatCompatibility = false});
            Assert.That(smart.Format("{JustNull}", new { JustNull = default(object)} ), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Null_Without_Format_Should_Be_Empty_String_CompatibilityMode()
        {
            var smart = GetFormatter(new SmartSettings {StringFormatCompatibility = true});
            Assert.That(smart.Format("{JustNull}", new { JustNull = default(object)} ), Is.EqualTo(string.Empty));
        }

        [TestCase("")] // empty format
        [TestCase("something")]
        public void NotNull_Should_Be_Empty_String(string format)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format($"{{NotNull:{format}}}", new { NotNull = ""} ), Is.EqualTo(string.Empty));
        }

        [TestCase("")] // empty format
        [TestCase("nothing")]
        public void Null_With_Format_Should_Be_Format_String(string format)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format($"{{JustNull:isnull:{format}}}", new { JustNull = default(object)} ), Is.EqualTo(format));
        }

        [TestCase(null, "")]
        [TestCase("a string", "")]
        [TestCase(new long[] {5, 6, 7}, "")]
        public void Empty_FormatOption_Should_Only_Be_Output_If_Argument_Is_Null(object? value, string expected)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format("{TheValue:isnull:}", new {TheValue = value}), Is.EqualTo(expected));
        }

        [TestCase(null, "It's null", "It's null")] // Null value and format for null
        [TestCase("a string", "It's null|It's a string", "It's a string")] // With format for "not null"
        [TestCase("a string", "It's null", "")] // No format for "not null"
        [TestCase(new long[] {5, 6, 7}, "It's null|Numbers", "Numbers")] // With format for "not null"
        public void FormatOption_Should_Only_Be_Output_If_Argument_Is_Null(object? value, string formats, string expected)
        {
            var smart = GetFormatter();
            Assert.That(smart.Format("{TheValue:isnull:" + formats + "}", new {TheValue = value}), Is.EqualTo(expected));
            Assert.That(smart.Format("{0:isnull:" + formats + "}", value), Is.EqualTo(expected));
        }

        [TestCase(null, 0, "Was null")] // first choose formatter
        [TestCase(123, 1000, "1k")] // first + nested choose formatter, option 1
        [TestCase(123, 2000, "2,000.00")] // first + nested choose formatter, option 2
        public void May_Contain_Nested_Formats(int? nullableInt, int valueIfNotNull, string expected)
        {
            var smart = GetFormatter();
            smart.AddExtensions(new ChooseFormatter());
            var data = new { NullableInt = nullableInt, IntValueIfNotNull = valueIfNotNull};
            var result = smart.Format(CultureInfo.InvariantCulture, "{NullableInt:isnull:Was null|{IntValueIfNotNull:choose(1000|2000):1k|{:N2}}}", data);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void NullFormatter_Must_Not_Contain_Choose_Options()
        {
            var smart = GetFormatter();
            Assert.That(() => smart.Format("{0:isnull(op|ti|ons):Is null}", 123), Throws.InstanceOf<FormattingException>());
        }

        [Test]
        public void NullFormatter_Format_Count_Must_Be_1_or_2()
        {
            var smart = GetFormatter();
            Assert.That(delegate { return smart.Format("{0:isnull:1|2|3}", 123); }, Throws.InstanceOf<FormattingException>(), "No format included");
        }
    }
}
