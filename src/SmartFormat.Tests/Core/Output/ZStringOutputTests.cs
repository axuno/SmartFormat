using System;
using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.ZString;

namespace SmartFormat.Tests.Core.Output;

[TestFixture]
public class ZStringOutputTests
{
    [Test]
    public void Create_With_Capacity()
    {
        using var zStringOutput = new ZStringOutput(SmartFormat.Utilities.ZStringBuilderExtensions.DefaultBufferSize + 10000);
        Assert.That(zStringOutput.Output, Is.InstanceOf<ZStringBuilder>());
        Assert.That(zStringOutput, Is.Not.Null);
    }

    [Test]
    public void Create_With_Other_ValueStringBuilder()
    {
        using var vsb = SmartFormat.Utilities.ZStringBuilderExtensions.CreateZStringBuilder();
        vsb.Append("text");
        using var zStringOutput = new ZStringOutput(vsb);
        Assert.That(zStringOutput, Is.Not.Null);
        Assert.That(zStringOutput.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void Output_Of_Span()
    {
        var so = new ZStringOutput();
        so.Write("text".AsSpan(), null);
        Assert.AreEqual("text", so.ToString());
    }

    [Test]
    public void Output_Of_String()
    {
        var so = new ZStringOutput();
        so.Write("text", null);
        Assert.AreEqual("text", so.ToString());
    }

    [Test]
    public void Output_Of_ValueStringBuilder()
    {
        var so = new ZStringOutput();
        using var sb = SmartFormat.Utilities.ZStringBuilderExtensions.CreateZStringBuilder();
        sb.Append("text");
        so.Write(sb, null);
        Assert.AreEqual("text", so.ToString());
    }
}