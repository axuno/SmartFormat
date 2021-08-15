//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Formatting
{
    /// <summary>
    /// The class contains the fields and methods which are necessary for formatting.
    /// </summary>
    public class FormattingInfo : IFormattingInfo, ISelectorInfo
    {
        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="formatDetails"></param>
        /// <param name="format"></param>
        /// <param name="currentValue"></param>
        public FormattingInfo(FormatDetails formatDetails, Format format, object? currentValue)
            : this(null, formatDetails, format, currentValue)
        {
        }

        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="formatDetails"></param>
        /// <param name="format">The <see cref="Parsing.Format"/> argument is used with <see cref="CreateChild(Parsing.Format,object?)"/></param>
        /// <param name="currentValue"></param>
        public FormattingInfo(FormattingInfo? parent, FormatDetails formatDetails, Format format, object? currentValue)
        {
            Parent = parent;
            CurrentValue = currentValue;
            FormatDetails = formatDetails;
            Format = format;
        }

        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="formatDetails"></param>
        /// <param name="placeholder"></param>
        /// <param name="currentValue"></param>
        public FormattingInfo(FormattingInfo? parent, FormatDetails formatDetails, Placeholder placeholder,
            object? currentValue)
        {
            Parent = parent;
            FormatDetails = formatDetails;
            Placeholder = placeholder;
            Format = placeholder.Format;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Gets the parent <see cref="FormattingInfo"/>.
        /// </summary>
        public FormattingInfo? Parent { get; }

        /// <summary>
        /// Gets or sets the <see cref="Parsing.Selector"/>.
        /// </summary>
        public Selector? Selector { get; set; }

        /// <summary>
        /// Gets the <see cref="FormatDetails"/>.
        /// </summary>
        public FormatDetails FormatDetails { get; }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public object? CurrentValue { get; set; }

        /// <summary>
        /// Gets the <see cref="Placeholder"/>.
        /// </summary>
        public Placeholder? Placeholder { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Alignment"/> of the current <see cref="Placeholder"/>,
        /// or - if this is <see langword="null"/> - the <see cref="Alignment"/>
        /// of any parent <see cref="IFormattingInfo"/> that is not zero.
        /// </summary>
        public int Alignment
        {
            get
            {
                if (Placeholder?.Alignment != null && Placeholder.Alignment != 0) return Placeholder.Alignment;

                var parentFormatInfo = this;
                var alignment = 0;
                // Find a parent FormattingInfo which has the Alignment set different from zero
                while (parentFormatInfo?.Parent?.Alignment != null)
                {
                    if(parentFormatInfo.Parent.Alignment != 0) alignment = parentFormatInfo.Parent.Alignment;
                    parentFormatInfo = parentFormatInfo.Parent;
                }

                return alignment;
            }
        }

        /// <summary>
        /// Gets the <see cref="FormatterOptions"/> of a <see cref="Placeholder"/>.
        /// </summary>
        public string? FormatterOptions => Placeholder?.FormatterOptions;

        /// <summary>
        /// Gets the <see cref="Format"/>.
        /// </summary>
        public Format? Format { get; }

        /// <summary>
        /// Writes the string parameter to the <see cref="Output.IOutput"/>
        /// and takes care of alignment.
        /// </summary>
        /// <param name="text">The string to write to the <see cref="Output.IOutput"/></param>
        public void Write(string text)
        {
            if (Alignment > 0) PreAlign(text.Length);
            FormatDetails.Output.Write(text);
            if (Alignment < 0) PostAlign(text.Length);
        }

        /// <summary>
        /// Creates a child <see cref="IFormattingInfo"/> from the current <see cref="IFormattingInfo"/> instance
        /// and invokes formatting with <see cref="SmartFormatter"/> with the child as parameter.
        /// </summary>
        /// <param name="format">The <see cref="Format"/> to use.</param>
        /// <param name="value">The value for the item in the format.</param>
        public void FormatAsChild(Format format, object? value)
        {
            var nestedFormatInfo = CreateChild(format, value);
            // recursive method call
            FormatDetails.Formatter.Format(nestedFormatInfo);
        }
        
        /// <summary>
        /// Creates a new <see cref="FormattingException"/>.
        /// </summary>
        /// <param name="issue">The text which goes to the <see cref="Exception.Message"/>.</param>
        /// <param name="problemItem">The <see cref="FormatItem"/> which caused the problem.</param>
        /// <param name="startIndex">The start index in the input format string.</param>
        /// <returns></returns>
        public FormattingException FormattingException(string issue, FormatItem? problemItem = null, int startIndex = -1)
        {
            problemItem ??= Format;
            if (startIndex == -1) startIndex = problemItem?.StartIndex ?? -1;
            return new FormattingException(problemItem, issue, startIndex);
        }

        /// <summary>
        /// Gets the (raw) text of the <see cref="Parsing.Selector"/>.
        /// </summary>
        public string SelectorText => Selector?.RawText ?? string.Empty;
        
        /// <summary>
        /// Gets index of the <see cref="Parsing.Selector"/> in the selector list.
        /// </summary>
        public int SelectorIndex => Selector?.SelectorIndex ?? -1;
        
        /// <summary>
        /// Gets the operator string of the <see cref="Parsing.Selector"/> (e.g.: comma, dot).
        /// </summary>
        public string SelectorOperator => Selector?.Operator ?? string.Empty;

        /// <summary>
        /// Gets the result after formatting is completed.
        /// </summary>
        public object? Result { get; set; }

        private FormattingInfo CreateChild(Format format, object? currentValue)
        {
            return new FormattingInfo(this, FormatDetails, format, currentValue);
        }

        /// <summary>
        /// Creates a child <see cref="IFormattingInfo"/> from the current <see cref="IFormattingInfo"/> instance for a <see cref="Placeholder"/>.
        /// </summary>
        /// <param name="placeholder">The <see cref="Placeholder"/> used for creating a child <see cref="IFormattingInfo"/>.</param>
        /// <returns>The child <see cref="IFormattingInfo"/>.</returns>
        public FormattingInfo CreateChild(Placeholder placeholder)
        {
            return new FormattingInfo(this, FormatDetails, placeholder, CurrentValue);
        }

        private void PreAlign(int textLength)
        {
            var filler = Alignment - textLength;
            if (filler > 0) FormatDetails.Output.Write(new string(FormatDetails.Settings.Formatter.AlignmentFillCharacter, filler));
        }

        private void PostAlign(int textLength)
        {
            var filler = -Alignment - textLength;
            if (filler > 0) FormatDetails.Output.Write(new string(FormatDetails.Settings.Formatter.AlignmentFillCharacter, filler));
        }

        /// <summary>
        /// Creates a new instance of <see cref="FormattingInfo"/>.
        /// </summary>
        /// <param name="format">The input format string.</param>
        /// <param name="data">The data argument.</param>
        /// <returns>A new instance of <see cref="FormattingInfo"/>.</returns>
        internal static FormattingInfo Create(string format, IList<object> data)
        {
            var formatter = new SmartFormatter(new SmartSettings());
            var formatParsed = formatter.Parser.ParseFormat(format);
            var formatDetails = new FormatDetails(formatter, formatParsed, data, null, new StringOutput());
            return new FormattingInfo(formatDetails, formatDetails.OriginalFormat, data);
        }
    }
}