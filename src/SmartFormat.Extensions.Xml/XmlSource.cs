// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System.Linq;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions;

/// <summary>
/// Class to evaluate sources of type <see cref="XElement"/>.
/// Include this source, if this type shall be used.
/// </summary>
public class XmlSource : Source
{
    /// <inheritdoc />
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.CurrentValue is XElement element)
        {
            var selector = selectorInfo.SelectorText;
            // Find elements that match a selector
            var selectorMatchedElements =
                element.Elements()
                    .Where(x => x.Name.LocalName == selector)
                    .ToList();
            if (selectorMatchedElements.Any())
            {
                selectorInfo.Result = selectorMatchedElements;
                return true;
            }
        }

        return false;
    }
}