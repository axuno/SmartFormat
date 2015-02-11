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

		public object CurrentValue { get; set; }

		public Placeholder Placeholder { get; private set; }
		public int Alignment { get { return this.Placeholder.Alignment; }}
		public string FormatterOptions { get { return Placeholder.FormatterOptions; } }

		public Format Format { get; private set; }

		public void Write(string text)
		{
			this.FormatDetails.Output.Write(text, this);
		}

		public void Write(string text, int startIndex, int length)
		{
			this.FormatDetails.Output.Write(text, startIndex, length, this);
		}

		public void Write(Format format, object value)
		{
			var nestedFormatInfo = this.CreateChild(format, value);
			this.FormatDetails.Formatter.Format(nestedFormatInfo);
		}


		public FormattingException FormattingException(string issue, FormatItem problemItem = null, int startIndex = -1)
		{
			if (problemItem == null) problemItem = this.Format;
			if (startIndex == -1) startIndex = problemItem.startIndex;
			return new FormattingException(problemItem, issue, startIndex);
		}

	}
}