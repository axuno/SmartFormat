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
    public class StringSourceTests
    {
        private SmartFormatter GetSimpleFormatter(SmartSettings? settings = null)
        {
            var smart = new SmartFormatter(settings ?? new SmartSettings());
            smart.AddExtensions(new StringSource(), new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());
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

        [TestCase("{0.CAPITALIZE}", "abc", "Abc")]
        [TestCase("{0.CAPitalizeWORDS}", "abc abc", "Abc Abc")]
        public void SmartFormat_Parameterless_String_Method_CaseInsensitive(string format, string arg, string expected)
        {
            var smart = GetSimpleFormatter(new SmartSettings{CaseSensitivity = CaseSensitivityType.CaseInsensitive});
            Assert.That(smart.Format(format, arg), Is.EqualTo(expected));
        }

        [TestCase("NotExistingAtAll")]
        [TestCase("TRim")] // wrong case of second character
        [TestCase("CApitalize")] // wrong case of second character
        public void TryEvaluate_Should_Fail_For_Unknown_Selector_Name(string selector)
        {
            var smart = GetSimpleFormatter();
            var format = $"{{0.{selector}}}";
            Assert.That(() => smart.Format(format, "dummy"), Throws.Exception.TypeOf(typeof(FormattingException)).And.Message.Contains($"selector \"{selector}\""));
        }

        [Test]
        public void TryEvaluate_Should_Fail_For_Bad_Base64_String()
        {
            var smart = GetSimpleFormatter();
            var format = "{0.FromBase64}";
            Assert.That(() => smart.Format(format, "dummy"), Throws.Exception.TypeOf(typeof(FormattingException)));
        }


        [TestCase("xyz", "x, y, and z")]
        [TestCase("", "")]
        public void String_ToCharArray_Should_Be_Formattable_As_List(string arg, string expected)
        {
            var smart = GetSimpleFormatter();
            var listSourceAndFormatter = new ListFormatter();
            smart.AddExtensions((ISource) listSourceAndFormatter);
            smart.AddExtensions((IFormatter) listSourceAndFormatter);
           
            Assert.That(smart.Format("{0.ToCharArray:list:{}|, |, and }", arg), Is.EqualTo(expected));
        }
    }
}
