using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
	public class DefaultFormatter : IFormatter
	{
		private string[] names = { "default", "d" };
		public string[] Names { get { return names; } set { names = value; } }
		
		/// <summary>
		/// Do the default formatting, same logic as "String.Format".
		/// </summary>
		public void TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			// This function always handles the method:
			formattingInfo.Handled = true;

			var format = formattingInfo.Format;
			var formatDetails = formattingInfo.FormatDetails;
			var output = formattingInfo.FormatDetails.Output;
			var current = formattingInfo.CurrentValue;

			// If the format has nested placeholders, we process those first
			// instead of formatting the item:
			if (format != null && format.HasNested)
			{
				formatDetails.Formatter.Format(output, format, current, formatDetails);
				return;
			}

			// If the object is null, we shouldn't write anything
			if (current == null)
			{
				current = "";
			}


			//  (The following code was adapted from the built-in String.Format code)

			//  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
			var formatter = formatDetails.Formatter;
			string result = null;
			ICustomFormatter cFormatter;
			IFormattable formattable;
			// Use the provider to see if a CustomFormatter is available:
			if (formatDetails.Provider != null && (cFormatter = formatDetails.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter) != null)
			{
				var formatText = format == null ? null : format.GetText();
				result = cFormatter.Format(formatText, current, formatDetails.Provider);
			}
			// IFormattable:
			else if ((formattable = current as IFormattable) != null)
			{
				var formatText = format == null ? null : format.ToString();
				result = formattable.ToString(formatText, formatDetails.Provider);
			}
			// ToString:
			else
			{
				result = current.ToString();
			}


			// Now that we have the result, let's output it (and consider alignment):


			// See if there's a pre-alignment to consider:
			if (formattingInfo.Alignment > 0)
			{
				var spaces = formattingInfo.Alignment - result.Length;
				if (spaces > 0)
				{
					formattingInfo.Write(new String(' ', spaces));
				}
			}

			// Output the result:
			formattingInfo.Write(result);


			// See if there's a post-alignment to consider:
			if (formattingInfo.Alignment < 0)
			{
				var spaces = -formattingInfo.Alignment - result.Length;
				if (spaces > 0)
				{
					formattingInfo.Write(new String(' ', spaces));
				}
			}
		}

	}
}
