using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class TemplateFormatterTests
{
    private static SmartFormatter GetFormatterWithRegisteredTemplates(CaseSensitivityType caseSensitivity)
    {
        var templates = new TemplateFormatter {CanAutoDetect = false};
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {CaseSensitivity = caseSensitivity});
        smart.AddExtensions(templates);

        templates.Register("firstLast", "{First} {Last}");
        templates.Register("lastFirst", "{Last}, {First}");
        templates.Register("FIRST", "{First.ToUpper}");
        templates.Register("last", "{Last.ToLower}");

        if (smart.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive)
        {
            templates.Register("LAST", "{Last.ToUpper}");
        }

        templates.Register("NESTED", "{:t:FIRST} {:t:last}");

        return smart;
    }

    [TestCase("{First} {Last}", "Scott Rippey")]
    public void Sanity_test(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);;
            
        var person = new
        {
            First = "Scott",
            Last = "Rippey",
        };

        var actual = smart.Format(format, person);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Undefined_Template_Should_Throw()
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);;
        Assert.That(() => smart.Format("{:t:does-not-exist}", new object()), Throws.Exception.TypeOf<FormattingException>());
    }

    [Test]
    public void Remove_Template()
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);;
        var tf = smart.GetFormatterExtension<TemplateFormatter>()!;
        Assert.That(tf.Remove("firstLast"), Is.True);
        Assert.That(() => tf.Clear(), Throws.Nothing);
    }

    [Test]
    [TestCase("{:t(firstLast)}", "Scott Rippey")]
    [TestCase("{:t:firstLast}", "Scott Rippey")]
    [TestCase("{:t():firstLast}", "Scott Rippey")]
    [TestCase("{:t(firstLast)}", "Scott Rippey")]
    [TestCase("{:t:firstLast}", "Scott Rippey")]
    [TestCase("{:t(firstLast):IGNORED}", "Scott Rippey")]
    [TestCase("{:t:firstLast}", "Scott Rippey")]
    public void Template_can_be_called_with_options_or_with_formatString(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);
            
        var person = new
        {
            First = "Scott",
            Last = "Rippey",
        };

        var actual = smart.Format(format, person);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    [TestCase("{:t:lastFirst}", "Rippey, Scott")]
    [TestCase("{:t:FIRST}", "SCOTT")]
    [TestCase("{:t:last}", "rippey")]
    [TestCase("{:t:LAST}", "RIPPEY")]
    public void Simple_templates_work_as_expected(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseSensitive);;
            
        var person = new
        {
            First = "Scott",
            Last = "Rippey",
        };

        var actual = smart.Format(format, person);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    [TestCase("{:t:FIRST} {:t:last}", "SCOTT rippey")]
    [TestCase("{:t:firstLast} | {:t:lastFirst}", "Scott Rippey | Rippey, Scott")]
    public void Multiple_templates_can_be_used(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);;
            
        var person = new
        {
            First = "Scott",
            Last = "Rippey",
        };

        var actual = smart.Format(format, person);
        Assert.AreEqual(expected, actual);
    }

    [TestCase(true, "Dear Mr Doe:")]
    [TestCase(false, "Hi Joe:")]
    public void Templates_can_be_nested(bool useFormalSalutation, string expected)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var templates = new TemplateFormatter();
        smart.AddExtensions(templates);

        // ConditionalFormatter with arg1 
        // controls the kind of salutation
        templates.Register("salutation", "{1:cond:{:t:sal_formal}|{:t:sal_informal}}");
        // Register the formal nested template
        templates.Register("sal_formal", "Dear Mr {LastName}");
        // Register the informal nested template
        templates.Register("sal_informal", "Hi {Nickname}");

        var person = new 
        { 
            FirstName = "Joseph", 
            Nickname = "Joe", 
            LastName = "Doe" 
        };

        // Use the "master" template with different args
        var result = smart.Format("{0:t(salutation)}:", person, useFormalSalutation);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("{0:list:{First.ToUpper} {Last}|, }", "JIM Halpert, PAM Beasley, DWIGHT Schrute")]
    [TestCase("{:{:t:FIRST} {:t:last}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
    [TestCase("{:{:t:NESTED}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
    public void Templates_can_be_reused(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);

        var people = new[] { 
            new { First = "Jim", Last = "Halpert" },
            new { First = "Pam", Last = "Beasley" },
            new { First = "Dwight", Last = "Schrute" },
        };
            
        var actual = smart.Format(format, (object) people);
        Assert.AreEqual(expected, actual);
    }
        
    [Test]
    [TestCase("{:template:AAAA}")]
    [TestCase("{:template:9999}")]
    [TestCase("{:template:FIRST_}")]
    [TestCase("{:template:_last}")]
    [TestCase("{:template:}")]
    [TestCase("{:template()}")]
    [TestCase("{:template(AAAA):}")]
    [TestCase("{:template():AAAA}")]
    public void Templates_must_be_defined(string format)
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
        Assert.Throws<FormattingException>(() => smart.Format(format, 5));
    }

    [Test]
    [TestCase("{:t:first}")]
    [TestCase("{:t:firstlast}")]
    [TestCase("{:t:LaSt}")]
    public void Templates_are_case_sensitive(string format)
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
        Assert.Throws<FormattingException>(() => smart.Format(format, 5));
    }

    [Test]
    [TestCase("{:t:first}", "SCOTT")]
    [TestCase("{:t:FIRST}", "SCOTT")]
    [TestCase("{:t:last}", "rippey")]
    [TestCase("{:t:LAST}", "rippey")]
    [TestCase("{:t:nested}", "SCOTT rippey")] 
    [TestCase("{:t:NESTED}", "SCOTT rippey")]
    [TestCase("{:t:NeStEd}", "SCOTT rippey")]
    [TestCase("{:t:fIrStLaSt}", "Scott Rippey")]
    public void Templates_can_be_case_insensitive_and_overwrite_each_other(string format, string expected)
    {
        var smart = GetFormatterWithRegisteredTemplates(CaseSensitivityType.CaseInsensitive);;
            
        var person = new
        {
            First = "Scott",
            Last = "Rippey",
        };

        var actual = smart.Format(format, person);
        Assert.AreEqual(expected, actual);
    }
}