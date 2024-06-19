//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
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
}
