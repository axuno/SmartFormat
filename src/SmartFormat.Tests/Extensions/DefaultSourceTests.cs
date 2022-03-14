using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class DefaultSourceTests
    {
        private static SmartFormatter GetFormatter()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new DefaultSource());
            smart.AddExtensions(new DefaultFormatter());
            return smart;
        }

        [Test]
        public void Call_With_NonNumeric_Placeholder_Should_Fail()
        {
            var smart = GetFormatter();
            Assert.That(code: () => smart.Format("{a}", 0),
                Throws.TypeOf<FormattingException>().And.Message.Contains("No source extension"));
        }

        [Test]
        public void Call_With_Numeric_Placeholder_Should_Succeed()
        {
            var smart = GetFormatter();
            var result = string.Empty;
            Assert.That(code:() => { result = smart.Format("{0}", 999); }, Throws.Nothing);
            Assert.That(result, Is.EqualTo("999"));
        }
    }
}
