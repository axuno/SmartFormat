using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class FormatterExtensionsTests
{
    private static List<T> GetExtensions<T>() where T: IFormatter
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
    public void Formatters_Can_Be_Initialized()
    {
        foreach (var formatter in GetExtensions<IFormatter>())
        {
            var guid = Guid.NewGuid().ToString("N");
            var negatedAutoDetection = formatter.CanAutoDetect;
            formatter.Name = guid;
            formatter.GetType().GetProperty("Names")?.SetValue(formatter, new[] {guid}); // "Names" property is obsolete
            Assert.Multiple(() =>
            {
                Assert.That(formatter.GetType().GetProperty("Names")?.GetValue(formatter), Is.EqualTo(new[] { guid }));  // "Names" property is obsolete
                Assert.That(formatter.Name, Is.EqualTo(guid));
            });

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
        foreach (var formatter in GetExtensions<IFormatter>().Where(f => f.CanAutoDetect))
        {
            var fi = FormattingInfoExtensions.Create("", new List<object?> {new()});
            Assert.That(() => formatter.TryEvaluateFormat(fi),
                Throws.Nothing);
        }
    }

    [Test]
    public void Add_Known_FormatterExtensions_In_Recommended_Order()
    {
        var sf = new SmartFormatter();

        // Formatters are in arbitrary order
        var allFormatters = GetExtensions<IFormatter>();
        // This should add formatters to the list in the recommended order
        sf.AddExtensions(allFormatters.ToArray());

        var orderedFormatters = allFormatters.OrderBy(f => WellKnownExtensionTypes.Formatters[f.GetType().FullName!])
            .ToList().AsReadOnly();

        Assert.That(sf.GetFormatterExtensions(), Is.EqualTo(orderedFormatters).AsCollection);
    }

    #region: Default Extensions :

    [TestCase("{0:cond:zero|one|two}", 0, "zero")]
    [TestCase("{0:cond:zero|one|two}", 1, "one")]
    [TestCase("{0:cond:zero|one|two}", 2, "two")]
    [TestCase("{0:list:+{}|, |, and }", new[] { 1, 2, 3 }, "+1, +2, and +3")]
    [TestCase("{0:list:+{}|, |, and }", new[] { 2, 3, 4 }, "+2, +3, and +4")]
    [TestCase("{0:d()}", 5, "5")]
    [TestCase("{0:d:N2}", 5, "5.00")]
    public void Invoke_extensions_by_name(string format, object arg0, string expectedResult)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var actualResult = smart.Format(new CultureInfo("en-US"), format, arg0); // must be culture with decimal point
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    #endregion

    [Test]
    [TestCase(true, "yes (probably)")]
    [TestCase(false, "no (possibly)")]
    public void Conditional_Formatter_With_Parenthesis(bool value, string expected)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        Assert.Multiple(() =>
        {
            // explicit conditional formatter
            Assert.That(smart.Format("{value:cond:yes (probably)|no (possibly)}", new { value }), Is.EqualTo(expected));
            // implicit
            Assert.That(smart.Format("{value:yes (probably)|no (possibly)}", new { value }), Is.EqualTo(expected));
        });
    }

    #region: Custom Extensions :

    [Test]
    [TestCase("{0}", 5, "TestExtension1 Options: , Format: ")]
    [TestCase("{0:N2}", 5, "TestExtension1 Options: , Format: N2")]
    public void Without_NamedFormatter_extensions_are_invoked_in_order(string format, object arg0, string expectedResult)
    {
        var smart = GetFormatterWithTestExtensions();
        var actualResult = smart.Format(format, arg0);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
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
        Assert.That(actualResult, Is.EqualTo(expectedResult));
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
        Assert.That(actual, Is.EqualTo(expectedOutput));
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
