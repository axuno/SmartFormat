using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class ChooseFormatter : IFormatter
	{
		private string[] names = { "choose", "c" };

		public string[] Names { get { return names; } set { names = value; } }

		public void EvaluateFormat(IFormattingInfo formattingInfo)
		{
			var formats = formattingInfo.Format.Split("|");
			Format chosenFormat;
			if (formats.Count == 1) {
				chosenFormat = formats[0];
			} else {
				chosenFormat = DetermineChosenFormat(formattingInfo, formats);
			}

			// Output the chosenFormat:
			formattingInfo.Write(chosenFormat);
		}

		private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> formats)
		{
			var chooseOptions = formattingInfo.FormatterOptions.Split(',');

			var currentValue = formattingInfo.CurrentValue;
			var currentValueString = (currentValue == null) ? "null" : currentValue.ToString();

			var chosenIndex = Array.IndexOf(chooseOptions, currentValueString);
			if (chosenIndex == -1 || chosenIndex >= formats.Count) {
				chosenIndex = formats.Count - 1;
			}

			var chosenFormat = formats[chosenIndex];
			return chosenFormat;
		}

		public void TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			// This extension must be invoked via named formatter.
		}
	}
}