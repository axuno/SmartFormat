using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class SmartExtensionsTests
{
    #region : StringBuilderTests :

    public static object[] GetArgs()
    {
        return new object[] {
            TimeSpan.Zero,
            new TimeSpan(1,1,1,1,1),
            new TimeSpan(0,2,0,2,0),
            new TimeSpan(3,0,0,3,0),
            new TimeSpan(0,0,0,0,4),
            new TimeSpan(5,0,0,0,0),
        };
    }

    [Test]
    public void Test_AppendLine()
    {
        var sb = new StringBuilder();
        sb.AppendLineSmart("{0} {1} {2} {3}", "these", "are", "the", "args");

        var actual = sb.ToString();

        Assert.That(actual, Is.EqualTo("these are the args" + Environment.NewLine));
    }

    [Test]
    public void Test_Append()
    {
        var sb = new StringBuilder();
        sb.AppendSmart("{0} {1} {2} {3}", "these", "are", "the", "args");

        var actual = sb.ToString();

        Assert.That(actual, Is.EqualTo("these are the args"));
    }

    #endregion

    #region : TextWriterTests :

    [Test]
    public void WriteSmartTest()
    {
        var sb = new StringBuilder();
        var text = "abc";
        var fmt = "{0}";
        var sw = new StringWriter(sb);
        sw.WriteSmart("{0}", text);
        sw.Flush();
        sw.Close();
        Assert.AreEqual(string.Format(fmt, text), sb.ToString());
    }

    [Test]
    public void WriteLineSmartTest()
    {
        var sb = new StringBuilder();
        var text = "abc";
        var fmt = "{0}";
        var sw = new StringWriter(sb);
        sw.WriteLineSmart("{0}", text);
        sw.Flush();
        sw.Close();
        Assert.AreEqual(string.Format(fmt, text) + Environment.NewLine, sb.ToString());
    }

    #endregion

    #region : StringExtensionTests :

    [Test]
    public void StringFormatSmartTest()
    {
        var text = "abc";
        var fmt = "{0}";
        var result = fmt.FormatSmart(text);

        Assert.AreEqual(string.Format(fmt, text), result);
    }

    #endregion
}