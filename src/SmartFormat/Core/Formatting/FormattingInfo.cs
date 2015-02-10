using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Formatting
{
	public class FormattingInfo : IFormattingInfo, ISelectorInfo
	{
		public FormattingInfo(FormatDetails formatDetails, Format format, object currentValue)
		{
			CurrentValue = currentValue;
			Format = format;
			FormatDetails = formatDetails;
		}

		public FormattingInfo(FormatDetails formatDetails, Placeholder placeholder, object currentValue)
		{
			this.FormatDetails = formatDetails;
			this.Placeholder = placeholder;
			this.Format = placeholder.Format;
			this.CurrentValue = currentValue;
		}

		public FormattingInfo CreateChild(Format format, object currentValue)
		{
			return new FormattingInfo(this.FormatDetails, format, currentValue);
		}

		public FormattingInfo CreateChild(Placeholder placeholder)
		{
			return new FormattingInfo(this.FormatDetails, placeholder, this.CurrentValue);
		}


		public FormatDetails FormatDetails { get; private set; }

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
		public bool Handled { get; set; }

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

		/// <summary>
		/// Writes the nested format to the output.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="value"></param>
		public void Write(Format format, object value)
		{
			var nestedFormatInfo = this.CreateChild(format, value);
			this.FormatDetails.Formatter.Format(nestedFormatInfo);
		}

	}
}