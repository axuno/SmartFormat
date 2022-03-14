// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Formatting
{
    /// <summary>
    /// The class contains the fields and methods which are necessary for formatting.
    /// </summary>
    public class FormattingInfo : IFormattingInfo, ISelectorInfo
    {
        /// <summary>
        /// CTOR for object pooling.
        /// Immediately after creating the instance, an overload of 'Initialize' must be called.
        /// </summary>
        public FormattingInfo()
        {
            FormatDetails = InitializationObject.FormatDetails;
        }

        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="formatDetails"></param>
        /// <param name="format"></param>
        /// <param name="currentValue"></param>
        public FormattingInfo Initialize(FormatDetails formatDetails, Format format, object? currentValue)
        {
            return Initialize(null, formatDetails, format, currentValue);
        }

        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="formatDetails"></param>
        /// <param name="format">The <see cref="Parsing.Format"/> argument is used with <see cref="CreateChild(Parsing.Format,object?)"/></param>
        /// <param name="currentValue"></param>
        public FormattingInfo Initialize (FormattingInfo? parent, FormatDetails formatDetails, Format format, object? currentValue)
        {
            Parent = parent;
            CurrentValue = currentValue;
            FormatDetails = formatDetails;
            Format = format;
            // inherit alignment
            if (parent != null) Alignment = parent.Alignment;
            else if (format.ParentPlaceholder != null) Alignment = format.ParentPlaceholder.Alignment;

            return this;
        }

        /// <summary>
        /// Creates a new class instance, that contains fields and methods which are necessary for formatting.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="formatDetails"></param>
        /// <param name="placeholder"></param>
        /// <param name="currentValue"></param>
        public FormattingInfo Initialize (FormattingInfo? parent, FormatDetails formatDetails, Placeholder placeholder,
            object? currentValue)
        {
            Parent = parent;
            FormatDetails = formatDetails;
            Placeholder = placeholder;
            Format = placeholder.Format;
            CurrentValue = currentValue;
            // inherit alignment
            Alignment = placeholder.Alignment;

            return this;
        }

        /// <summary>
        /// Returns this instance and its <see cref="FormattingInfo"/> children to the object pool.
        /// This method gets called by <see cref="FormattingInfoPool"/> <see cref="PoolPolicy{T}.ActionOnReturn"/>.
        /// </summary>
        public void ReturnToPool()
        {
            Parent = null;
            // Assign new value, but leave existing references untouched
            FormatDetails = InitializationObject.FormatDetails;
            Placeholder = null;
            Selector = null;
            Alignment = 0;
            
            Format = null;
            CurrentValue = null;

            // Children can safely be returned
            foreach (var c in Children)
            {
                FormattingInfoPool.Instance.Return(c);
            }

            Children.Clear();
        }

        /// <summary>
        /// Gets the parent <see cref="FormattingInfo"/>.
        /// </summary>
        public FormattingInfo? Parent { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Parsing.Selector"/>.
        /// </summary>
        public Selector? Selector { get; internal set; }

        /// <summary>
        /// Gets the <see cref="FormatDetails"/>.
        /// </summary>
        public FormatDetails FormatDetails { get; private set; }

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
        public int Alignment { get; set; }

        /// <summary>
        /// Gets the <see cref="FormatterOptions"/> of a <see cref="Placeholder"/>.
        /// </summary>
        public string FormatterOptions => Placeholder?.FormatterOptions ?? string.Empty;

        /// <summary>
        /// Gets the <see cref="Format"/>.
        /// </summary>
        public Format? Format { get; private set; }

        /// <summary>
        /// Gets the list of child <see cref="FormattingInfo"/>s created by this instance.
        /// </summary>
        internal List<FormattingInfo> Children { get; } = new();

        /// <summary>
        /// Writes the <see cref="string"/> parameter to the <see cref="Output.IOutput"/>
        /// and takes care of alignment.
        /// </summary>
        /// <param name="text">The string to write to the <see cref="Output.IOutput"/></param>
        public void Write(string text)
        {
            if (Alignment > 0) PreAlign(text.Length);
            FormatDetails.Output.Write(text, this);
            if (Alignment < 0) PostAlign(text.Length);
        }

        /// <summary>
        /// Writes the <see cref="ReadOnlySpan{T}"/> text parameter to the <see cref="Output.IOutput"/>
        /// and takes care of alignment.
        /// </summary>
        /// <param name="text">The string to write to the <see cref="Output.IOutput"/></param>

        public void Write(ReadOnlySpan<char> text)
        {
            if (Alignment > 0) PreAlign(text.Length);
            FormatDetails.Output.Write(text, this);
            if (Alignment < 0) PostAlign(text.Length);
        }

        /// <summary>
        /// Creates a child <see cref="IFormattingInfo"/> from the current <see cref="IFormattingInfo"/> instance
        /// and invokes formatting with <see cref="SmartFormatter"/> and with the child as parameter.
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
        /// Gets the result after an <see cref="ISource"/> has assigned a value.
        /// </summary>
        public object? Result { get; set; }

        private FormattingInfo CreateChild(Format format, object? currentValue)
        {
            var fi = FormattingInfoPool.Instance.Get().Initialize(this, FormatDetails, format, currentValue);
            Children.Add(fi);
            return fi;
        }

        /// <summary>
        /// Creates a child <see cref="IFormattingInfo"/> from the current <see cref="IFormattingInfo"/> instance for a <see cref="Placeholder"/>.
        /// </summary>
        /// <param name="placeholder">The <see cref="Placeholder"/> used for creating a child <see cref="IFormattingInfo"/>.</param>
        /// <returns>The child <see cref="IFormattingInfo"/>.</returns>
        public FormattingInfo CreateChild(Placeholder placeholder)
        {
            var fi = FormattingInfoPool.Instance.Get().Initialize(this, FormatDetails, placeholder, CurrentValue);
            Children.Add(fi);
            return fi;
        }

        private void PreAlign(int textLength)
        {
            var filler = Alignment - textLength;
            if (filler > 0) FormatDetails.Output.Write(new string(FormatDetails.Settings.Formatter.AlignmentFillCharacter, filler), this);
        }

        private void PostAlign(int textLength)
        {
            var filler = -Alignment - textLength;
            if (filler > 0) FormatDetails.Output.Write(new string(FormatDetails.Settings.Formatter.AlignmentFillCharacter, filler), this);
        }
    }
}