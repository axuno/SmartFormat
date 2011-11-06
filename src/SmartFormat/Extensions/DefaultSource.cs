using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
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
            int selectorValue;
            if (int.TryParse(selector.Text, out selectorValue))
            {
                // Argument Index:
                // Just like String.Format, the arg index must be in-range, 
                // should be the first item, and shouldn't have any operator:
                if (selector.SelectorIndex == 0
                    && selectorValue < formatDetails.OriginalArgs.Length
                    && selector.Operator == "")
                {
                    // This selector is an argument index.
                    result = formatDetails.OriginalArgs[selectorValue];
                    handled = true;
                }
                // Alignment:
                // An alignment item should be preceeded by a comma
                else if (selector.Operator == ",")
                {
                    // This selector is actually an Alignment modifier.
                    result = current; // (don't change the current item)
                    formatDetails.Placeholder.Alignment = selectorValue; // Set the alignment
                    handled = true;
                }
            }
        }
    }
}
