﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions;

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
        Assert.That(smart.Format(format, arg0), Is.EqualTo(expectedResult));
    }

    [Test]
    public void Choose_With_Changed_SplitChar()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        // Set SplitChar from | to ~, so we can use | for the output string
        smart.GetFormatterExtension<ChooseFormatter>()!.SplitChar = '~';
        var result = smart.Format("{0:choose(1~2~3):|one|~|two|~|three|}", 2);
        Assert.That(result, Is.EqualTo("|two|"));
    }

    // bool and null args: always case-insensitive
    [TestCase("{0:choose(true|false):one|two|default}", false, true, "one")]
    [TestCase("{0:choose(True|FALSE):one|two|default}", false, false, "two")]
    [TestCase("{0:choose(null):is null|default}", false, null, "is null")]
    [TestCase("{0:choose(NULL):is null|default}", false, null, "is null")]
    // strings
    [TestCase("{0:choose(string|String):one|two|default}", true, "String", "two")]
    [TestCase("{0:choose(string|STRING):one|two|default}", true, "String", "default")]
    // Enum
    [TestCase("{0:choose(ignore|Ignore):one|two|default}", true, FormatErrorAction.Ignore, "two")]
    [TestCase("{0:choose(ignore|IGNORE):one|two|default}", true, FormatErrorAction.Ignore, "default")]
    public void Choose_should_be_case_sensitive(string format, bool caseSensitive, object? arg0, string expectedResult)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        smart.GetFormatterExtension<ChooseFormatter>()!.CaseSensitivity =
            caseSensitive ? CaseSensitivityType.CaseSensitive : CaseSensitivityType.CaseInsensitive;
        Assert.That(smart.Format(format, arg0), Is.EqualTo(expectedResult));
    }
        
    [TestCase("{0:choose(1|2|3):one|two|three|default}", 1, "one")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", 2, "two")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", 3, "three")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", 4, "default")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", 99, "default")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", null, "default")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", true, "default")]
    [TestCase("{0:choose(1|2|3):one|two|three|default}", "whatever", "default")]
    public void Choose_should_default_to_the_last_item(string format, object? arg0, string expectedResult)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(smart.Format(format, arg0), Is.EqualTo(expectedResult));
    }

    [TestCase("{0:choose(Male|Female):man|woman}", Gender.Male, "man")]
    [TestCase("{0:choose(Male|Female):man|woman}", Gender.Female, "woman")]
    [TestCase("{0:choose(Male):man|woman}", Gender.Male, "man")]
    [TestCase("{0:choose(Male):man|woman}", Gender.Female, "woman")]
    public void Choose_should_work_with_enums(string format, object arg0, string expectedResult)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(smart.Format(format, arg0), Is.EqualTo(expectedResult));
    }
        
    [TestCase("{0:choose(null):nothing|{}}", null, "nothing")]
    [TestCase("{0:choose(null):nothing|{}}", 5, "5")]
    [TestCase("{0:choose(null|5):nothing|five|{}}", null, "nothing")]
    [TestCase("{0:choose(null|5):nothing|five|{}}", 5, "five")]
    [TestCase("{0:choose(null|5):nothing|five|{}}", 6, "6")]
    public void Choose_has_a_special_case_for_null(string format, object? arg0, string expectedResult)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.That(smart.Format(format, arg0), Is.EqualTo(expectedResult));
    }

    [TestCase(null, "Must be a word like 'good', 'bad'")]
    [TestCase("", "'' is not a word")]
    [TestCase("good", "good")]
    [TestCase("bad", "bad")]
    public void Choose_Result_May_Contain_Placeholders(string? input, string expected)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var arg = new {
            Input = input,
            Examples = new[] { "good", "bad" }
        };

        var result = smart.Format("{Input:choose(null|):Must be a word like {Examples:list:'{}'|, }|'' is not a word|{}}", arg);
        Assert.That(result, Is.EqualTo(expected));
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

    [Test, Description("Case-insensitive option string comparison")]
    public void Choose_Should_Use_CultureInfo_For_Option_Strings()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        var smart = Smart.CreateDefaultSmartFormat();
        smart.GetFormatterExtension<ChooseFormatter>()!.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

        var result1 = smart.Format(CultureInfo.GetCultureInfo("de"), "{0:choose(ä|ü):umlautA|umlautU}", "Ä");
        var result2 = smart.Format(CultureInfo.GetCultureInfo("de"), "{0:choose(ä|ü):umlautA|umlautU}", "ä");

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo("umlautA"));
            Assert.That(result2, Is.EqualTo("umlautA"));
        });
    }

    [Test]
    public void Parallel_ChooseFormatter()
    {
        // Switch to thread safety - otherwise the test would throw an InvalidOperationException
        var savedMode = ThreadSafeMode.SwitchOn();

        var results = new ConcurrentDictionary<long, string>();
        var threadIds = new ConcurrentDictionary<int, int>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 100 };
        long resultCounter = 0;

        // One instance for all threads
        var smart = new SmartFormatter().AddExtensions(new DefaultSource())
            .AddExtensions(new DefaultFormatter(), new ChooseFormatter());

        Assert.That(code: () =>
            Parallel.For(0L, 1000, options, (i, loopState) =>
            {
                // register unique thread ids
                threadIds.TryAdd(Environment.CurrentManagedThreadId, Environment.CurrentManagedThreadId);

                // Re-use the same SmartFormatter instance, where the Format method is thread-safe.
                results.TryAdd(i, smart.Format("{0:D3} {1:choose(1|2|3):one|two|three|default}", i, 2));
                Interlocked.Increment(ref resultCounter);
            }), Throws.Nothing);

        var sortedResult = results.OrderBy(r => r.Value).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(threadIds, Has.Count.AtLeast(2)); // otherwise the test is not significant
            Assert.That(results, Has.Count.EqualTo(resultCounter));
            for (var i = 0; i < resultCounter; i++)
            {
                Assert.That(sortedResult[i].Value, Is.EqualTo(i.ToString("D3") + " two"));
            }
        });

        // Restore to saved value
        ThreadSafeMode.SwitchTo(savedMode);
    }
}
