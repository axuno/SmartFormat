// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// A class used to format and output <see cref="XElement"/>s.
    /// </summary>
    public class XElementFormatter : IFormatter
    {
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"xelement", "xml", "x", string.Empty};

        ///<inheritdoc/>
        public string Name { get; set; } = "xml";

        ///<inheritdoc/>
        public bool CanAutoDetect { get; set; } = true;

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // Check whether arguments can be handled by this formatter
            if (format is {HasNested: true})
            {
                // Auto detection calls just return a failure to evaluate
                if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                    return false;

                // throw, if the formatter has been called explicitly
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' cannot process nested formats.");
            }

            XElement? currentXElement = null;

            // if we need to format list of XElements then we just take and format the first in the list
            if (current is IList<XElement> {Count: > 0} xElementsAsList) currentXElement = xElementsAsList[0];

            var currentAsXElement = currentXElement ?? current as XElement;
            if (currentAsXElement != null)
            {
                formattingInfo.Write(currentAsXElement.Value);
                return true;
            }

            return false;
        }
    }
}