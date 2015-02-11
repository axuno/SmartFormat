using NUnit.Framework;
using SmartFormat.Core.Formatting;

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
		[Test]
		public void Choose_should_work_with_numbers_and_strings(string format, object arg0, string expectedResult)
		{
			Assert.AreEqual(expectedResult, Smart.Format(format, arg0));
		}
		
		[TestCase("{0:choose(1|2|3):one|two|three|default}", 1, "one")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", 2, "two")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", 3, "three")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", 4, "default")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", 99, "default")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", null, "default")]
		[TestCase("{0:choose(1|2|3):one|two|three|default}", "whatever", "default")]
		[Test]
		public void Choose_should_default_to_the_last_item(string format, object arg0, string expectedResult)
		{
			Assert.AreEqual(expectedResult, Smart.Format(format, arg0));
		}

		[TestCase("{0:choose(Male|Female):man|woman}", Gender.Male, "man")]
		[TestCase("{0:choose(Male|Female):man|woman}", Gender.Female, "woman")]
		[TestCase("{0:choose(Male):man|woman}", Gender.Male, "man")]
		[TestCase("{0:choose(Male):man|woman}", Gender.Female, "woman")]
		[Test]
		public void Choose_should_work_with_enums(string format, object arg0, string expectedResult)
		{
			Assert.AreEqual(expectedResult, Smart.Format(format, arg0));
		}
		
		[TestCase("{0:choose(null):nothing|{} }", null, "nothing")]
		[TestCase("{0:choose(null):nothing|{} }", 5, "5 ")]
		[TestCase("{0:choose(null|5):nothing|five|{} }", null, "nothing")]
		[TestCase("{0:choose(null|5):nothing|five|{} }", 5, "five")]
		[TestCase("{0:choose(null|5):nothing|five|{} }", 6, "6 ")]
		[Test]
		public void Choose_has_a_special_case_for_null(string format, object arg0, string expectedResult)
		{
			Assert.AreEqual(expectedResult, Smart.Format(format, arg0));
		}

		[TestCase("{0:choose(1|2):1|2}", 99)]
		[TestCase("{0:choose(1):1}", 99)]
		[Test]
		[ExpectedException(typeof(FormattingException))]
		public void Choose_throws_when_choice_is_invalid(string format, object arg0)
		{
			Smart.Format(format, arg0);
		}

		// Too few choices:
		[TestCase("{0:choose(1|2):1}", 1)]
		[TestCase("{0:choose(1|2|3):1|2}", 1)]
		// Too many choices:
		[TestCase("{0:choose(1):1|2|3}", 1)]
		[TestCase("{0:choose(1|2):1|2|3|4}", 1)]
		[Test]
		[ExpectedException(typeof(FormattingException))]
		public void Choose_throws_when_choices_are_too_few_or_too_many(string format, object arg0)
		{
			Smart.Format(format, arg0);
		}

	}
}