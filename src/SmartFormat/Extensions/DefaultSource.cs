// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

/// <summary>
/// Class to evaluate an index-based <see cref="Selector"/>.
/// Include this source, if an indexed source shall be used just the way string.Format does.
/// </summary>
/// <example>
/// Smart.Format("{0}-{1}", 1234, 5678);
/// </example>
public class DefaultSource : Source
{
    /// <inheritdoc />
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        var selector = selectorInfo.SelectorText;
        var formatDetails = selectorInfo.FormatDetails;

        if (int.TryParse(selector, out var selectorValue)
            && selectorInfo.SelectorIndex == 0
            && selectorValue < formatDetails.OriginalArgs.Count
            && selectorInfo.SelectorOperator == string.Empty)
        {
            // Argument Index:
            // Just like string.Format, the arg index must be in-range,
            // must be the first item, and shouldn't have any operator
            // This selector is an argument index.
            // Get the value from arguments.
            selectorInfo.Result = formatDetails.OriginalArgs[selectorValue];
            return true;
        }

        return false;
    }
}