using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class DefaultSource : ISource
	{
		public DefaultSource(SmartFormatter formatter)
		{
			formatter.Parser.AddOperators(","); // This is for alignment.
			formatter.Parser.AddAdditionalSelectorChars("-"); // This is for alignment.
		}

		/// <summary>
		/// Performs the default index-based selector, same as String.Format.
		/// </summary>
		public void EvaluateSelector(FormattingInfo formattingInfo)
		{
			var current = formattingInfo.CurrentValue;
			var selector = formattingInfo.Selector;
			var formatDetails = formattingInfo.FormatDetails;

			int selectorValue;
			if (int.TryParse(selector.Text, out selectorValue))
			{
				// Argument Index:
				// Just like String.Format, the arg index must be in-range,
				// should be the first item, and shouldn't have any operator:
				if (selector.SelectorIndex == 0
					&& selectorValue < formatDetails.OriginalArgs.Length
					&& selector.Operator == "")
				{
					// This selector is an argument index.
					formattingInfo.CurrentValue = formatDetails.OriginalArgs[selectorValue];
					formattingInfo.Handled = true;
				}
				// Alignment:
				// An alignment item should be preceded by a comma
				else if (selector.Operator == ",")
				{
					// This selector is actually an Alignment modifier.
					formattingInfo.CurrentValue = current; // (don't change the current item)
					//placeholder.Alignment = selectorValue; // Set the alignment
					formattingInfo.Handled = true;
				}
			}
		}
	}
}
