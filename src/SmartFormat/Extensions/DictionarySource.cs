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

		public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
		{
			// See if current is a IDictionary and contains the selector:
			var rawDict = current as IDictionary;

			if (rawDict != null)
			{
				foreach (DictionaryEntry entry in rawDict)
				{
					var key = (entry.Key as string) ?? entry.Key.ToString();

					if (key.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparision()))
					{
						result = entry.Value;
						handled = true;
						return;
					}
				}
			}

			// this check is for dynamics and generic dictionaries
			var dict = current as IDictionary<string, object>;

			if (dict != null)
			{
				var val = dict.FirstOrDefault(x => x.Key.Equals(selector.Text, Smart.Settings.GetCaseSensitivityComparision())).Value;
				if (val != null)
				{
					result = val;
					handled = true;
				}
			}
		}
	}
}
