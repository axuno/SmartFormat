using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Core;

[TestFixture]
class FormatCacheTests
{
    private static SmartFormatter GetSimpleFormatter()
    {
        var formatter = new SmartFormatter()
            .AddExtensions(new DefaultFormatter())
            .AddExtensions(new ReflectionSource(), new DefaultSource());
        return formatter;
    }

    [Test]
    public void Format_WithCache()
    {
        var data = new {Name = "Joe", City = "Melbourne"};
        var formatter = GetSimpleFormatter();
        var formatString = "{Name}, {City}";
        var format = formatter.Parser.ParseFormat(formatString);
        Assert.That(formatter.Format(format, data), Is.EqualTo($"{data.Name}, {data.City}"));
    }

    [Test]
    public void Format_WithCache_List_Args()
    {
        var data = new System.Collections.Generic.List<object?> { "Joe", "Melbourne" };
        var formatter = GetSimpleFormatter();
        var formatString = "{0}, {1}";
        var format = formatter.Parser.ParseFormat(formatString);
        Assert.That(formatter.Format(format, data), Is.EqualTo($"{data[0]}, {data[1]}"));
    }

    [Test]
    public void Format_WithCache_Into_StringOutput()
    {
        var data = new {Name = "Joe", City = "Melbourne"};
        var formatter = GetSimpleFormatter();
        var formatString = "{Name}, {City}";
        var format = formatter.Parser.ParseFormat(formatString);
        var output = new StringOutput();
        formatter.FormatInto(output, null, format, data);
        Assert.That(output.ToString(), Is.EqualTo($"{data.Name}, {data.City}"));
    }

    [Test]
    public void Format_WithCache_Into_ZStringOutput()
    {
        var data = new {Name = "Joe", City = "Melbourne"};
        var formatter = GetSimpleFormatter();
        var formatString = "{Name}, {City}";
        var format = formatter.Parser.ParseFormat(formatString);
        using var output = new ZStringOutput();
        formatter.FormatInto(output, null, format, data);
        Assert.That(output.ToString(), Is.EqualTo($"{data.Name}, {data.City}"));
    }
}
