using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class SourceExtensionsTests
    {
        private static List<T> GetExtensions<T>() where T: ISource
        {
            var formatterExtensions =
                WellKnownExtensionTypes.GetReferencedExtensions<T>();
            
            // Create instances of all T types
            var toReturn = formatterExtensions
                .Select(WellKnownExtensionTypes.CreateInstanceForType<T>)
                .ToList();
            
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
        public void Add_Known_SourceExtensions_In_Recommended_Order()
        {
            var sf = new SmartFormatter();

            // Sources are in arbitrary order
            var allSources = GetExtensions<ISource>();
            // This should add sources to the list in the recommended order
            sf.AddExtensions(allSources.ToArray());

            var orderedSources = allSources.OrderBy(f => WellKnownExtensionTypes.Sources[f.GetType().FullName!])
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
            var testFormatter = new SmartFormatter(new SmartSettings
                    { Formatter = new FormatterSettings { ErrorAction = FormatErrorAction.ThrowError } })
                .AddExtensions(new TestExtension1(), new TestExtension2()).InsertExtension(2, new DefaultFormatter())
                .AddExtensions(new DefaultSource());
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
