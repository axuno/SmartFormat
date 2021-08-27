using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class FormatterExtensionsTests
    {
        private static List<IFormatter> GetAllFormatters()
        {
            var allFormatterTypes = Smart.Default.GetType().Assembly.GetTypes()
                .Where(x => typeof(IFormatter).IsAssignableFrom(x) && !x.IsInterface &&
                            !x.IsAbstract & x.Namespace == $"{nameof(SmartFormat)}.{nameof(SmartFormat.Extensions)}").ToList();

            var allFormatters = new List<IFormatter>();
            foreach (var ft in allFormatterTypes)
            {
                if(ft.Name == nameof(PluralLocalizationFormatter) || ft.Name == nameof(TimeFormatter))
                    allFormatters.Add((IFormatter) Activator.CreateInstance(Type.GetType(ft.AssemblyQualifiedName!)!, "en")!);
                else
                    allFormatters.Add((IFormatter) Activator.CreateInstance(Type.GetType(ft.AssemblyQualifiedName!)!)!);
            }
            
            return allFormatters;
        }

        [Test]
        public void Formatters_Can_Be_Initialized()
        {
            foreach (var formatter in GetAllFormatters())
            {
                var guid = Guid.NewGuid().ToString("N");
                var negatedAutoDetection = formatter.CanAutoDetect;
                formatter.Name = guid;
                formatter.GetType().GetProperty("Names")?.SetValue(formatter, new[] {guid}); // "Names" property is obsolete
                
                Assert.That(formatter.GetType().GetProperty("Names")?.GetValue(formatter), Is.EqualTo(new[]{guid}));  // "Names" property is obsolete
                Assert.That(formatter.Name, Is.EqualTo(guid));
                
                if (formatter is not TemplateFormatter)
                {
                    formatter.CanAutoDetect = negatedAutoDetection;
                    Assert.That(formatter.CanAutoDetect, Is.EqualTo(negatedAutoDetection));
                }

                if (formatter is IInitializer initializer)
                {
                    Assert.That(() => initializer.Initialize(new SmartFormatter()), Throws.Nothing);
                }
            }
        }

        [Test]
        public void Formatters_AutoDetection_Should_Not_Throw()
        {
            foreach (var formatter in GetAllFormatters().Where(f => f.CanAutoDetect))
            {
                Assert.That(() => formatter.TryEvaluateFormat(FormattingInfo.Create("", new List<object> {new()})),
                    Throws.Nothing);
            }
        }

        #region: Default Extensions :

        [Test]
        [TestCase("{0:cond:zero|one|two}", 0, "zero")]
        [TestCase("{0:cond:zero|one|two}", 1, "one")]
        [TestCase("{0:cond:zero|one|two}", 2, "two")]
        [TestCase("{0:plural:one|many}", 1, "one")]
        [TestCase("{0:plural:one|many}", 2, "many")]

        [TestCase("{0:list:+{}|, |, and }", new []{ 1, 2, 3 }, "+1, +2, and +3")]
        [TestCase("{0:list:+{}|, |, and }", new []{ 1, 2, 3 }, "+1, +2, and +3")]
        
        [TestCase("{0:d()}", 5, "5")]
        [TestCase("{0:d:N2}", 5, "5.00")]

        public void Invoke_extensions_by_name_or_shortname(string format, object arg0, string expectedResult)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            var actualResult = smart.Format(new CultureInfo("en-US"), format, arg0); // must be culture with decimal point
            Assert.AreEqual(expectedResult, actualResult);
        }

        #endregion

        [Test]
        [TestCase(true, "yes (probably)")]
        [TestCase(false, "no (possibly)")]
        public void Conditional_Formatter_With_Parenthesis(bool value, string expected)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            // explicit conditional formatter
            Assert.AreEqual(expected, smart.Format("{value:cond:yes (probably)|no (possibly)}", new { value }));
            // implicit
            Assert.AreEqual(expected, smart.Format("{value:yes (probably)|no (possibly)}", new { value }));
        }

        #region: Custom Extensions :

        [Test]
        [TestCase("{0}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0:N2}", 5, "TestExtension1 Options: , Format: N2")]
        public void Without_NamedFormatter_extensions_are_invoked_in_order(string format, object arg0, string expectedResult)
        {
            var smart = GetCustomFormatter();
            var actualResult = smart.Format(format, arg0);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase("{0:test1:}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0:test1():}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0:test1:N2}", 5, "TestExtension1 Options: , Format: N2")]
        [TestCase("{0:test1():N2}", 5, "TestExtension1 Options: , Format: N2")]
        [TestCase("{0:test1(a,b,c):}", 5, "TestExtension1 Options: a,b,c, Format: ")]
        [TestCase("{0:test1(a,b,c):N2}", 5, "TestExtension1 Options: a,b,c, Format: N2")]

        [TestCase("{0:test2:}", 5, "TestExtension2 Options: , Format: ")]
        [TestCase("{0:test2():}", 5, "TestExtension2 Options: , Format: ")]
        [TestCase("{0:test2:N2}", 5, "TestExtension2 Options: , Format: N2")]
        [TestCase("{0:test2():N2}", 5, "TestExtension2 Options: , Format: N2")]
        [TestCase("{0:test2(a,b,c):}", 5, "TestExtension2 Options: a,b,c, Format: ")]
        [TestCase("{0:test2(a,b,c):N2}", 5, "TestExtension2 Options: a,b,c, Format: N2")]

        [TestCase("{0:d:}", 5, "5")]
        [TestCase("{0:d():}", 5, "5")]
        [TestCase("{0:d:N2}", 5, "5.00")]
        [TestCase("{0:d():N2}", 5, "5.00")]
        public void NamedFormatter_invokes_a_specific_formatter(string format, object arg0, string expectedResult)
        {
            var smart = GetCustomFormatter();
            var actualResult = smart.Format(new CultureInfo("en-US"), format, arg0); // must be culture with decimal point
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase("{0:test1:}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0}", 5, "TestExtension2 Options: , Format: ")]
        [TestCase("{0:N2}", 5, "TestExtension2 Options: , Format: N2")]
        public void Implicit_formatters_require_an_isDefaultFormatter_flag(string format, object arg0, string expectedOutput)
        {
            var formatter = GetCustomFormatter();
            formatter.GetFormatterExtension<TestExtension1>()!.CanAutoDetect = false;
            formatter.GetFormatterExtension<TestExtension2>()!.CanAutoDetect = true;
            var actual = formatter.Format(format, arg0);
            Assert.AreEqual(expectedOutput, actual);
        }

        private static SmartFormatter GetCustomFormatter()
        {
            var testFormatter = new SmartFormatter(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            testFormatter.AddExtensions(new TestExtension1(), new TestExtension2(), new DefaultFormatter());
            testFormatter.AddExtensions(new DefaultSource());
            return testFormatter;
        }

        private class TestExtension1 : IFormatter
        {
            ///<inheritdoc/>
            public string Name { get; set; } = "test1";

            ///<inheritdoc/>
            public bool CanAutoDetect { get; set; } = true;

            public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
            {
                var options = formattingInfo.FormatterOptions;
                var format = formattingInfo.Format;
                var formatString = format != null ? format.RawText : "";
                formattingInfo.Write("TestExtension1 Options: " + options + ", Format: " + formatString);
                return true;
            }
        }
        private class TestExtension2 : IFormatter
        {
            ///<inheritdoc/>
            public string Name { get; set; } = "test2";

            ///<inheritdoc/>
            public bool CanAutoDetect { get; set; } = true;

            public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
            {
                var options = formattingInfo.FormatterOptions;
                var format = formattingInfo.Format;
                var formatString = format != null ? format.ToString() : "";
                formattingInfo.Write("TestExtension2 Options: " + options + ", Format: " + formatString);
                return true;
            }
        }

        #endregion

    }

}