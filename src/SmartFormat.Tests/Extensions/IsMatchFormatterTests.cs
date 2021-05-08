using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
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
            _formatter.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
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
        public void Less_Than_2_Format_Options_Should_Throw()
        {
            // less than 2 format options should throw exception
            Assert.Throws<FormattingException>(() =>
                _formatter.Format("{theKey:ismatch(^.+123.+$):Dummy content}", _variable));
            Assert.DoesNotThrow(() =>
                _formatter.Format("{theKey:ismatch(^.+123.+$):Dummy content|2nd option}", _variable));
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

        [TestCase("€ Euro", true)]
        [TestCase("¥ Yen", true)]
        [TestCase("none", false)]
        public void Currency_Symbol(string currency, bool isMatch)
        {
            var variable = new { Currency = currency};

            // If special characters like \{}: are escaped, they can be used in format options:
            var result = _formatter.Format("{Currency:ismatch(\\p\\{Sc\\}):Currency: {}|Unknown}", variable);
            if(isMatch) Assert.IsTrue(result.Contains("Currency"), "Result contains Currency");
            if(!isMatch) Assert.IsTrue(result.Contains("Unknown"), "Result contains Unknown");
        }
        
        // Single-escaped: only for RegEx
        [TestCase("|", @"\|", @"\|")]
        [TestCase("?", @"\?", @"\?")]
        [TestCase("+", @"\+", @"\+")]
        [TestCase("*", @"\*", @"\*")]
        [TestCase("^", @"\^", @"\^")]
        [TestCase(".", @"\.", @"\.")]
        [TestCase("[", @"\[", @"\[")]
        [TestCase("]", @"\]", @"\]")]
        // Single-escaped: only for Smart.Format
        [TestCase(":", @":", @"\:")]
        // Double-escaped: once for RegEx, one for Smart.Format
        [TestCase(@"\", @"\\", @"\\\\")]
        [TestCase("(", @"\(", @"\\\(")]
        [TestCase(")", @"\)", @"\\\)")]
        [TestCase("{", @"\{", @"\\\{")]
        [TestCase("}", @"\}", @"\\\}")]
        public void Escaped_Option_And_RegEx_Chars(string search, string regExEscaped, string optionsEscaped)
        {
            // To be escaped with backslash for PCRE RegEx:  ".^$*+?()[]{}\|"

            var regEx = new Regex(regExEscaped);
            Assert.IsTrue(regEx.Match(search).Success);
            var result = _formatter.Format("{0:ismatch(" + optionsEscaped + "):found {}|}", search);
            Assert.That(result, Is.EqualTo("found " + search));
        }

        [Test]
        public void Match_Text_Inside_Parenthesis()
        {
            var pattern = @"\(([^\)]*)\)";
            var search = "This text has some (text inside parenthesis)";
            var regEx = new Regex(pattern);
            
            // Escaped pattern: @"\\\(\([^\\\)]*\)\\\)";
            var optionsEscaped = new string(EscapedLiteral.EscapeCharLiterals('\\', pattern, true).ToArray());
            var optionsUnEscaped = EscapedLiteral.UnEscapeCharLiterals('\\', optionsEscaped.AsSpan(), true).ToString();
            var result = _formatter.Format("{0:ismatch(" + optionsEscaped + "):found {}|}", search);
            
            Assert.IsTrue(regEx.Match(search).Success);
            Assert.That(optionsUnEscaped, Is.EqualTo(pattern));
            Assert.That(result, Is.EqualTo("found " + search));
            Assert.That(result, Is.Not.EqualTo("found " + search.Replace("(", string.Empty).Replace(")", string.Empty)));
        }
    }
}
