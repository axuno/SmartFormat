using System;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
	public class FormattingInfo : IFormattingInfo, ISelectorInfo
	{
		public FormattingInfo(object currentValue, Format format, FormatDetails formatDetails)
		{
			CurrentValue = currentValue;
			Format = format;
			FormatDetails = formatDetails;
		}

		public FormattingInfo(FormatDetails formatDetails)
		{
			this.FormatDetails = formatDetails;
		}

		public Selector Selector { get; set; }
		public object Result { get; set; }


		/// <summary>
		/// The current value that is to be formatted.
		/// </summary>
		/// <example>
		/// In "{Items.Length:choose(1,2,3):one|two|three}",
		/// the CurrentValue would be the value of "Items.Length".
		/// </example>
		public object CurrentValue { get; set; }
		/// <summary>
		/// A flag to indicate that formatting has been handled.
		/// </summary>
		[Obsolete("Named formatters has made this flag obsolete. Still available, though, for backwards compatibility.")]
		public bool Handled { get; set; }
		public FormatDetails FormatDetails { get; private set; }


		public void SetCurrent(object currentValue, Placeholder placeholder)
		{
			this.CurrentValue = currentValue;
			this.Placeholder = placeholder;
			this.Format = placeholder.Format;
		}

		public Placeholder Placeholder { get; private set; }
		public int Alignment { get { return this.Placeholder.Alignment; }}
		public string FormatterOptions { get { return Placeholder.FormatterOptions; } }

		public Format Format { get; private set; }

		/// <summary>
		/// Writes a string to the output.
		/// </summary>
		/// <param name="text"></param>
		public void Write(string text)
		{
			this.FormatDetails.Output.Write(text, this);
		}

		/// <summary>
		/// Writes a substring to the output.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		public void Write(string text, int startIndex, int length)
		{
			this.FormatDetails.Output.Write(text, startIndex, length, this);
		}

	}
}