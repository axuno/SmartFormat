using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class SourceExtensionsTests
    {
        private static List<T> GetExtensions<T>()
        {
            var (transientExtensionTypes, singletonExtensionTypes) =
                WellKnownExtensions.GetReferencedExtensions<T>();
            
            // Create instances of all T types
            var toReturn = transientExtensionTypes
                .Select(it => (T)Activator.CreateInstance(Type.GetType(it.AssemblyQualifiedName!)!)!)
                .ToList();
            toReturn.AddRange(singletonExtensionTypes.Select(st => (T) st.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)!.GetValue(st)!));
            return toReturn;
        }

        [Test]
        public void Sources_Can_Be_Initialized()
        {
            foreach (var source in GetExtensions<ISource>())
            {
                if (source is IInitializer initializer)
                {
                    Assert.That(() => initializer.Initialize(new SmartFormatter()), Throws.Nothing);
                }
            }
        }

        [Test]
        public void Add_Known_SourceExtensions()
        {
            var sf = new SmartFormatter();

            // sources are in arbitrary order
            var allSources = GetExtensions<ISource>();
            foreach (var source in allSources)
            {
                var index = WellKnownExtensions.GetIndexToInsert(sf.SourceExtensions, source);
                sf.AddExtensions(index, source);
            }

            var orderedSources = allSources.OrderBy(f => WellKnownExtensions.Sources[f.GetType().FullName])
                .ToList().AsReadOnly();

            CollectionAssert.AreEqual(orderedSources, sf.GetSourceExtensions());
        }

        #region: Custom Extensions :

        [Test]
        [TestCase("{0}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0:N2}", 5, "TestExtension1 Options: , Format: N2")]
        public void Without_NamedFormatter_extensions_are_invoked_in_order(string format, object arg0, string expectedResult)
        {
            var smart = GetFormatterWithTestExtensions();
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
            var smart = GetFormatterWithTestExtensions();
            var actualResult = smart.Format(new CultureInfo("en-US"), format, arg0); // must be culture with decimal point
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase("{0:test1:}", 5, "TestExtension1 Options: , Format: ")]
        [TestCase("{0}", 5, "TestExtension2 Options: , Format: ")]
        [TestCase("{0:N2}", 5, "TestExtension2 Options: , Format: N2")]
        public void Implicit_formatters_require_an_isDefaultFormatter_flag(string format, object arg0, string expectedOutput)
        {
            var formatter = GetFormatterWithTestExtensions();
            formatter.GetFormatterExtension<TestExtension1>()!.CanAutoDetect = false;
            formatter.GetFormatterExtension<TestExtension2>()!.CanAutoDetect = true;
            var actual = formatter.Format(format, arg0);
            Assert.AreEqual(expectedOutput, actual);
        }

        private static SmartFormatter GetFormatterWithTestExtensions()
        {
            var testFormatter = new SmartFormatter(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            testFormatter.AddExtensions(new TestExtension1(), new TestExtension2(), new DefaultFormatter());
            testFormatter.AddExtensions(new DefaultSource());
            return testFormatter;
        }

        private class TestExtension1 : IFormatter
        {
            public string Name { get; set; } = "test1";

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
            public string Name { get; set; } = "test2";

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