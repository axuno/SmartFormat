using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class IsMatchFormatterTests
{
    private KeyValuePair<string, object> _variable = new("theValue", "Some123Content");

    private static SmartFormatter GetFormatter()
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
                { Formatter = new FormatterSettings { ErrorAction = FormatErrorAction.ThrowError } })
            .AddExtensions(new IsMatchFormatter());
        var mf = smart.GetFormatterExtension<IsMatchFormatter>()!;
        mf.RegexOptions = RegexOptions.CultureInvariant;
        mf.PlaceholderNameForMatches = "m";

        return smart;
    }
        
    [TestCase("{theValue:ismatch(^.+123.+$):Okay - {}|No match content}", RegexOptions.None, "Okay - Some123Content")]
    [TestCase("{theValue:ismatch(^.+123.+$):Fixed content if match|No match content}", RegexOptions.None, "Fixed content if match")]
    [TestCase("{theValue:ismatch(^.+999.+$):{}|No match content}", RegexOptions.None, "No match content")]
    [TestCase("{theValue:ismatch(^.+123.+$):|Only content with no match}", RegexOptions.None, "")]
    [TestCase("{theValue:ismatch(^.+999.+$):|Only content with no match}", RegexOptions.None, "Only content with no match")]
    [TestCase("{theValue:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.IgnoreCase, "Okay - Some123Content")]
    [TestCase("{theValue:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.None, "No match content")]
    public void Test_Formats_And_CaseSensitivity(string format, RegexOptions options, string expected)
    {
        var smart = GetFormatter();
        smart.GetFormatterExtension<IsMatchFormatter>()!.RegexOptions = options;
        var result = smart.Format(format, _variable);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Less_Than_2_Format_Options_Should_Throw()
    {
        var smart = GetFormatter();
        // less than 2 format options should throw exception
        Assert.Throws<FormattingException>(code: () =>
            smart.Format("{theValue:ismatch(^.+123.+$):Dummy content}", _variable));
    }

    // The "{}" in the format will output the input variable
    [TestCase("{theValue:ismatch(^.+123.+$):|Has match for '{}'|~|No match|}", "|Has match for 'Some123Content'|")]
    [TestCase("{theValue:ismatch(^.+999.+$):|Has match for '{}'|~|No match|}", "|No match|")]
    public void Test_With_Changed_SplitChar(string format, string expected)
    {
        var variable = new Dictionary<string, object> { {"theValue", "Some123Content"}};
        var smart = GetFormatter();;
        // Set SplitChar from | to ~, so we can use | for the output string
        smart.GetFormatterExtension<IsMatchFormatter>()!.SplitChar = '~';
        var result = smart.Format(format, variable);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test, Description("Output with RegEx matching group values")]
    [TestCase("{theValue:ismatch(^.+\\(1\\)\\(2\\)\\(3\\).+$):Matches for '{}'\\: {m:list:| - }|No match}", "Matches for 'Some123Content': Some123Content - 1 - 2 - 3")]
    [TestCase("{theValue:ismatch(^.+\\(9\\)\\(9\\)\\(9\\).+$):Matches for '{}'\\: {m:list:| - }|No match}", "No match")]
    [TestCase("{theValue:ismatch(^.+\\(1\\)\\(2\\)\\(3\\).+$):First 2 matches in '{}'\\: {m[1]} and {m[2]}|No match}", "First 2 matches in 'Some123Content': 1 and 2")]
    public void Match_And_List_Matches(string format, string expected)
    {
        var variable = new KeyValuePair<string, object> ("theValue", "Some123Content");
        var smart = GetFormatter();
        var result = smart.Format(format, variable);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test, Description("The name of the placeholder for RegEx matching group values can be changed")]
    public void Change_Placeholder_Name_For_Matches()
    {
        var variable = new KeyValuePair<string, object> ("theValue", "12345");
        var smart = GetFormatter();
        smart.GetFormatterExtension<IsMatchFormatter>()!.PlaceholderNameForMatches = "match";

        var format = "{theValue:ismatch(^\\(123\\)\\(4\\)\\(5\\)$):First match in '{}'\\: {match[1]}|No match}";
            
        var result = smart.Format(format, variable);
        Assert.That(result, Is.EqualTo("First match in '12345': 123"));
    }

    [Test]
    public void Match_Nested_In_ListFormatter()
    {
        var smart = GetFormatter();
        var myList = new List<int> {100, 200, 300};
        Assert.That(smart.Format(CultureInfo.InvariantCulture,
                "{0:list:{:ismatch(^100|200|999$):{:0.00}|no match for '{}'}|, | and }", myList), Is.EqualTo("100.00, 200.00 and no match for '300'"));
    }

    [TestCase("€ Euro", "Currency: €")]
    [TestCase("¥ Yen", "Currency: ¥")]
    [TestCase("none", "Unknown")]
    public void Currency_Symbol(string currency, string expected)
    {
        var smart = GetFormatter();
        var variable = new { Currency = currency};

        // If special characters like \{}: are escaped, they can be used in format options:
        var regex = "\\p{Sc}";
        var escapedRegex = new string(EscapedLiteral.EscapeCharLiterals('\\', regex, 0, regex.Length, true).ToArray());
        var result = smart.Format("{Currency:ismatch(" + escapedRegex + "):Currency: {m[0]}|Unknown}", variable);
        Assert.That(result, Is.EqualTo(expected));
    }
        
    // Single-escaped: only for RegEx
    [TestCase("|", @"\|", @"\\|")]
    [TestCase("?", @"\?", @"\\?")]
    [TestCase("+", @"\+", @"\\+")]
    [TestCase("*", @"\*", @"\\*")]
    [TestCase("^", @"\^", @"\\^")]
    [TestCase("$", @"\$", @"\\$")]
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
        var smart = GetFormatter();
        // To be escaped with backslash for PCRE RegEx:  ".^$*+?()[]{}\|"
        var regEx = new Regex(regExEscaped);

        Assert.Multiple(() =>
        {
            Assert.That(EscapedLiteral.EscapeCharLiterals('\\', regExEscaped, 0, regExEscaped.Length, true), Is.EquivalentTo(optionsEscaped));
            Assert.That(regEx.Match(search).Success, Is.True);
        });

        var result = smart.Format("{0:ismatch(" + optionsEscaped + "):found {}|}", search);
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
        var smart = GetFormatter();
        var regExOptions = RegexOptions.None;
        ((IsMatchFormatter) smart.FormatterExtensions.First(fex =>
            fex.GetType() == typeof(IsMatchFormatter))).RegexOptions = regExOptions;

        var regEx = new Regex(pattern, regExOptions);
        var optionsEscaped = new string(EscapedLiteral.EscapeCharLiterals('\\', pattern, 0, pattern.Length, true).ToArray());
        var result = smart.Format("{0:ismatch(" + optionsEscaped + "):found {}|}", input);

        Assert.Multiple(() =>
        {
            Assert.That(regEx.Match(input).Success, Is.EqualTo(shouldMatch), "RegEx pattern match");
            Assert.That(result, shouldMatch ? Is.EqualTo("found " + input) : Is.EqualTo(string.Empty), "IsMatchFormatter pattern match");
        });
    }
}
