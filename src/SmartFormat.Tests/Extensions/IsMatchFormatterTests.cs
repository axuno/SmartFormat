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
        private Dictionary<string, object> _variable = new() { {"theKey", "Some123Content"}};
        private SmartFormatter _formatter;

        public IsMatchFormatterTests()
        {
            _formatter = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            _formatter.AddExtensions(new IsMatchFormatter {RegexOptions = RegexOptions.CultureInvariant});
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
            var result = _formatter.Format("{Currency:ismatch(\\\\p\\{Sc\\}):Currency: {}|Unknown}", variable);
            if (isMatch) Assert.IsTrue(result.Contains("Currency"), "Result contains Currency");
            if (!isMatch) Assert.IsTrue(result.Contains("Unknown"), "Result contains Unknown");
        }
        
        // Single-escaped: only for RegEx
        [TestCase("|", @"\|", @"\\|")]
        [TestCase("?", @"\?", @"\\?")]
        [TestCase("+", @"\+", @"\\+")]
        [TestCase("*", @"\*", @"\\*")]
        [TestCase("^", @"\^", @"\\^")]
        [TestCase(".", @"\.", @"\\.")]
        [TestCase("[", @"\[", @"\\[")]
        [TestCase("]", @"\]", @"\\]")]
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

        [TestCase(@"\(([^\)]*)\)", "Text (inside) parenthesis", true)] // escaped parenthesis
        [TestCase(@"\(([^\)]*)\)", "No parenthesis", false)]
        [TestCase(@"Lon(?=don)", "This is London", true)] // parenthesis
        [TestCase(@"Lon(?=don)", "This is Loando", false)]
        [TestCase(@"<[^<>]+>", "<abcde>", true)] // square and pointed brackets
        [TestCase(@"<[^<>]+>", "<>", false)]
        [TestCase(@"\d{3,}", "1234", true)] // curly braces
        [TestCase(@"\d{3,}", "12", false)]
        [TestCase(@"^.{5,}:,$", "1z%aW:,", true)] // dot, colon, comma
        [TestCase(@"^.{5,}:,$", "1z:,", false)]
        public void Match_Special_Characters(string pattern, string input, bool shouldMatch)
        {
            var regExOptions = RegexOptions.None;
            ((IsMatchFormatter) _formatter.FormatterExtensions.First(fex =>
                fex.GetType() == typeof(IsMatchFormatter))).RegexOptions = regExOptions;

            var regEx = new Regex(pattern, regExOptions);
            var optionsEscaped = new string(EscapedLiteral.EscapeCharLiterals('\\', pattern, 0, pattern.Length, true).ToArray());
            var result = _formatter.Format("{0:ismatch(" + optionsEscaped + "):found {}|}", input);

            Assert.That(regEx.Match(input).Success, Is.EqualTo(shouldMatch), "RegEx pattern match");
            Assert.That(result, shouldMatch ? Is.EqualTo("found " + input) : Is.EqualTo(string.Empty), "IsMatchFormatter pattern match");
        }
    }
}
