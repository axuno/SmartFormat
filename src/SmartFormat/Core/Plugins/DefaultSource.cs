using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    [PluginPriority(PluginPriority.Low)]
    public class DefaultSource : ISource
    {
        public DefaultSource(SmartFormatter formatter)
        {
            formatter.Parser.AddOperators(","); // This is for alignment.
            formatter.Parser.AddAdditionalSelectorChars("-"); // This is for alignment.
        }

        /// <summary>
        /// Performs the default index-based selector, same as String.Format.
        /// </summary>
        public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            // Make sure the selector is a valid in-range index:
            int selectorValue;
            if (int.TryParse(selector.Text, out selectorValue))
            {
                if (selector.Operator == ",")
                {
                    // This selector is actually an Alignment modifier.
                    result = current; // (don't change the current item)
                    formatDetails.Placeholder.Alignment = selectorValue; // Set the alignment
                    handled = true;
                }
                else if (selectorValue < formatDetails.OriginalArgs.Length)
                {
                    // This selector is an argument index.
                    result = formatDetails.OriginalArgs[selectorValue];
                    handled = true;
                }
            }
        }
    }
}
