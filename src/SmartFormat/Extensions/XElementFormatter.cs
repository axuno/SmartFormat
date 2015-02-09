using System.Collections.Generic;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class XElementFormatter : IFormatter
	{
		private string[] names = {"xelement", "xml", "x"};
		public string[] Names { get { return names; } set { names = value; } }

		public void EvaluateFormat(FormattingInfo formattingInfo)
		{
			var format = formattingInfo.Format;
			var current = formattingInfo.CurrentValue;
			var formatDetails = formattingInfo.FormatDetails;
			var output = formattingInfo.FormatDetails.Output;

			XElement currentXElement = null;
			if (format != null && format.HasNested) return;
			// if we need to format list of XElements then we just take and format first
			var xElmentsAsList = current as IList<XElement>;
			if (xElmentsAsList != null && xElmentsAsList.Count > 0)
			{
				currentXElement = xElmentsAsList[0];
				formattingInfo.Handled = true;
			}

			var currentAsXElement = (currentXElement) ?? current as XElement;
			if (currentAsXElement != null)
			{
				formattingInfo.Write(currentAsXElement.Value);
				formattingInfo.Handled = true;
			}
		}

	}

}
