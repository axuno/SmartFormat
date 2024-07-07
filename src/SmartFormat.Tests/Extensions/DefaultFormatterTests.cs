using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class DefaultFormatterTests
{
    private static SmartFormatter GetFormatter(SmartSettings? settings = null)
    {
        settings ??= new SmartSettings();
        var smart = new SmartFormatter(settings);
        smart.AddExtensions(new DefaultSource());
        smart.AddExtensions(new DefaultFormatter());
        return smart;
    }

    [Test]
    public void Call_With_Indexed_Placeholder()
    {
        var smart = GetFormatter();
        smart.AddExtensions(new KeyValuePairSource());
        var result = smart.Format("{0}", "a:value");
        Assert.That(result, Is.EqualTo("a:value"));
    }
        
    [Test]
    public void Call_With_Named_Placeholder()
    {
        var smart = GetFormatter();
        smart.AddExtensions(new KeyValuePairSource());
        var result = smart.Format("{a}", new KeyValuePair<string, object?>("a", "a:value"));
        Assert.That(result, Is.EqualTo("a:value"));
    }

    [Test]
    public void Call_With_Nested_Placeholder()
    {
        var smart = GetFormatter();
        smart.AddExtensions(new KeyValuePairSource());
        smart.AddExtensions(new ChooseFormatter());
        var result = smart.Format("{a:{:choose(no-value|a-value):No|Aha, a value}}", new KeyValuePair<string, object?>("a", "a-value"));
        Assert.That(result, Is.EqualTo("Aha, a value"));
    }

    #region * IFormattable Test *

    public class FmtDemo : IFormattable
    {
        public string ToString(string? format, IFormatProvider? p)
        {
            return $"{format} implementing IFormattable";
        }
    }

    [Test]
    public void Call_With_IFormattable_Argument()
    {
        var smart = GetFormatter();
        var result = smart.Format("{0:This variable is}", new FmtDemo());
        Assert.That(result, Is.EqualTo("This variable is implementing IFormattable"));
    }

    #endregion

    #region * ISpanFormattable Test *

#if NET6_0_OR_GREATER
    [Test]
    public void ISpanFormattable_Exceeding_Stackalloc_Buffer()
    {
        var smart = GetFormatter();
        var data = new ISpanFormattableTest(DefaultFormatter.StackAllocCharBufferSize + 1);
        var result = smart.Format("{0}", data);
        Assert.That(result, Is.EqualTo(data.ToString()));
    }

    private class ISpanFormattableTest : ISpanFormattable
    {
        char[] _buffer;

        public ISpanFormattableTest(int size)
        {
            _buffer = new char[size];
            _buffer.AsSpan().Fill('b');
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (destination.Length < _buffer.Length)
            {
                charsWritten = 0;
                return false;
            }

            _buffer.AsSpan().CopyTo(destination);
            charsWritten = _buffer.Length;
            return true;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider, format ?? string.Empty, new string(_buffer));
        }

        public override string ToString()
        {
            return new string(_buffer);
        }
    }
#endif

    #endregion

    #region *** Format with custom formatter ***

    [TestCase("format", "value", true)]
    [TestCase("tamrof", "eulav", true)]
    [TestCase("format", "value", false)]
    [TestCase("tamrof", "eulav", false)]
    public void Format_With_CustomFormatter(string format, string value, bool stringFormatCompatible)
    {
        var smart = GetFormatter(new SmartSettings {StringFormatCompatibility = stringFormatCompatible});
        var expected = new string(format.Reverse().Select(c => c).ToArray()) + ": " +
                       new string(value.Reverse().Select(c => c).ToArray());
        var resultSmartFormat = smart.Format(new ReverseFormatProvider(), $"{{0:{format}}}", value);
        var resultStringFormat = string.Format(new ReverseFormatProvider(), $"{{0:{format}}}", value);
        Assert.Multiple(() =>
        {
            Assert.That(resultSmartFormat, Is.EqualTo(expected));
            Assert.That(resultStringFormat, Is.EqualTo(expected));
        });
    }

    /// <summary>
    /// Used for Format_With_CustomFormatter test
    /// </summary>
    public class ReverseFormatProvider : IFormatProvider
    {
        public object GetFormat(Type? formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return new ReverseFormatAndArgumentFormatter();
                
            return new object();
        }
    }

    /// <summary>
    /// Used for Format_With_CustomFormatter test
    /// </summary>
    public class ReverseFormatAndArgumentFormatter : ICustomFormatter
    {
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            return new string(format!.Reverse().Select(c => c).ToArray()) + ": " +
                   new string((arg as string ?? "?").Reverse().Select(c => c).ToArray());
        }
    }
#endregion

    #region *** FormatDelegate Tests **

    // This example method behaves similar to MVC's Html.ActionLink method:
    private static string HtmlActionLink(string linkText, string actionName)
    {
        return string.Format("<a href='www.example.com/{1}'>{0}</a>", linkText, actionName);
    }

    [TestCase("Please visit {0:this page} for more info.", "Please visit <a href='www.example.com/SomePage'>this page</a> for more info.")]
    [TestCase("And {0:this other page} is cool too.", "And <a href='www.example.com/SomePage'>this other page</a> is cool too.")]
    [TestCase("There are {0:two} {0:links} in this one.", "There are <a href='www.example.com/SomePage'>two</a> <a href='www.example.com/SomePage'>links</a> in this one.")]
    public void FormatDelegate_Works_WithStringFormat(string format, string expected)
    {
        var formatDelegate = new FormatDelegate(text => HtmlActionLink(text ?? "null", "SomePage"));

        Assert.That(string.Format(format, formatDelegate), Is.EqualTo(expected));
    }

    [TestCase("Please visit {0:this page} for more info.", "Please visit <a href='www.example.com/SomePage'>this page</a> for more info.")]
    [TestCase("And {0:this other page} is cool too.", "And <a href='www.example.com/SomePage'>this other page</a> is cool too.")]
    [TestCase("There are {0:two} {0:links} in this one.", "There are <a href='www.example.com/SomePage'>two</a> <a href='www.example.com/SomePage'>links</a> in this one.")]
    public void FormatDelegate_Works_WithSmartFormat(string format, string expected)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var formatDelegate = new FormatDelegate((text) => HtmlActionLink(text ?? "null", "SomePage"));

        Assert.That(smart.Format(format, formatDelegate), Is.EqualTo(expected));
    }

    [Test]
    public void FormatDelegate_WithCulture()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var amount = (decimal) 123.456;
        var c = new CultureInfo("fr-FR");
        // Only works for indexed placeholders
        var formatDelegate = new FormatDelegate((text, culture) => $"{text}: {amount.ToString(c)}");
        Assert.That(smart.Format("{0:The amount is}", formatDelegate)
            , Is.EqualTo($"The amount is: {amount.ToString(c)}"));
    }

    #endregion
}
