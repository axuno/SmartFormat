using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RTF;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Demo.Sample_Extensions
{
	public class RTFOutput : IOutput
	{
		public RTFOutput(Color[] nestedColors, Color errorColor)
		{
			if (nestedColors == null || nestedColors.Length == 0) throw new ArgumentException("Nested colors cannot be null or empty.");
			this.nestedColors = nestedColors;
			this.errorColor = errorColor;
		}

		private readonly Color[] nestedColors;

		private RTFBuilder output = new RTFBuilder();
		private Color errorColor;

		public void Clear()
		{
			//output.Clear();
			output = new RTFBuilder();
		}

		public void Write(string text, FormattingInfo formattingInfo)
		{
			Write(text, 0, text.Length, formattingInfo);
		}

		public void Write(string text, int startIndex, int length, FormattingInfo formattingInfo)
		{
			// Depending on the nested level, we will color this item differently:
			if (formattingInfo.FormatDetails.FormatError != null)
			{
				output.BackColor(errorColor).Append(text, startIndex, length);
			}
			else if (formattingInfo.Placeholder == null)
			{
				// There is no "nesting" so just output plain text:
				output.Append(text, startIndex, length);
			}
			else
			{
				var nestedDepth = formattingInfo.Placeholder.NestedDepth;
				var backcolor = this.nestedColors[nestedDepth % nestedColors.Length];
				output.BackColor(backcolor).Append(text, startIndex, length);
			}
		}


		public override string ToString()
		{
			return output.ToString();
		}
	}
}
