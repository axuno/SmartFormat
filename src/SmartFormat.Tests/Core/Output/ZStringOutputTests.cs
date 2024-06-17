using System;
using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Core.Output;

[TestFixture]
public class ZStringOutputTests
{
    [Test]
    public void Create_With_Capacity()
    {
        using var zStringOutput = new ZStringOutput(ZString.ZStringBuilderUtilities.DefaultBufferSize + 10000);
        Assert.Multiple(() =>
        {
            Assert.That(zStringOutput.Output, Is.InstanceOf<ZString.ZStringBuilder>());
            Assert.That(zStringOutput, Is.Not.Null);
        });
    }

    [Test]
    public void Create_With_Other_ValueStringBuilder()
    {
        using var vsb = ZString.ZStringBuilderUtilities.CreateZStringBuilder();
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
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void Output_Of_String()
    {
        var so = new ZStringOutput();
        so.Write("text", null);
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void Output_Of_ValueStringBuilder()
    {
        var so = new ZStringOutput();
        using var sb = ZString.ZStringBuilderUtilities.CreateZStringBuilder();
        sb.Append("text");
        so.Write(sb, null);
        Assert.That(so.ToString(), Is.EqualTo("text"));
    }

    [Test]
    public void CreateZStringBuilder_from_Format()
    {
        var input = new string('a', 123);
        var format = new SmartFormat.Core.Parsing.Format().Initialize(new SmartSettings(), input, 0, input.Length);
        // The capacity is calculated from the format length and the number of items
        var sb = ZString.ZStringBuilderUtilities.CreateZStringBuilder(format);
        sb.Append(input);
        Assert.That(sb.Length, Is.EqualTo(input.Length));
    }
}
