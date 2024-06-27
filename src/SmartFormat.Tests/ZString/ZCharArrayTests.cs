//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.ZString;

namespace SmartFormat.Tests.ZString;

public class ZCharArrayTests
{
    [Test]
    public void Grow_Buffer()
    {
        using var buffer = new ZCharArray(1);

        var originalCapacity = buffer.Capacity;
        buffer.Write('!', originalCapacity);
        buffer.Write("Hello World");

        Assert.Multiple(() =>
        {
            Assert.That(buffer.Capacity, Is.GreaterThan(originalCapacity));
            Assert.That(buffer.Span.Slice(0, originalCapacity).ToString(), Is.EqualTo(new string('!', originalCapacity)));
            Assert.That(buffer.Span.Slice(originalCapacity).ToString(), Is.EqualTo("Hello World"));
        });
    }

    [Test]
    public void Write_Span()
    {
        using var buffer = new ZCharArray();

        buffer.Write("Hello World".ToCharArray().AsSpan());
        Assert.That(buffer.Length, Is.EqualTo(11));
        Assert.That(buffer.ToString(), Is.EqualTo("Hello World"));
    }

    [Test]
    public void Buffer_Should_Throw_If_Used_When_Disposed()
    {
        var buffer = new ZCharArray();
        buffer.Dispose();

        Assert.That(() => buffer.Write(string.Empty), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Disposing_More_Than_Once_Will_Do_Nothing()
    {
        var buffer = new ZCharArray();
        buffer.Dispose();

        Assert.That(() => buffer.Dispose(), Throws.Nothing);
    }

#if NET6_0_OR_GREATER
    [Test]
    public void Buffer_Write_ISpanFormattable()
    {
        var buffer = new ZCharArray();
        
        buffer.Write((ISpanFormattable) 12.34, "0.0000".AsSpan(), CultureInfo.InvariantCulture);

        Assert.That(buffer.ToString(), Is.EqualTo("12.3400"));
    }
#endif

    [Test]
    public void Buffer_Write_IFormattable()
    {
        var buffer = new ZCharArray();
        buffer.Write((IFormattable) 12.34, "0.0000", CultureInfo.InvariantCulture);

        Assert.That(buffer.ToString(), Is.EqualTo("12.3400"));
    }

    [Test]
    public void Buffer_Reset_Size_To_Zero()
    {
        var buffer = new ZCharArray();
        buffer.Write("Hello");

        var lengthBeforeReset = buffer.Length;
        buffer.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(lengthBeforeReset, Is.EqualTo(5));
            Assert.That(buffer.Length, Is.EqualTo(0));
        });
    }

    [Test]
    public void Buffer_Thread_Safety()
    {
        // The ZCharArray struct itself is thread-safe, meaning that
        // individual operations on an instance of ZCharArray can be
        // safely performed concurrently from multiple threads 

        const int maxLoops = 100;
        var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
        var list = new ConcurrentBag<string>();

        Assert.That(() =>
        {
            Parallel.For(1L, maxLoops, options, (i, loopState) =>
            {
                using var buffer = new ZCharArray();
                buffer.Write("Number: ");
                buffer.Write(i.ToString("00000"));
                list.Add(buffer.ToString());
            });
        }, Throws.Nothing);

        var result = list.OrderBy(e => e);
        long compareCounter = 1;
        Assert.Multiple(() =>
        {
            Assert.That(list, Has.Count.EqualTo(maxLoops - 1));
            Assert.That(result.All(r => r == $"Number: {compareCounter++:00000}"));
        });
    }
}
