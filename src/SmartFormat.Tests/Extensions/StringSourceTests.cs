﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class StringSourceTests
    {
        private SmartFormatter GetSimpleFormatter()
        {
            var smart = new SmartFormatter();
            smart.SourceExtensions.AddRange(new ISource[]{new StringSource(smart), new DefaultSource(smart)});
            smart.FormatterExtensions.AddRange(new IFormatter[] {new DefaultFormatter()});
            return smart;
        }

        [TestCase("{0.Length}", "7")]
        [TestCase("{0.ToUpper}", " ABCDE ")]
        [TestCase("{0.ToUpperInvariant}", " ABCDE ")]
        [TestCase("{0.ToLower}", " abcde ")]
        [TestCase("{0.ToLowerInvariant}", " abcde ")]
        [TestCase("{0.ToUpper.Trim}", "ABCDE")]
        [TestCase("{0.ToLower.TrimStart.TrimEnd}", "abcde")]
        [TestCase("{0.TrimStart.TrimEnd.ToLower}", "abcde")]
        public void Parameterless_String_Methods_Should_Be_Equal_DotNet(string format, string expected)
        {
            Assert.That(GetSimpleFormatter().Format(format, " aBcDe "), Is.EqualTo(expected));
        }

        [TestCase("{0.Capitalize}", "", "")]
        [TestCase("{0.Capitalize}", "A", "A")]
        [TestCase("{0.Capitalize}", "a", "A")]
        [TestCase("{0.Capitalize}", "abc", "Abc")]
        [TestCase("{0.Capitalize}", "aBcd", "ABcd")]
        [TestCase("{0.CapitalizeWords}", "", "")]
        [TestCase("{0.CapitalizeWords}", "A A", "A A")]
        [TestCase("{0.CapitalizeWords}", "a a", "A A")]
        [TestCase("{0.CapitalizeWords}", "abc abc", "Abc Abc")]
        [TestCase("{0.CapitalizeWords}", "aBcd aBcd", "ABcd ABcd")]
        [TestCase("{0.ToBase64}", "a", "YQ==")]
        [TestCase("{0.FromBase64}", "YWJj", "abc")]
        public void SmartFormat_Parameterless_String_Methods(string format, string arg, string expected)
        {
            Assert.That(GetSimpleFormatter().Format(format, arg), Is.EqualTo(expected));
        }

        [TestCase("{0.Capitalize}", "abc", "Abc")]
        [TestCase("{0.CapitalizeWords}", "abc abc", "Abc Abc")]
        public void SmartFormat_Parameterless_String_Method_With_Culture(string format, string arg, string expected)
        {
            Assert.That(GetSimpleFormatter().Format(CultureInfo.InvariantCulture, format, arg), Is.EqualTo(expected));
        }
    }
}
