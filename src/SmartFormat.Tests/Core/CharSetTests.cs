using System;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Tests.Core;

[TestFixture]
internal class CharSetTests
{
    [Test]
    public void CharSet_Add_Remove()
    {
        char[] asciiChars = ['A', 'B', 'C'];
        char[] nonAsciiChars = ['Ā', 'Б', '中'];
        var charSet = new CharSet();
        charSet.AddRange(asciiChars.AsEnumerable());
        charSet.AddRange(nonAsciiChars.AsSpan());
        var countBeforeRemoval = charSet.Count;
        var existingRemoved = charSet.Remove('C');
        charSet.Remove('中');
        // trying to remove a not existing char returns false
        var nonExistingRemoved = charSet.Remove('?');
        var count = charSet.Count;

        Assert.Multiple(() =>
        {
            Assert.That(countBeforeRemoval, Is.EqualTo(asciiChars.Length + nonAsciiChars.Length));
            Assert.That(count, Is.EqualTo(countBeforeRemoval - 2));
            Assert.That(existingRemoved, Is.True);
            Assert.That(nonExistingRemoved, Is.False);   
        });
    }

    [Test]
    public void CharSet_CreateFromSpan_GetCharacters_Contains()
    {
        char[] asciiAndNonAscii = ['\0', 'A', 'B', 'C', 'Ā', 'Б', '中'];
        var charSet = new CharSet(asciiAndNonAscii.AsSpan());

        Assert.Multiple(() =>
        {
            Assert.That(charSet, Has.Count.EqualTo(7));
            Assert.That(charSet.Contains('A'), Is.True); // ASCII
            Assert.That(charSet.Contains('\0'), Is.True); // control character
            Assert.That(charSet.Contains('中'), Is.True); // non-ASCII
            Assert.That(charSet.Contains('?'), Is.False);
            Assert.That(charSet.GetCharacters(), Is.EquivalentTo(asciiAndNonAscii));
            charSet.Clear();
            Assert.That(charSet, Has.Count.EqualTo(0));
            Assert.That(charSet.GetCharacters(), Is.Empty);
        });
    }
}
