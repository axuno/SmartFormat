using System.Linq;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class XmlSource : ISource
	{
		public XmlSource(SmartFormatter formatter)
		{
			// Add some special info to the parser:
			formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
			formatter.Parser.AddAdditionalSelectorChars("_");
			formatter.Parser.AddOperators(".");
		}

		public void EvaluateSelector(object current, Selector selector, ref bool handled,
            ref object result, FormatDetails formatDetails)
		{
		    var element = current as XElement;
		    if (element != null)
		    {
                // Find elements that match a selector
		        var selectorMatchedElements = element.Elements(XName.Get(selector.Text)).ToArray();
		        if (selectorMatchedElements.Any())
		        {
		            var firstMatch = selectorMatchedElements.First();
                    // if there are more XML child elements
		            if (!IsLastSelector(selector, formatDetails.Placeholder))
		            {
		                result = firstMatch;
		                handled = true;
		            }
		            else
		            {
		                result = firstMatch.Value;
		                handled = true;
		            }
		        }
		    }
		}

        private static bool IsLastSelector(FormatItem selector, Placeholder placeholder)
	    {
            /*
             * example:
             * returns `true` if selector `Name` for placeholder `Person.Name`
             * returns `false` if selector `Person` for placeholder `Person.Name`
             * returns `true` if selector `Person` for placeholder `Person`
             */

            return placeholder
                .Selectors.Select(s => s.Text)
                .Last().Equals(selector.Text);
	    }
	}
}
