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

		public void TryEvaluateSelector(ISelectorInfo selectorInfo)
		{
			var current = selectorInfo.CurrentValue;
			var selector = selectorInfo.Selector;

			// See if current is a IDictionary and contains the selector:
			var rawDict = current as IDictionary;
			if (rawDict != null)
			{
				foreach (DictionaryEntry entry in rawDict)
				{
					var key = (entry.Key as string) ?? entry.Key.ToString();

					if (key.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparison()))
					{
						selectorInfo.Result = entry.Value;
						selectorInfo.Handled = true;
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
					selectorInfo.Result = val;
					selectorInfo.Handled = true;
				}
			}
		}
	}
}
