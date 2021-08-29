using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Text;
using NUnit.Framework;
using SmartFormat.Core.Output;

namespace SmartFormat.Tests.Core.Output
{
    [TestFixture]
    public class TextWriterOOutputTests
    {
        [Test]
        public void Output_Of_Span()
        {
            var sw = new StringWriter(new StringBuilder());
            var two = new TextWriterOutput(sw);
            two.Write("text".AsSpan(), null);
            sw.Flush();
            Assert.AreEqual("text", sw.ToString());
        }

        [Test]
        public void Output_Of_String()
        {
            var sw = new StringWriter(new StringBuilder());
            var two = new TextWriterOutput(sw);
            two.Write("text", null);
            sw.Flush();
            Assert.AreEqual("text", sw.ToString());
        }

        [Test]
        public void Output_Of_ValueStringBuilder()
        {
            using var sb = ZString.CreateStringBuilder();
            sb.Append("text");
            var sw = new StringWriter(new StringBuilder());
            var two = new TextWriterOutput(sw);
            two.Write(sb, null);
            sw.Flush();
            Assert.AreEqual("text", sw.ToString());
        }
    }
}
