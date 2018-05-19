using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class IsMatchFormatterTests
    {
        private Dictionary<string, object> _variable = new Dictionary<string,object>() { {"theKey", "Some123Content"}};
        private SmartFormatter _formatter;

        public IsMatchFormatterTests()
        {
            _formatter = Smart.CreateDefaultSmartFormat();
            _formatter.FormatterExtensions.Add(new IsMatchFormatter {RegexOptions = RegexOptions.CultureInvariant});
            _formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;
        }
        
        [TestCase("{theKey:ismatch(^.+123.+$):Okay - {}|No match content}", RegexOptions.None, "Okay - Some123Content")]
        [TestCase("{theKey:ismatch(^.+123.+$):Fixed content if match|No match content}", RegexOptions.None, "Fixed content if match")]
        [TestCase("{theKey:ismatch(^.+999.+$):{}|No match content}", RegexOptions.None, "No match content")]
        [TestCase("{theKey:ismatch(^.+123.+$):|Only content with no match}", RegexOptions.None, "")]
        [TestCase("{theKey:ismatch(^.+999.+$):|Only content with no match}", RegexOptions.None, "Only content with no match")]
        [TestCase("{theKey:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.IgnoreCase, "Okay - Some123Content")]
        [TestCase("{theKey:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.None, "No match content")]
        public void Test_Formats_And_CaseSensitivity(string format, RegexOptions options, string expected)
        {
            ((IsMatchFormatter) _formatter.FormatterExtensions.First(fex =>
                fex.GetType() == typeof(IsMatchFormatter))).RegexOptions = options;

            Assert.AreEqual(expected, _formatter.Format(format, _variable));
        }

        [Test]
        public void Test_FormatException()
        {
            // less than 2 format options throw exception
            Assert.Throws<FormattingException>(() =>
                _formatter.Format("{theKey:ismatch(^.+123.+$):Dummy content}", _variable));
        }

        [Test]
        public void Test_List()
        {
            var myList = new List<int> {100, 200, 300};
            Assert.AreEqual("100.00, 200.00 and 'no match'",
                _formatter.Format(CultureInfo.InvariantCulture,
                    "{0:list:{:ismatch(^100|200|999$):{:0.00}|'no match'}|, | and }", myList));

            Assert.AreEqual("'match', 'match' and 'no match'",
                _formatter.Format(CultureInfo.InvariantCulture,
                    "{0:list:{:ismatch(^100|200|999$):'match'|'no match'}|, | and }", myList));
        }
    }
}
