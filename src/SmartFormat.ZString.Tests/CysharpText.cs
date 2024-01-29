//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using NUnit.Framework;

namespace SmartFormat.ZString.Tests;

[TestFixture]
public class CysharpText
{
    /// <summary>
    /// To stop namespace collision with Cysharp.Text nuget package,
    /// change all public types in namespace Cysharp.Text to internal.
    /// This can easily happen when Cysharp.Text is updated in the SmartFormat.ZString project.
    /// Using https://github.com/zzzprojects/findandreplace on command line, so we can automate this step:
    /// "fnr.exe" --cl --dir "SmartFormat.ZString\repo\src\ZString" --fileMask "*.cs,*.tt" --includeSubDirectories --caseSensitive --useRegEx --find "public (?=.*(class |struct |enum |interface |delegate ))" --replace "internal "
    /// </summary>
    [Test]
    public void Classes_Of_Cysharp_Text_Namespace_Should_Be_Internal()
    {
        // If this tests fails, you need to run the above command line
        // to change all public types in namespace Cysharp.Text to internal
        Assert.Multiple(() =>
        {
            Assert.That(typeof(Cysharp.Text.Utf16ValueStringBuilder).IsPublic, Is.False);
            Assert.That(typeof(Cysharp.Text.Utf8ValueStringBuilder).IsPublic, Is.False);
            Assert.That(typeof(Cysharp.Text.ZString).IsPublic, Is.False);
            Assert.That(typeof(Cysharp.Text.FormatParser).IsPublic, Is.False);
            Assert.That(typeof(System.HexConverter).IsPublic, Is.False);
        });
    }
}
