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
            // See if current is a IDictionary and contains the selector:
            var rawDict = current as IDictionary;

            if (rawDict != null && rawDict.Contains(selector.Text))
            {
                result = rawDict[selector.Text];
                handled = true;
			}

			var dict = current as IDictionary<string, object>;

			if (dict != null && dict.ContainsKey(selector.Text))
			{
				result = dict[selector.Text];
				handled = true;
			}
        }
    }
}
