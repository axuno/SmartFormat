//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.Linq;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate sources of type <see cref="XElement"/>.
    /// </summary>
    public class XmlSource : Source
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public XmlSource(SmartFormatter formatter) : base(formatter)
        {
        }

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
}