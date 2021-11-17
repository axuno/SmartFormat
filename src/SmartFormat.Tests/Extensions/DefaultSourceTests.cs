using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class DefaultSourceTests
    {
        private class SourceImplementation : Source
        { }

        [Test]
        public void Call_With_NonNumeric_Argument_Should_Fail()
        {
            var source = new DefaultSource();
            Assert.That(source.TryEvaluateSelector(FormattingInfoExtensions.Create("{a}", new List<object?>())), Is.EqualTo(false));
        }

        [Test]
        public void TryEvaluateSelector_Should_Fail()
        {
            var source = new SourceImplementation();
            Assert.That(source.TryEvaluateSelector(FormattingInfoExtensions.Create("{Dummy}", new List<object?>())), Is.EqualTo(false));
        }
    }
}
