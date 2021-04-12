//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Formatting
{
    public class FormattingInfo : IFormattingInfo, ISelectorInfo
    {
        public FormattingInfo(FormatDetails formatDetails, Format format, object? currentValue)
            : this(null, formatDetails, format, currentValue)
        {
        }

        public FormattingInfo(FormattingInfo? parent, FormatDetails formatDetails, Format format, object? currentValue)
        {
            Parent = parent;
            CurrentValue = currentValue;
            Format = format;
            FormatDetails = formatDetails;
        }

        public FormattingInfo(FormattingInfo? parent, FormatDetails formatDetails, Placeholder placeholder,
            object? currentValue)
        {
            Parent = parent;
            FormatDetails = formatDetails;
            Placeholder = placeholder;
            Format = placeholder.Format;
            CurrentValue = currentValue;
        }

        public FormattingInfo? Parent { get; }

        public Selector? Selector { get; set; }

        public FormatDetails FormatDetails { get; }

        public object? CurrentValue { get; set; }

        public Placeholder? Placeholder { get; }
        public int Alignment => Placeholder?.Alignment ?? 0;
        public string? FormatterOptions => Placeholder?.FormatterOptions;

        public Format? Format { get; }

        public void Write(string text)
        {
            FormatDetails.Output.Write(text, this);
        }

        public void Write(string text, int startIndex, int length)
        {
            FormatDetails.Output.Write(text, startIndex, length, this);
        }

        public void Write(Format format, object value)
        {
            var nestedFormatInfo = CreateChild(format, value);
            FormatDetails.Formatter.Format(nestedFormatInfo);
        }


        public FormattingException FormattingException(string issue, FormatItem? problemItem = null, int startIndex = -1)
        {
            problemItem ??= Format;
            if (startIndex == -1) startIndex = problemItem?.startIndex ?? -1;
            return new FormattingException(problemItem, issue, startIndex);
        }

        public string? SelectorText => Selector?.RawText;
        public int SelectorIndex => Selector?.SelectorIndex ?? -1;
        public string? SelectorOperator => Selector?.Operator;

        public object? Result { get; set; }

        private FormattingInfo CreateChild(Format format, object currentValue)
        {
            return new FormattingInfo(this, FormatDetails, format, currentValue);
        }

        public FormattingInfo CreateChild(Placeholder placeholder)
        {
            return new FormattingInfo(this, FormatDetails, placeholder, CurrentValue);
        }
    }
}