using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.ZString;

namespace SmartFormat.Tests.Core.Output;

internal class CustomOutput : IOutput
{
    private readonly StringBuilder _sb = new(1000);
    private const string OutputFormat = "Format: {0}, Selector {1}, Formatted: {2}";

    public override string ToString()
    {
        return _sb.ToString();
    }

    public void Write(string text, IFormattingInfo? formattingInfo = null)
    {
        Write(text.AsSpan(), formattingInfo);
    }

    public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo = null)
    {
        var fi = (FormattingInfo) formattingInfo!;
        _sb.AppendFormat(OutputFormat, fi.Format, fi.Selector, text.ToString());
    }

    public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo = null)
    {
        Write(stringBuilder.AsSpan(), formattingInfo);
    }
}

[TestFixture]
internal class CustomOutputTests
{
    [Test]
    public void CustomOutput_GetsValid_FormattingInfo_Argument()
    {
        // This test ensures that the IOutput.Write method overloads
        // get a valid IFormattingInfo argument.
        var smart = Smart.CreateDefaultSmartFormat();
        var output = new CustomOutput();
        smart.FormatInto(output, CultureInfo.InvariantCulture, "{0:0.0000}", [9m]);
        Assert.That(output.ToString(),
            Is.EqualTo("Format: 0.0000, Selector 0, Formatted: 9.0000"));
    }
}
