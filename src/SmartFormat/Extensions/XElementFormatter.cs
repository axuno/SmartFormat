using System.Collections.Generic;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class XElementFormatter : IFormatter
	{
		#region IFormatter
	    private string[] names = {"xelement", "xml", "x"};
        public string[] Names { get { return names; } set { names = value; } }

		public void EvaluateFormat(object current, Format format,
			ref bool handled, IOutput output, FormatDetails formatDetails)
		{
			XElement currentXElement = null;
			if (format != null && format.HasNested) return;
			// if we need to format list of XElements then we just take and format first
			var xElmentsAsList = current as IList<XElement>;
			if (xElmentsAsList != null && xElmentsAsList.Count > 0)
			{
				currentXElement = xElmentsAsList[0];
				handled = true;
			}

			var currentAsXElement = (currentXElement) ?? current as XElement;
			if (currentAsXElement != null)
			{
				output.Write(currentAsXElement.Value, formatDetails);
				handled = true;
			}
		}

		#endregion
	}

}
