using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using FormatException = SmartFormat.Core.Formatting.FormatException;

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
			var chooseOptions = formattingInfo.FormatterOptions.Split(splitChar);
			var formats = formattingInfo.Format.Split(splitChar);
			var chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);

			// Output the chosenFormat:
			formattingInfo.Write(chosenFormat, formattingInfo.CurrentValue);
		}

		private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats, string[] chooseOptions)
		{
			var currentValue = formattingInfo.CurrentValue;
			var currentValueString = (currentValue == null) ? "null" : currentValue.ToString();

			var chosenIndex = Array.IndexOf(chooseOptions, currentValueString);
			
			// Validate the number of formats:
			if (choiceFormats.Count < chooseOptions.Length)
			{
				throw formattingInfo.FormattingException("You must specify at least " + chooseOptions.Length + " choices");
			}
			else if (choiceFormats.Count > chooseOptions.Length + 1)
			{
				throw formattingInfo.FormattingException("You cannot specify more than " + (chooseOptions.Length + 1) + " choices");
			}
			else if (chosenIndex == -1 && choiceFormats.Count == chooseOptions.Length)
			{
				throw formattingInfo.FormattingException("\"" + currentValueString + "\" is not a valid choice, and a \"default\" choice was not supplied");
			}

			if (chosenIndex == -1) {
				chosenIndex = choiceFormats.Count - 1;
			}

			var chosenFormat = choiceFormats[chosenIndex];
			return chosenFormat;
		}

		public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			// This extension must be invoked via named formatter.
			return false;
		}
	}
}