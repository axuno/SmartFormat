using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Text;
using NUnit.Framework;
using SmartFormat.Core.Output;

namespace SmartFormat.Tests.Core.Output
{
    [TestFixture]
    public class StringOutputTests
    {
        [Test]
        public void Output_Of_Span()
        {
            var so = new StringOutput();
            so.Write("text".AsSpan(), null!);
            Assert.AreEqual("text", so.ToString());
        }

        [Test]
        public void Output_Of_String()
        {
            var so = new StringOutput();
            so.Write("text", null!);
            Assert.AreEqual("text", so.ToString());
        }

        [Test]
        public void Output_Of_ValueStringBuilder()
        {
            var so = new StringOutput();
            using var sb = ZString.CreateStringBuilder();
            sb.Append("text");
            so.Write(sb, null!);
            Assert.AreEqual("text", so.ToString());
        }
    }
}
