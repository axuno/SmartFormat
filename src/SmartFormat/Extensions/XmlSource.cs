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

		public void EvaluateSelector(FormattingInfo formattingInfo)
		{
			var current = formattingInfo.CurrentValue;
			var selector = formattingInfo.Selector;

			var element = current as XElement;
			if (element != null)
			{
				// Find elements that match a selector
				var selectorMatchedElements = element.Elements()
					.Where(x => x.Name.LocalName == selector.Text).ToList();
				if (selectorMatchedElements.Any())
				{
					formattingInfo.CurrentValue = selectorMatchedElements;
					formattingInfo.Handled = true;
				}
			}
		}
	}
}
