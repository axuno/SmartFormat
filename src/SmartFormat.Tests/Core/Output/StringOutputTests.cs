using System;
using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.ZString;

namespace SmartFormat.Tests.Core.Output;

[TestFixture]
public class StringOutputTests
{
    [Test]
    public void Output_Of_Span()
    {
        var so = new StringOutput();
        so.Write("text".AsSpan(), null);
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void Output_Of_String()
    {
        var so = new StringOutput(16);
        so.Write("text", null);
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void Output_Of_ValueStringBuilder()
    {
        var so = new StringOutput();
        using var sb = ZStringBuilderUtilities.CreateZStringBuilder();
        sb.Append("text");
        so.Write(sb, null);
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }
}
