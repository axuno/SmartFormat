//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// A placeholder is the part of a format string between the {braces}.
    /// </summary>
    /// <example>
    /// For example, in "{Items.Length,-10:choose(1|2|3):one|two|three}",
    /// the <see cref="Alignment" />s is "-10",
    /// the <see cref="Selector" />s are "Items" and "Length", separated by the dot "Operator".
    /// the <see cref="FormatterName" /> is "choose",
    /// the <see cref="FormatterOptionsRaw" /> is "1|2|3",
    /// and the <see cref="Format" /> is "one|two|three".
    /// </example>
    public class Placeholder : FormatItem
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="parent">The parent <see cref="Format"/> of the placeholder</param>
        /// <param name="startIndex">The index inside the input string, where the placeholder starts.</param>
        /// <param name="nestedDepth">The nesting level of this placeholder.</param>
        public Placeholder(Format parent, int startIndex, int nestedDepth) : base(
            parent.SmartSettings, parent.BaseString, startIndex, parent.EndIndex)
        {
            Parent = parent;
            NestedDepth = nestedDepth;
            FormatterNameStartIndex = startIndex;
            FormatterNameLength = 0;
            FormatterOptionsStartIndex = startIndex;
            FormatterOptionsLength = 0;
        }

        /// <summary>
        /// Gets the parent <see cref="Parsing.Format"/>.
        /// </summary>
        public Format Parent { get; }

        /// <summary>
        /// Gets the parent <see cref="Parsing.Format"/>.
        /// </summary>
        [Obsolete("Use property 'Parent' instead")]
        public Format parent => Parent;

        /// <summary>
        /// Gets or sets the nesting level the <see cref="Placeholder"/>.
        /// </summary>
        public int NestedDepth { get; set; }

        /// <summary>
        /// Gets a list of all <see cref="Selector"/> within the <see cref="Placeholder"/>.
        /// </summary>
        public List<Selector> Selectors { get; } = new();

        /// <summary>
        /// Gets or sets the <see cref="Alignment"/> of the result string,
        /// used like with string.Format("{0,-10}"), where -10 is the alignment.
        /// </summary>
        public int Alignment { get; internal set; }
        
        /// <summary>
        /// Gets or sets the start index of the <see cref="FormatterName"/> within the <see cref="FormatItem.BaseString"/>
        /// </summary>
        internal int FormatterNameStartIndex { get; set; }

        /// <summary>
        /// Gets or sets the length of the <see cref="FormatterName"/> within the <see cref="FormatItem.BaseString"/>
        /// </summary>
        internal int FormatterNameLength { get; set; }

        /// <summary>
        /// Gets or sets the start index of the <see cref="FormatterOptions"/> within the <see cref="FormatItem.BaseString"/>
        /// </summary>
        internal int FormatterOptionsStartIndex { get; set; }

        /// <summary>
        /// Gets or sets the length of the <see cref="FormatterOptions"/> within the <see cref="FormatItem.BaseString"/>
        /// </summary>
        internal int FormatterOptionsLength { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the formatter.
        /// </summary>
        public string FormatterName => BaseString.Substring(FormatterNameStartIndex, FormatterNameLength);

        /// <summary>
        /// Gets the formatter option string unescaped.
        /// To get the raw formatter option string, <see cref="FormatterOptionsRaw"/>.
        /// </summary>
        public string FormatterOptions
        {
            get
            {
                // It's enough to have a buffer with the same size as input length
                var resultBuffer = new Span<char>(new char[Length]);
                return EscapedLiteral
                    .UnEscapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar, BaseString.AsSpan(FormatterOptionsStartIndex, FormatterOptionsLength), 0,
                        FormatterOptionsRaw.Length, true, resultBuffer).ToString();
            }
        }

        /// <summary>
        /// Gets the raw formatter option string as in the input format string (unescaped).
        /// </summary>
        public string FormatterOptionsRaw => BaseString.Substring(FormatterOptionsStartIndex, FormatterOptionsLength);
        
        /// <summary>
        /// Gets or sets the <see cref="Format"/> of the <see cref="Placeholder"/>.
        /// </summary>
        public Format? Format { get; set; }

        /// <summary>
        /// Gets the string representation of the <see cref="Placeholder"/> with all parsed components.
        /// </summary>
        /// <returns>The string representation of the <see cref="Placeholder"/> with all parsed components.</returns>
        public override string ToString()
        {
            var result = new StringBuilder(EndIndex - StartIndex);
            result.Append(SmartSettings.Parser.PlaceholderBeginChar);
            foreach (var s in Selectors) result.Append(s.BaseString, s.OperatorStartIndex, s.EndIndex - s.OperatorStartIndex);
            if (Alignment != 0)
            {
                result.Append(SmartSettings.Parser.AlignmentOperator);
                result.Append(Alignment);
            }

            if (FormatterName != string.Empty)
            {
                result.Append(SmartSettings.Parser.FormatterNameSeparator);
                result.Append(FormatterName);
                if (FormatterOptions != string.Empty)
                {
                    result.Append(SmartSettings.Parser.FormatterOptionsBeginChar);
                    result.Append(FormatterOptions);
                    result.Append(SmartSettings.Parser.FormatterOptionsEndChar);
                }
            }

            if (Format != null)
            {
                result.Append(SmartSettings.Parser.FormatterNameSeparator);
                result.Append(Format);
            }

            result.Append(SmartSettings.Parser.PlaceholderEndChar);

            return result.ToString();
        }
    }
}