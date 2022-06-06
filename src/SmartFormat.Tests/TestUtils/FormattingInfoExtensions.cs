using System.Collections.Generic;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.TestUtils;

internal static class FormattingInfoExtensions
{
    /// <summary>
    /// Creates a new instance of <see cref="FormattingInfo"/>.
    /// </summary>
    /// <param name="format">The input format string.</param>
    /// <param name="data">The data argument.</param>
    /// <returns>A new instance of <see cref="FormattingInfo"/>.</returns>
    public static FormattingInfo Create(string format, IList<object?> data)
    {
        var formatter = new SmartFormatter(new SmartSettings());
        var formatParsed = formatter.Parser.ParseFormat(format);
        // use StringOutput because we don't have to care about disposing.
        var formatDetails = new FormatDetails().Initialize(formatter, formatParsed, data, null, new StringOutput());
        return new FormattingInfo().Initialize(formatDetails, formatDetails.OriginalFormat, data);
    }
}