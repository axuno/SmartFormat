using System;
using System.Collections.Generic;
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
        private object[] errorArgs = new object[]{ new FormatDelegate(format => { throw new Exception("ERROR!"); } ) };

        private SmartFormatter GetSimpleFormatter()
        {
            var formatter = new SmartFormatter(); 
            formatter.FormatterExtensions.Add(new DefaultFormatter());
            formatter.SourceExtensions.Add(new ReflectionSource(formatter));
            formatter.SourceExtensions.Add(new DefaultSource(formatter));
            formatter.Parser.AddAlphanumericSelectors();
            return formatter;
        }

        [Test]
        public void Formatter_With_Params_Objects()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            Assert.That(formatter.Format("{0}{1}ABC", 0, 1), Is.EqualTo("01ABC"));
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
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;

            Assert.Throws<FormattingException>(() => formatter.Test("--{0}--", errorArgs, "--ERROR!--ERROR!--"));
        }

        [Test]
        public void Formatter_Outputs_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.OutputErrorInResult;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--ERROR!--ERROR!--");
        }

        [Test]
        public void Formatter_Ignores_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.Ignore;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "------");
        }

        [Test]
        public void Formatter_Maintains_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.MaintainTokens;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--{0}--{0:ZZZZ}--");
        }

        [Test]
        public void Formatter_Maintains_Object_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.MaintainTokens;
            formatter.Test("--{Object.Thing}--", errorArgs, "--{Object.Thing}--");
        }

        [Test]
        public void Formatter_Align()
        {
            string name = "Joe";
            var obj = new { name = name };
            var str2 = GetSimpleFormatter().Format("Name: {name,10}| Column 2", obj);
            Assert.That(str2, Is.EqualTo("Name:        Joe| Column 2"));
        }

        [Test]
        public void Formatter_AlignNull()
        {
            string? name = null;
            var obj = new { name = name };
            var str2 = GetSimpleFormatter().Format("Name: {name,-10}| Column 2", obj);
            Assert.That(str2, Is.EqualTo("Name:           | Column 2"));
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

        [TestCase(1)]
        [TestCase(2)]
        public void Nested_PlaceHolders_Conditional(int numOfPeople)
        {

            var data = numOfPeople == 1
                ? new {People = new List<object> {new {Name = "Name 1", Age = 20}}}
                : new {People = new List<object> {new {Name = "Name 1", Age = 20}, new {Name = "Name 2", Age = 30}}};
            var formatter = Smart.CreateDefaultSmartFormat();
            
            var result = formatter.Format("There {People.Count:is a person.|are {} people.}", data);
            
            Assert.That(result, numOfPeople == 1 ? Is.EqualTo("There is a person.") : Is.EqualTo("There are 2 people."));
        }

        [Test]
        public void Formatter_NotifyFormattingError()
        {
            var obj = new { Name = "some name" };
            var badPlaceholder = new List<string>();

            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.Ignore;
            formatter.OnFormattingFailure += (o, args) => badPlaceholder.Add(args.Placeholder);
            var res = formatter.Format("{NoName} {Name} {OtherMissing}", obj);
            Assert.That(badPlaceholder.Count == 2 && badPlaceholder[0] == "{NoName}" && badPlaceholder[1] == "{OtherMissing}");
        }

        [Test]
        public void LeadingBackslashMustNotEscapeBraces()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.Parser.ConvertCharacterStringLiterals = false;
            smart.Settings.UseStringFormatCompatibility = true;

            var expected = "\\Hello";
            var actual = smart.Format("\\{Test}", new { Test = "Hello" });
            Assert.AreEqual(expected, actual);

            smart.Settings.Parser.ConvertCharacterStringLiterals = true;

            expected = @"\Hello";
            actual = smart.Format(@"\\{Test}", new { Test = "Hello" }); // double backslash means escaping the backslash
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
            var formatter = new SmartFormatter();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            formatter.Settings.ConvertCharacterStringLiterals = true;
            formatter.Settings.Formatter.ErrorAction = FormatErrorAction.OutputErrorInResult;
            formatter.Settings.Parser.ErrorAction = ParseErrorAction.OutputErrorInResult;
            formatter.Parser.AddAlphanumericSelectors(); // required for this test
            var formatParsed = formatter.Parser.ParseFormat(format);
            var formatDetails = new FormatDetails(formatter, formatParsed, args, null, null, output);
            
            Assert.AreEqual(args, formatDetails.OriginalArgs);
            Assert.AreEqual(format, formatDetails.OriginalFormat.RawText);
            Assert.AreEqual(formatter.Settings, formatDetails.Settings);
            Assert.IsTrue(formatDetails.FormatCache == null);
        }

        [Test]
        public void Missing_FormatExtensions_Should_Throw()
        {
            var formatter = GetSimpleFormatter();

            formatter.FormatterExtensions.Clear();
            Assert.Throws<InvalidOperationException>(() => formatter.Format("", Array.Empty<object>()));
        }

        [Test]
        public void Missing_SourceExtensions_Should_Throw()
        {
            var formatter = GetSimpleFormatter();

            formatter.SourceExtensions.Clear();
            Assert.Throws<InvalidOperationException>(() => formatter.Format("", Array.Empty<object>()));
        }

        [Test]
        public void Formatter_GetSourceExtension()
        {
            var formatter = GetSimpleFormatter();
            Assert.That(formatter.GetSourceExtension<DefaultSource>(), Is.InstanceOf(typeof(DefaultSource)));  ;
        }
    }
}