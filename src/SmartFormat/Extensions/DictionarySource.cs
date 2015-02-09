using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		public void EvaluateSelector(FormattingInfo formattingInfo)
		{
			var current = formattingInfo.CurrentValue;
			var selector = formattingInfo.Selector;

			// See if current is a IDictionary and contains the selector:
			var rawDict = current as IDictionary;
			if (rawDict != null)
			{
				foreach (DictionaryEntry entry in rawDict)
				{
					var key = (entry.Key as string) ?? entry.Key.ToString();

					if (key.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparison()))
					{
						formattingInfo.CurrentValue = entry.Value;
						formattingInfo.Handled = true;
						return;
					}
				}
			}

			// this check is for dynamics and generic dictionaries
			var dict = current as IDictionary<string, object>;

			if (dict != null)
			{
				var val = dict.FirstOrDefault(x => x.Key.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparison())).Value;
				if (val != null)
				{
					formattingInfo.CurrentValue = val;
					formattingInfo.Handled = true;
				}
			}
		}
	}
}
