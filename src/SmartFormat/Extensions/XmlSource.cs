using System.Collections;
using System.Collections.Generic;
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
		        foreach (var el in element.Elements())
		        {
		            if (el.Name.LocalName.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparison()))
		            {
		                result = el.Value;
		                handled = true;
		                return;
		            }
		        }
		    }

		}
	}
}
