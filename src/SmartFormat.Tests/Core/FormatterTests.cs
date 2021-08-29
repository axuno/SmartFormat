using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class FormatterTests
    {
        private readonly object[] _errorArgs = { new FormatDelegate(format => throw new Exception("ERROR!")) };

        private static SmartFormatter GetSimpleFormatter()
        {
            var formatter = new SmartFormatter(); 
            formatter.AddExtensions(new DefaultFormatter());
            formatter.AddExtensions(new ReflectionSource(), new DefaultSource());
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
        public void Formatter_With_IList_Objects()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            Assert.That(formatter.Format("{0}{1}", new List<object>{0,1}), Is.EqualTo("01"));
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

        [TestCase(1, "There {People.Count:plural:is a person.|are {} people.}", false)]
        [TestCase(2, "There {People.Count:plural:is a person.|are {} people.}", false)]
        [TestCase(1, "There {People.Count:is a person.|are {} people.}", true)]
        [TestCase(2, "There {People.Count:is a person.|are {} people.}", true)]
        public void Nested_PlaceHolders_Pluralization(int numOfPeople, string format, bool markAsDefault)
        {
            var data = numOfPeople == 1
                ? new {People = new List<object> {new {Name = "Name 1", Age = 20}}}
                : new {People = new List<object> {new {Name = "Name 1", Age = 20}, new {Name = "Name 2", Age = 30}}};
            
            var formatter = new SmartFormatter();
            formatter.AddExtensions(new ReflectionSource());
            // Note: If pluralization AND conditional formatters are registers, the formatter
            //       name MUST be included in the format string, because both could return successful evaluation
            // Here, we register only pluralization:
            formatter.AddExtensions(new PluralLocalizationFormatter("en"){CanAutoDetect = markAsDefault}, new DefaultFormatter());
            
            var result = formatter.Format(format, data);
            
            Assert.That(result, numOfPeople == 1 ? Is.EqualTo("There is a person.") : Is.EqualTo("There are 2 people."));
        }

        [Test]
        public void Formatter_NotifyFormattingError()
        {
            var obj = new { Name = "some name" };
            var badPlaceholder = new List<string>();

            var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.Ignore}});
            formatter.OnFormattingFailure += (o, args) => badPlaceholder.Add(args.Placeholder);
            var res = formatter.Format("{NoName} {Name} {OtherMissing}", obj);
            Assert.That(badPlaceholder.Count == 2 && badPlaceholder[0] == "{NoName}" && badPlaceholder[1] == "{OtherMissing}");
        }

        [TestCase("\\{Test}", "\\Hello", false)]
        [TestCase(@"\\{Test}",@"\Hello",  true)]
        public void LeadingBackslashMustNotEscapeBraces(string format, string expected, bool convertCharacterStringLiterals)
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
                {StringFormatCompatibility = true, Parser = new ParserSettings {ConvertCharacterStringLiterals = convertCharacterStringLiterals}});

            var actual = smart.Format(format, new { Test = "Hello" });
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NullAndBoxedNullBehaveTheSame()
        {
            // see issue https://github.com/scottrippey/SmartFormat.NET/issues/101
            var smart = Smart.CreateDefaultSmartFormat();
            object? boxedNull = null;
            Assert.AreEqual(smart.Format("{0}", default(object)!), smart.Format("{0}", boxedNull!));
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
            var formatDetails = new FormatDetails(formatter, formatParsed, args, null, output);
            
            Assert.AreEqual(args, formatDetails.OriginalArgs);
            Assert.AreEqual(format, formatDetails.OriginalFormat.RawText);
            Assert.AreEqual(formatter.Settings, formatDetails.Settings);
        }

        [Test]
        public void Missing_FormatExtensions_Should_Throw()
        {
            var formatter = new SmartFormatter();
            // make sure we test against missing format extensions
            formatter.AddExtensions(new DefaultSource());

            Assert.That(formatter.FormatterExtensions.Count, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => formatter.Format("", Array.Empty<object>()));
        }

        [Test]
        public void Missing_SourceExtensions_Should_Throw()
        {
            var formatter = new SmartFormatter();
            // make sure we test against missing source extensions
            formatter.AddExtensions(new DefaultFormatter());

            Assert.That(formatter.SourceExtensions.Count, Is.EqualTo(0));
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
            
            Assert.That(formatter.SourceExtensions.Count, Is.EqualTo(0));
            Assert.That(formatter.RemoveSourceExtension<StringSource>(), Is.EqualTo(false));
        }

        [Test]
        public void Remove_Existing_Source()
        {
            var formatter = new SmartFormatter();
            
            Assert.That(formatter.SourceExtensions.Count, Is.EqualTo(0));
            formatter.AddExtensions(new StringSource());
            Assert.That(formatter.RemoveSourceExtension<StringSource>(), Is.EqualTo(true));
        }

        [Test]
        public void Remove_None_Existing_Formatter()
        {
            var formatter = new SmartFormatter();
            
            Assert.That(formatter.FormatterExtensions.Count, Is.EqualTo(0));
            Assert.That(formatter.RemoveFormatterExtension<DefaultFormatter>(), Is.EqualTo(false));
        }

        [Test]
        public void Remove_Existing_Formatter()
        {
            var formatter = new SmartFormatter();
            
            Assert.That(formatter.FormatterExtensions.Count, Is.EqualTo(0));
            formatter.AddExtensions(new DefaultFormatter());
            Assert.That(formatter.RemoveFormatterExtension<DefaultFormatter>(), Is.EqualTo(true));
        }

        [Test]
        public void Formatter_GetSourceExtension()
        {
            var formatter = GetSimpleFormatter();
            Assert.That(formatter.GetSourceExtensions().Count, Is.EqualTo(formatter.SourceExtensions.Count));
            Assert.That(formatter.GetSourceExtension<DefaultSource>(), Is.InstanceOf(typeof(DefaultSource)));  ;
        }

        [Test]
        public void Formatter_GetFormatterExtension()
        {
            var formatter = GetSimpleFormatter();
            Assert.That(formatter.GetFormatterExtensions().Count, Is.EqualTo(formatter.FormatterExtensions.Count));
            Assert.That(formatter.GetFormatterExtension<DefaultFormatter>(), Is.InstanceOf(typeof(DefaultFormatter)));  ;
        }

        [Test]
        public void Not_Existing_Formatter_Name_Should_Throw()
        {
            var smart = GetSimpleFormatter();
            Assert.That(() => smart.Format("{0:not_existing_formatter_name:}", new object()), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains("not_existing_formatter_name"));
        }
    }
}