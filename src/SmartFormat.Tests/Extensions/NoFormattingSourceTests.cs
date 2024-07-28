using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class NoFormattingSourceTests
{
    [Test]
    public void Use_of_ToggleFormattingExtensions()
    {
        var smart = new SmartFormatter();
        smart.AddExtensions(new NoFormattingSource());
        smart.AddExtensions(new DefaultFormatter());

        Assert.That(smart.Format("{0}", 999), Is.EqualTo("No formatting"));
    }

    public class NoFormattingSource : ISource
    {
        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // Split test for IFormattingExtensionsToggle and FormattingInfo
            // for clarity. This is not necessary in production code.

            // Disable all formatting extensions
            if (selectorInfo is IFormattingExtensionsToggle toggle)
            {
                toggle.DisableFormattingExtensions = true;
            }

            if (selectorInfo is FormattingInfo fi)
            {
                // Write a note or result directly to the output
                fi.Write("No formatting");
            }

            selectorInfo.Result = selectorInfo.CurrentValue;
            return true;
        }
    }
}
