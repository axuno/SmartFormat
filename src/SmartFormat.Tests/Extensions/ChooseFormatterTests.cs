using System;
using System.Globalization;
using FluentAssertions.Formatting;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class ChooseFormatterTests
    {
        [TestCase("{0:choose(1|2|3):one|two|three}", 1, "one")]
        [TestCase("{0:choose(1|2|3):one|two|three}", 2, "two")]
        [TestCase("{0:choose(1|2|3):one|two|three}", 3, "three")]

        [TestCase("{0:choose(3|2|1):three|two|one}", 1, "one")]
        [TestCase("{0:choose(3|2|1):three|two|one}", 2, "two")]
        [TestCase("{0:choose(3|2|1):three|two|one}", 3, "three")]

        [TestCase("{0:choose(1|2|3):one|two|three}", "1", "one")]
        [TestCase("{0:choose(1|2|3):one|two|three}", "2", "two")]
        [TestCase("{0:choose(1|2|3):one|two|three}", "3", "three")]

        [TestCase("{0:choose(A|B|C):Alpha|Bravo|Charlie}", "A", "Alpha")]
        [TestCase("{0:choose(A|B|C):Alpha|Bravo|Charlie}", "B", "Bravo")]
        [TestCase("{0:choose(A|B|C):Alpha|Bravo|Charlie}", "C", "Charlie")]

        [TestCase("{0:choose(True|False):yep|nope}", true, "yep")]
        [TestCase("{0:choose(True|False):yep|nope}", false, "nope")]
        public void Choose_should_work_with_numbers_strings_and_booleans(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.AreEqual(expectedResult, smart.Format(format, arg0));
        }

        [TestCase("{0:choose(true|True):one|two|default}", true, "two")]
        [TestCase("{0:choose(true|TRUE):one|two|default}", true, "default")]
        [TestCase("{0:choose(string|String):one|two|default}", "String", "two")]
        [TestCase("{0:choose(string|STRING):one|two|default}", "String", "default")]
        [TestCase("{0:choose(ignore|Ignore):one|two|default}", SmartFormat.Core.Settings.FormatErrorAction.Ignore, "two")]
        [TestCase("{0:choose(ignore|IGNORE):one|two|default}", SmartFormat.Core.Settings.FormatErrorAction.Ignore, "default")]
        public void Choose_should_be_case_sensitive(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.AreEqual(expectedResult, smart.Format(format, arg0));
        }
        
        [TestCase("{0:choose(1|2|3):one|two|three|default}", 1, "one")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", 2, "two")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", 3, "three")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", 4, "default")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", 99, "default")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", null, "default")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", true, "default")]
        [TestCase("{0:choose(1|2|3):one|two|three|default}", "whatever", "default")]
        public void Choose_should_default_to_the_last_item(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.AreEqual(expectedResult, smart.Format(format, arg0));
        }

        [TestCase("{0:choose(Male|Female):man|woman}", Gender.Male, "man")]
        [TestCase("{0:choose(Male|Female):man|woman}", Gender.Female, "woman")]
        [TestCase("{0:choose(Male):man|woman}", Gender.Male, "man")]
        [TestCase("{0:choose(Male):man|woman}", Gender.Female, "woman")]
        public void Choose_should_work_with_enums(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.AreEqual(expectedResult, smart.Format(format, arg0));
        }
        
        [TestCase("{0:choose(null):nothing|{}}", null, "nothing")]
        [TestCase("{0:choose(null):nothing|{}}", 5, "5")]
        [TestCase("{0:choose(null|5):nothing|five|{}}", null, "nothing")]
        [TestCase("{0:choose(null|5):nothing|five|{}}", 5, "five")]
        [TestCase("{0:choose(null|5):nothing|five|{}}", 6, "6")]
        public void Choose_has_a_special_case_for_null(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.AreEqual(expectedResult, smart.Format(format, arg0));
        }

        [TestCase("{0:choose(1|2):1|2}", 99)]
        [TestCase("{0:choose(1):1}", 99)]
        public void Choose_throws_when_choice_is_invalid(string format, object arg0)
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            Assert.Throws<FormattingException>(() => smart.Format(format, arg0));
        }

        // Too few choices:
        [TestCase("{0:choose(1|2):1}", 1)]
        [TestCase("{0:choose(1|2|3):1|2}", 1)]
        // Too many choices:
        [TestCase("{0:choose(1):1|2|3}", 1)]
        [TestCase("{0:choose(1|2):1|2|3|4}", 1)]
        public void Choose_throws_when_choices_are_too_few_or_too_many(string format, object arg0)
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            Assert.Throws<FormattingException>(() => smart.Format(format, arg0));
        }

        [TestCase(1234, 9999, "1,234.00")]
        [TestCase(null, 9999, "9,999.00")]
        public void May_Contain_Nested_Formats_As_Choice(int? nullableInt, int valueIfNull, string expected)
        {
            var data = new { NullableInt = nullableInt, IntValueIfNull = valueIfNull};
            var smart = Smart.CreateDefaultSmartFormat();
            var result = smart.Format(CultureInfo.InvariantCulture, "{NullableInt:choose(null):{IntValueIfNull:N2}|{:N2}}", data);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(1234, 9999, "1,234.00")] // first choose formatter
        [TestCase(null, 1000, "1k")] // first + nested choose formatter, option 1
        [TestCase(null, 2000, "2k")] // first + nested choose formatter, option 2
        public void May_Contain_Nested_Choose_Formats(int? nullableInt, int valueIfNull, string expected)
        {
            var data = new { NullableInt = nullableInt, IntValueIfNull = valueIfNull};
            var smart = Smart.CreateDefaultSmartFormat();
            var result = smart.Format(CultureInfo.InvariantCulture, "{NullableInt:choose(null):{IntValueIfNull:choose(1000|2000):1k|2k}|{:N2}}", data);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}