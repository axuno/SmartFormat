using System.Globalization;
using NUnit.Framework;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Utilities
{
    [TestFixture]
    public class FormatDelegateTests
    {
        // This example method behaves similar to MVC's Html.ActionLink method:
        private string HtmlActionLink(string linkText, string actionName)
        {
            return string.Format("<a href='www.example.com/{1}'>{0}</a>", linkText, actionName);
        }

        private string GetAnswer(string theText, decimal theValue, CultureInfo culture)
        {
            return $"{theText}{theValue.ToString(culture)}";
        }

        [Test]
        public void FormatDelegate_Works_WithStringFormat()
        {
            var formatDelegate = new FormatDelegate(text => HtmlActionLink(text, "SomePage"));

            Assert.That(string.Format("Please visit {0:this page} for more info.", formatDelegate)
                         , Is.EqualTo("Please visit <a href='www.example.com/SomePage'>this page</a> for more info."));

            Assert.That(string.Format("And {0:this other page} is cool too.", formatDelegate)
                         , Is.EqualTo("And <a href='www.example.com/SomePage'>this other page</a> is cool too."));

            Assert.That(string.Format("There are {0:two} {0:links} in this one.", formatDelegate)
                         , Is.EqualTo("There are <a href='www.example.com/SomePage'>two</a> <a href='www.example.com/SomePage'>links</a> in this one."));
        }

        [Test]
        public void FormatDelegate_Works_WithSmartFormat()
        {
            var formatDelegate = new FormatDelegate((text) => HtmlActionLink(text, "SomePage"));

            Assert.That(Smart.Format("Please visit {0:this page} for more info.", formatDelegate)
                        , Is.EqualTo("Please visit <a href='www.example.com/SomePage'>this page</a> for more info."));

            Assert.That(Smart.Format("And {0:this other page} is cool too.", formatDelegate)
                        , Is.EqualTo("And <a href='www.example.com/SomePage'>this other page</a> is cool too."));

            Assert.That(Smart.Format("There are {0:two} {0:links} in this one.", formatDelegate)
                        , Is.EqualTo("There are <a href='www.example.com/SomePage'>two</a> <a href='www.example.com/SomePage'>links</a> in this one."));
        }

        [Test]
        public void FormatDelegate_WithCulture_WithSmartFormat()
        {
            var amount = (decimal) 123.456;
            var c = new CultureInfo("fr-FR");
            var formatDelegate = new FormatDelegate((text, culture) => GetAnswer("The amount is: ", amount, c));
            Assert.That(Smart.Format("{0}", formatDelegate)
                , Is.EqualTo($"The amount is: {amount.ToString(c)}"));
        }
    }
}
