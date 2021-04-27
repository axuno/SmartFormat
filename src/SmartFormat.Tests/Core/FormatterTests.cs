﻿using System;
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
            Assert.That(formatter.Format("{0}{1}", 0, 1), Is.EqualTo("01"));
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
            formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;

            Assert.Throws<FormattingException>(() => formatter.Test("--{0}--", errorArgs, "--ERROR!--ERROR!--"));
        }

        [Test]
        public void Formatter_Outputs_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.OutputErrorInResult;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--ERROR!--ERROR!--");
        }

        [Test]
        public void Formatter_Ignores_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.Ignore;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "------");
        }

        [Test]
        public void Formatter_Maintains_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.MaintainTokens;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--{0}--{0:ZZZZ}--");
        }

        [Test]
        public void Formatter_Maintains_Object_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.MaintainTokens;
            formatter.Test("--{Object.Thing}--", errorArgs, "--{Object.Thing}--");
        }

        [Test]
        public void Formatter_AlignNull()
        {
            string? name = null;
            var obj = new { name = name };
            var str2 = Smart.Format("Name: {name,-10}| Column 2", obj);
            Assert.That(str2, Is.EqualTo("Name:           | Column 2"));
        }

        [Test]
        public void Formatter_NotifyFormattingError()
        {
            var obj = new { Name = "some name" };
            var badPlaceholder = new List<string>();

            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.Ignore;
            formatter.OnFormattingFailure += (o, args) => badPlaceholder.Add(args.Placeholder);
            var res = formatter.Format("{NoName} {Name} {OtherMissing}", obj);
            Assert.That(badPlaceholder.Count == 2 && badPlaceholder[0] == "{NoName}" && badPlaceholder[1] == "{OtherMissing}");
        }

        [Test]
        public void LeadingBackslashMustNotEscapeBraces()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.ConvertCharacterStringLiterals = false;

            var expected = "\\Hello";
            var actual = smart.Format("\\{Test}", new { Test = "Hello" });
            Assert.AreEqual(expected, actual);

            smart.Settings.ConvertCharacterStringLiterals = true;

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
            formatter.Settings.FormatErrorAction = ErrorAction.OutputErrorInResult;
            formatter.Settings.ParseErrorAction = ErrorAction.OutputErrorInResult;
            formatter.Parser.AddAlphanumericSelectors(); // required for this test
            var formatParsed = formatter.Parser.ParseFormat(format, new []{string.Empty});
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
        public void Format_WithCache()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString, formatter.FormatterExtensions[0].Names);
            var cache = new FormatCache(format);
            Assert.That(formatter.FormatWithCache(ref cache, formatString, data), Is.EqualTo($"{data.Name}, {data.City}"));
        }

        [Test]
        public void Format_WithCache_Into()
        {
            var data = new {Name = "Joe", City = "Melbourne"};
            var formatter = GetSimpleFormatter();
            var formatString = "{Name}, {City}";
            var format = formatter.Parser.ParseFormat(formatString, formatter.FormatterExtensions[0].Names);
            var cache = new FormatCache(format);
            var output = new StringOutput();
            formatter.FormatWithCacheInto(ref cache, output, formatString, data);
            Assert.That(output.ToString(), Is.EqualTo($"{data.Name}, {data.City}"));
        }

        [Test]
        public void Formatter_GetSourceExtension()
        {
            var formatter = GetSimpleFormatter();
            Assert.That(formatter.GetSourceExtension<DefaultSource>(), Is.InstanceOf(typeof(DefaultSource)));  ;
        }
    }
}