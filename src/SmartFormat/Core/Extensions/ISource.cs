// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions;

/// <summary>
/// Evaluates a selector.
/// </summary>
public interface ISource
{
    /// <summary>
    /// Evaluates the <see cref="Selector" /> based on the <see cref="ISelectorInfo.CurrentValue" />.
    /// </summary>
    /// <param name="selectorInfo">The information about the selector being evaluated.<br/>
    /// Note: This can be casted to <seealso cref="FormattingInfo"/>, which also implements <seealso cref="IFormattingExtensionsToggle"/>.</param>
    /// <returns>If the <see cref="Selector"/> could be evaluated,
    /// the <see cref="ISelectorInfo.Result" /> will be set and <see langword="true"/> will be returned.
    /// </returns>
    bool TryEvaluateSelector(ISelectorInfo selectorInfo);
}
