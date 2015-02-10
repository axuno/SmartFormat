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

		private char splitChar = '|';
		public char SplitChar { get { return splitChar;  } set { splitChar = value; } }

		public void EvaluateFormat(IFormattingInfo formattingInfo)
		{
			var formats = formattingInfo.Format.Split(splitChar);
			Format chosenFormat;
			if (formats.Count == 1) {
				chosenFormat = formats[0];
			} else {
				var chooseOptions = formattingInfo.FormatterOptions.Split(splitChar);
				chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);
			}

			// Output the chosenFormat:
			formattingInfo.Write(chosenFormat, formattingInfo.CurrentValue);
		}

		private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> formats, string[] chooseOptions)
		{
			var currentValue = formattingInfo.CurrentValue;
			var currentValueString = (currentValue == null) ? "null" : currentValue.ToString();

			var chosenIndex = Array.IndexOf(chooseOptions, currentValueString);
			if (chosenIndex == -1 || chosenIndex >= formats.Count) {
				chosenIndex = formats.Count - 1;
			}

			var chosenFormat = formats[chosenIndex];
			return chosenFormat;
		}

		public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			// This extension must be invoked via named formatter.
			return false;
		}
	}
}