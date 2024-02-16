using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class FormatterTests
{
    private readonly object[] _errorArgs = { new FormatDelegate(format => throw new Exception("ERROR!")) };

    private static SmartFormatter GetSimpleFormatter()
    {
        var formatter = new SmartFormatter()
            .AddExtensions(new DefaultFormatter())
            .AddExtensions(new ReflectionSource(), new DefaultSource());
        return formatter;
    }

    [Test]
    public void Formatter_With_Numeric_Params_Objects()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        Assert.That(formatter.Format("ABC{0}{1}DEF", 0, 1), Is.EqualTo("ABC01DEF"));
    }

    [Test]
    public void Formatter_With_String_Params_Objects()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        Assert.That(formatter.Format("Name: {0}", "Joe"), Is.EqualTo("Name: Joe"));
    }

    [Test]
    public void Formatter_Pure_Literal_No_Args()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        var parsed = formatter.Parser.ParseFormat("ABC");
        Assert.That(formatter.Format(parsed), Is.EqualTo("ABC"));
    }

    [Test]
    public void Formatter_With_Null_Args()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        Assert.That(formatter.Format("a{0}b{1}c", null, null), Is.EqualTo("abc"));
    }
        
    [Test]
    public void Formatter_With_IList_Objects()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        Assert.That(formatter.Format("{0}{1}", new List<object?>{0,1}), Is.EqualTo("01"));
    }

    [Test]
    public void Formatter_With_IList_Null()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        Assert.That(formatter.Format("a{0}{1}b", new List<object?>{null, null}), Is.EqualTo("ab"));
    }

    [Test]
    public void Formatter_Throws_Exceptions()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});

        Assert.Throws<FormattingException>(() => formatter.Test("--{0}--", _errorArgs, "--ERROR!--ERROR!--"));
    }

    [Test]
    public void Formatter_Outputs_Exceptions()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.OutputErrorInResult}});

        formatter.Test("--{0}--{0:ZZZZ}--", _errorArgs, "--ERROR!--ERROR!--");
    }

    [Test]
    public void Formatter_Ignores_Exceptions()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.Ignore}});

        formatter.Test("--{0}--{0:ZZZZ}--", _errorArgs, "------");
    }

    [Test]
    public void Formatter_Maintains_Tokens()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings{Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.MaintainTokens}});

        formatter.Test("--{0}--{0:ZZZZ}--", _errorArgs, "--{0}--{0:ZZZZ}--");
    }

    [Test]
    public void Formatter_Maintains_Object_Tokens()
    {
        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.MaintainTokens}});
        formatter.Test("--{Object.Thing}--", _errorArgs, "--{Object.Thing}--");
    }

    [Test]
    public void Nested_Placeholders_Braces()
    {
        var data = new {Person = new {FirstName = "John", LastName = "Long"}, Address = new {City = "London"}};
        var formatter = Smart.CreateDefaultSmartFormat();
            
        // This allows a nested template to access outer scopes.
        // Here, {City} will come from Address, but {FirstName} will come from Person:
        var result = formatter.Format("{Person:{Address:City\\: {City}, Name\\: {FirstName}}}", data);
            
        Assert.That(result, Is.EqualTo("City: London, Name: John"));
    }

    [TestCase("({.Joe.})", ":{Joe}:")]
    [TestCase("Kate", ":{(.Not:Joe.)}:")]
    public void Any_Character_Anywhere_If_Escaped(string name, string expected)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var arg = new {Name = name};
        // {} and () must and can only be escaped inside options
        var format = @":\{{Name:choose(\(\{.Joe.\}\)):Joe|(.Not\:Joe.)}\}:";
        Assert.That(smart.Format(format, arg), Is.EqualTo(expected));
    }

    [Test]
    public void Formatter_NotifyFormattingError()
    {
        var obj = new { Name = "some name" };
        var badPlaceholder = new List<FormattingErrorEventArgs>();

        var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.Ignore}});
        formatter.OnFormattingFailure += (o, args) => badPlaceholder.Add(args);
        var res = formatter.Format("{NoName} {Name} {OtherMissing}", obj);
        Assert.That(badPlaceholder, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(badPlaceholder[0].Placeholder, Is.EqualTo("{NoName}"));
            Assert.That(badPlaceholder[1].Placeholder, Is.EqualTo("{OtherMissing}"));
            Assert.That(badPlaceholder[0].ErrorIndex, Is.EqualTo(7));
            Assert.That(badPlaceholder[0].IgnoreError, Is.EqualTo(true));
        });
    }

    [TestCase("\\{Test}", "\\Hello", false)]
    [TestCase(@"\\{Test}",@"\Hello",  true)]
    public void LeadingBackslashMustNotEscapeBraces(string format, string expected, bool convertCharacterStringLiterals)
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            {StringFormatCompatibility = true, Parser = new ParserSettings {ConvertCharacterStringLiterals = convertCharacterStringLiterals}});

        var actual = smart.Format(format, new { Test = "Hello" });
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void NullAndBoxedNullBehaveTheSame()
    {
        // see issue https://github.com/scottrippey/SmartFormat.NET/issues/101
        var smart = Smart.CreateDefaultSmartFormat();
        object? boxedNull = null;
        Assert.That(smart.Format("{0}", boxedNull!), Is.EqualTo(smart.Format("{0}", default(object)!)));
    }

    [Test]
    public void SmartFormatter_FormatDetails()
    {
        var args = new object[] {new Dictionary<string, string> {{"Greeting", "Hello"}} };
        var format = "{Greeting}";
        var output = new StringOutput();
        var formatter = new SmartFormatter(new SmartSettings
        {
            CaseSensitivity = CaseSensitivityType.CaseInsensitive,
            Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.OutputErrorInResult},
            Parser = new ParserSettings {ErrorAction = ParseErrorAction.OutputErrorInResult}
        });
        var formatParsed = formatter.Parser.ParseFormat(format);
        var formatDetails = new FormatDetails().Initialize(formatter, formatParsed, args, null, output);

        Assert.Multiple(() =>
        {
            Assert.That(formatDetails.OriginalArgs, Is.EqualTo(args));
            Assert.That(formatDetails.OriginalFormat.RawText, Is.EqualTo(format));
            Assert.That(formatDetails.Settings, Is.EqualTo(formatter.Settings));
        });
    }

    [Test]
    public void Missing_FormatExtensions_Should_Throw()
    {
        var formatter = new SmartFormatter();
        // make sure we test against missing format extensions
        formatter.AddExtensions(new DefaultSource());

        Assert.That(formatter.FormatterExtensions, Is.Empty);
        Assert.Throws<InvalidOperationException>(() => formatter.Format("", Array.Empty<object>()));
    }

    [Test]
    public void Missing_SourceExtensions_Should_Throw()
    {
        var formatter = new SmartFormatter();
        // make sure we test against missing source extensions
        formatter.AddExtensions(new DefaultFormatter());

        Assert.That(formatter.SourceExtensions, Is.Empty);
        Assert.Throws<InvalidOperationException>(() => formatter.Format("", Array.Empty<object>()));
    }

    [Test]
    public void Adding_FormatExtension_With_Existing_Name_Should_Throw()
    {
        var formatter = new SmartFormatter();
        var firstExtension = new DefaultFormatter();
        formatter.AddExtensions(firstExtension);
        var dupeExtension = new NullFormatter {Name = firstExtension.Name};
        Assert.That(() => formatter.AddExtensions(dupeExtension), Throws.TypeOf(typeof(ArgumentException)));
    }

    [Test]
    public void Remove_None_Existing_Source()
    {
        var formatter = new SmartFormatter();
        Assert.Multiple(() =>
        {
            Assert.That(formatter.SourceExtensions, Is.Empty);
            Assert.That(formatter.RemoveSourceExtension<StringSource>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void Remove_Existing_Source()
    {
        var formatter = new SmartFormatter();
            
        Assert.That(formatter.SourceExtensions, Is.Empty);
        formatter.AddExtensions(new StringSource());
        Assert.That(formatter.RemoveSourceExtension<StringSource>(), Is.EqualTo(true));
    }

    [Test]
    public void Remove_None_Existing_Formatter()
    {
        var formatter = new SmartFormatter();
        Assert.Multiple(() =>
        {
            Assert.That(formatter.FormatterExtensions, Is.Empty);
            Assert.That(formatter.RemoveFormatterExtension<DefaultFormatter>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void Remove_Existing_Formatter()
    {
        var formatter = new SmartFormatter();
            
        Assert.That(formatter.FormatterExtensions, Is.Empty);
        formatter.AddExtensions(new DefaultFormatter());
        Assert.That(formatter.RemoveFormatterExtension<DefaultFormatter>(), Is.EqualTo(true));
    }

    [Test]
    public void Formatter_GetSourceExtension()
    {
        var formatter = GetSimpleFormatter();
        Assert.Multiple(() =>
        {
            Assert.That(formatter.GetSourceExtensions(), Has.Count.EqualTo(formatter.SourceExtensions.Count));
            Assert.That(formatter.GetSourceExtension<DefaultSource>(), Is.InstanceOf(typeof(DefaultSource)));
        });
        ;
    }

    [Test]
    public void Formatter_GetFormatterExtension()
    {
        var formatter = GetSimpleFormatter();
        Assert.Multiple(() =>
        {
            Assert.That(formatter.GetFormatterExtensions(), Has.Count.EqualTo(formatter.FormatterExtensions.Count));
            Assert.That(formatter.GetFormatterExtension<DefaultFormatter>(), Is.InstanceOf(typeof(DefaultFormatter)));
        });
        ;
    }

    [Test]
    public void Not_Existing_Formatter_Name_Should_Throw()
    {
        var smart = GetSimpleFormatter();
        Assert.That(() => smart.Format("{0:not_existing_formatter_name:}", new object()), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("not_existing_formatter_name"));
    }

    [Test]
    public void Parallel_Smart_Format()
    {
        // Switch to thread safety - otherwise the test would throw an InvalidOperationException
        var savedMode = ThreadSafeMode.SwitchOn();
            
        var results = new ConcurrentDictionary<long, string>();
        var threadIds = new ConcurrentDictionary<int, int>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 100 };
        long resultCounter = 0;
        long formatterInstancesCounter = 0;

        Assert.That(code: () =>
            Parallel.For(0L, 1000, options, (i, loopState) =>
            {
                // If the ChooseFormatter extension does not exist,
                // we re-use an existing SmartFormatter instance
                if (Smart.Default.GetFormatterExtension<ChooseFormatter>() is not null)
                {
                    // Remove an extension we don't need for the test
                    if (Smart.Default.RemoveFormatterExtension<ChooseFormatter>())
                        Interlocked.Increment(ref formatterInstancesCounter);
                }

                // register unique thread ids
                threadIds.TryAdd(Environment.CurrentManagedThreadId, Environment.CurrentManagedThreadId);
                // Smart.Default is a thread-static instance of the SmartFormatter,
                // which is used here
                results.TryAdd(i, Smart.Format("{0}", i));
                Interlocked.Increment(ref resultCounter);
            }), Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(threadIds, Has.Count.AtLeast(2)); // otherwise the test is not significant
            Assert.That(Smart.CreateDefaultSmartFormat().GetFormatterExtension<ChooseFormatter>(), Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(threadIds, Has.Count.EqualTo(formatterInstancesCounter));
            Assert.That(results, Has.Count.EqualTo(resultCounter));
        });

        // Restore to saved value
        ThreadSafeMode.SwitchTo(savedMode);
    }

    public class AbortFormatSource : ISource
    {
        public string? Message { get; set; }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (selectorInfo.SelectorText == "abort")
            {
                if (Message == null)
                    throw new AbortFormattingException();
                throw new AbortFormattingException(Message);
            }
            return false;
        }
    }

    [TestCase("Some text {abort}", "Some text ")]
    [TestCase("Some text {1.abort}", "Some text ")]
    [TestCase("Some text {0.abort.some.selector}", "Some text ")]
    [TestCase("Some text {abort.some.selector:plural:Apple|{} Apples}", "Some text ")]
    public void AbortFormattingException_PreventsFurtherProcessing(string text, string expected)
    {
        var formatter = GetSimpleFormatter();
        formatter.AddExtensions(new AbortFormatSource());

        var actual = formatter.Format(text, 1, 2);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("Some text {abort}", "", "Some text ")]
    [TestCase("Some text {1.abort}", "Message", "Some text Message")]
    [TestCase("Some text {0.abort.some.selector}", "A Message", "Some text A Message")]
    [TestCase("Some text {abort.some.selector:plural:Apple|{} Apples}", "<Error>", "Some text <Error>")]
    public void AbortFormattingException_PreventsFurtherProcessingAndIncludesMessage(string text, string message, string expected)
    {
        var formatter = GetSimpleFormatter();
        formatter.AddExtensions(new AbortFormatSource { Message = message });

        var actual = formatter.Format(text, 1, 2);
        Assert.That(actual, Is.EqualTo(expected));
    }
}
