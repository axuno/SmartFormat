using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
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

            templates.Register("NESTED", "{:template:FIRST} {:template:last}");

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
            Assert.That(() => smart.Format("{:template:does-not-exist}", new object()), Throws.Exception.TypeOf<FormattingException>());
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
        [TestCase("{:template(firstLast)}", "Scott Rippey")]
        [TestCase("{:template:firstLast}", "Scott Rippey")]
        [TestCase("{:template():firstLast}", "Scott Rippey")]
        [TestCase("{:template(firstLast)}", "Scott Rippey")]
        [TestCase("{:template:firstLast}", "Scott Rippey")]
        [TestCase("{:template(firstLast):IGNORED}", "Scott Rippey")]
        [TestCase("{:template:firstLast}", "Scott Rippey")]
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
        [TestCase("{:template:lastFirst}", "Rippey, Scott")]
        [TestCase("{:template:FIRST}", "SCOTT")]
        [TestCase("{:template:last}", "rippey")]
        [TestCase("{:template:LAST}", "RIPPEY")]
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
        [TestCase("{:template:FIRST} {:template:last}", "SCOTT rippey")]
        [TestCase("{:template:firstLast} | {:template:lastFirst}", "Scott Rippey | Rippey, Scott")]
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

        [Test]
        [TestCase("{:template:NESTED}", "SCOTT rippey")]
        public void Templates_can_be_nested(string format, string expected)
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

        [TestCase("{0:list:{First.ToUpper} {Last}|, }", "JIM Halpert, PAM Beasley, DWIGHT Schrute")]
        [TestCase("{:{:template:FIRST} {:template:last}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
        [TestCase("{:{:template:NESTED}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
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
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            Assert.Throws<FormattingException>(() => smart.Format(format, 5));
        }

        [Test]
        [TestCase("{:template:first}")]
        [TestCase("{:template:firstlast}")]
        [TestCase("{:template:LaSt}")]
        public void Templates_are_case_sensitive(string format)
        {
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            Assert.Throws<FormattingException>(() => smart.Format(format, 5));
        }

        [Test]
        [TestCase("{:template:first}", "SCOTT")]
        [TestCase("{:template:FIRST}", "SCOTT")]
        [TestCase("{:template:last}", "rippey")]
        [TestCase("{:template:LAST}", "rippey")]
        [TestCase("{:template:nested}", "SCOTT rippey")] 
        [TestCase("{:template:NESTED}", "SCOTT rippey")]
        [TestCase("{:template:NeStEd}", "SCOTT rippey")]
        [TestCase("{:template:fIrStLaSt}", "Scott Rippey")]
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
}