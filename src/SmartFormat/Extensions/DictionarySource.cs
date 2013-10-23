using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    public class DictionarySource : ISource
    {
        public DictionarySource(SmartFormatter formatter)
        {
            // Add some special info to the parser:
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            formatter.Parser.AddOperators(".");
        }

        public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            // See if current is a IDictionary and contains the selector
            var dict = current as IDictionary;
            if (dict != null && dict.Contains(selector.Text))
            {
                result = dict[selector.Text];
                handled = true;
                return;
            }

            // See if current is an IDictionary<string, object> (also an "expando" object) 
            // and contains the selector.
            var genericDictionary = current as IDictionary<string, object>;
            if (genericDictionary != null && genericDictionary.ContainsKey(selector.Text))
            {
                result = genericDictionary[selector.Text];
                handled = true;
            }
        }
    }
}
