using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Utilities
{
    [TestFixture]
    public class CustomFormatProviderTests
    {
        private static SmartFormatter GetSimpleFormatter(SmartSettings? settings = null)
        {
            var formatter = new SmartFormatter(settings ?? new SmartSettings())
                .AddExtensions(new DefaultFormatter())
                .AddExtensions(new DefaultSource());
            return formatter;
        }

        #region *** Format with custom formatter ***

        [TestCase("format", "value", true)]
        [TestCase("tamrof", "eulav", true)]
        [TestCase("format", "value", false)]
        [TestCase("tamrof", "eulav", false)]
        public void Format_With_CustomFormatter(string format, string value, bool stringFormatCompatible)
        {
            var smart = GetSimpleFormatter(new SmartSettings {StringFormatCompatibility = stringFormatCompatible});
            var expected = new string(format.Reverse().Select(c => c).ToArray()) + ": " +
                           new string(value.Reverse().Select(c => c).ToArray());
            var resultSmartFormat = smart.Format(new ReverseFormatProvider(), $"{{0:{format}}}", value);
            var resultStringFormat = string.Format(new ReverseFormatProvider(), $"{{0:{format}}}", value);
            Assert.That(resultSmartFormat, Is.EqualTo(expected));
            Assert.That(resultStringFormat, Is.EqualTo(expected));
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
    }
}
