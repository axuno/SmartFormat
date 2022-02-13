using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class KeyValuePairSourceTests
    {
        private SmartFormatter GetFormatter()
        {
            var smart = new SmartFormatter();
            smart.AddExtensions(new KeyValuePairSource());
            smart.AddExtensions(new DefaultFormatter());
            return smart;
        }

        [Test]
        public void Call_With_Null_Should_Fail()
        {
            var smart = GetFormatter();
            Assert.That(code: () => smart.Format("{a}", null, null),
                Throws.TypeOf<FormattingException>().And.Message.Contains("No source extension"));
        }

        [Test]
        public void Call_With_Unknown_Type_Should_Fail()
        {
            var smart = GetFormatter();
            // only KeyValuePair<string, object?> can be used
            Assert.That(code: () => smart.Format("{a}", new KeyValuePair<string, int>("a", 123)),
                Throws.TypeOf<FormattingException>().And.Message.Contains("No source extension"));
        }

        [TestCase("my value", "my value")]
        [TestCase(null, "")]
        public void Call_With_KeyValuePair_Should_Succeed(string? theValue, string expected)
        {
            var smart = GetFormatter();
            var result = string.Empty;
            Assert.That(
                code: () =>
                {
                    result = smart.Format("{placeholder}", new KeyValuePair<string, object?>("placeholder", theValue));
                }, Throws.Nothing);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
