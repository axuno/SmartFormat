using System;
using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Core.Parsing;

[TestFixture]
public class SplitListTests
{
    private static Parser GetRegularParser(SmartSettings? settings = null)
    {
        var parser = new Parser(settings ?? new SmartSettings
            { StringFormatCompatibility = false, Parser = new ParserSettings { ErrorAction = ParseErrorAction.ThrowError } });
        return parser;
    }

    [Test]
    public void Test_Format_Split()
    {
        var parser = GetRegularParser();
        const string format = " a|aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg|g ";
        var parsedFormat = parser.ParseFormat(format);
        var splits = parsedFormat.Split('|');

        Assert.That(splits, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(splits[0].ToString(), Is.EqualTo(" a"));
            // Split character inside nested format (Placeholder) is ignored:
            Assert.That(splits[1].ToString(), Is.EqualTo("aa {bbb: ccc dd|d {:|||} {eee} ff|f } gg"));
            Assert.That(splits[2].ToString(), Is.EqualTo("g "));
        });
    }

    [Test]
    public void SplitList_Should_EnumerateFormats()
    {
        var parser = GetRegularParser();
        // Pure literal with split characters
        const string format = "a|b|c";
        var parsedFormat = parser.ParseFormat(format);
        var splits = parsedFormat.Split('|');

        // foreach is using GetEnumerator()
        var result1 = string.Empty;
        foreach (var split in splits)
        {
            result1 += split.ToString();
        }

        // Use explicit interface for GetEnumerator
        var result2 = string.Empty;
        var enumerator = ((System.Collections.IEnumerable) splits).GetEnumerator();
        using var enumeratorDisposable = (IDisposable) enumerator;
        while (enumerator.MoveNext())
        {
            result2 += enumerator.Current!.ToString();
        }

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo("abc"));
            Assert.That(result2, Is.EqualTo("abc"));
            Assert.That(splits.IsReadOnly, Is.EqualTo(true));
        });
    }

    [Test]
    public void CopyTo_ShouldCopyFormats()
    {
        var parser = GetRegularParser();
        // Pure literal with split characters
        const string format = "a|b|c";
        var parsedFormat = parser.ParseFormat(format);
        var splits = parsedFormat.Split('|');
        var array = new Format[3];
        splits.CopyTo(array, 0);

        Assert.That(array, Is.EquivalentTo(splits));
    }

    [Test]
    public void Indexer_ShouldThrow_IfCacheNotFilled()
    {
        var parser = GetRegularParser();
        // Pure literal with split characters
        const string format = "a|b";
        var parsedFormat = parser.ParseFormat(format);
        // Initialize SplitList directly without calling SplitList.CreateSplitCache()
        var splits = new SplitList().Initialize(parsedFormat, [format.IndexOf('|')]);

        Assert.That(() => _ = splits[0], Throws.Exception.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void InvalidIndexCall_ShouldThrow()
    {
        var parser = GetRegularParser();
        // Pure literal with split characters
        const string format = "a|b|c";
        var parsedFormat = parser.ParseFormat(format);
        var splits = parsedFormat.Split('|');

        Assert.That(() => _ = splits[int.MaxValue], Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void UnsupportedMemberCalls_ShouldThrow()
    {
        // Track although SplitList is internal

        var parser = GetRegularParser();
        // Pure literal with split characters
        const string format = "a|b|c";
        var fmt = parser.ParseFormat(format);
        var splits = fmt.Split('|');

        Assert.Multiple(() =>
        {
            Assert.That(() => splits[0] = fmt, Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.IndexOf(fmt), Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.Insert(0, fmt), Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.Add(fmt), Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.Contains(fmt), Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.Remove(fmt), Throws.Exception.InstanceOf<NotSupportedException>());
            Assert.That(() => splits.RemoveAt(0), Throws.Exception.InstanceOf<NotSupportedException>());
        });
    }
}
