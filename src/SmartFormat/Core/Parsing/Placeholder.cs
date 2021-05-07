//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;
using SmartFormat.Core.Settings;

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
        /// <param name="smartSettings">The Smart.Format <see cref="Settings"/></param>
        /// <param name="parent">The parent <see cref="Format"/> of the placeholder</param>
        /// <param name="startIndex">The index inside the input string, where the placeholder starts.</param>
        /// <param name="nestedDepth">The nesting level of this placeholder.</param>
        public Placeholder(SmartSettings smartSettings, Format parent, int startIndex, int nestedDepth) : base(
            smartSettings, parent, startIndex)
        {
            this.parent = parent;
            Selectors = new List<Selector>();
            NestedDepth = nestedDepth;
            FormatterName = "";
            FormatterOptionsRaw = "";
        }

        /// <summary>
        /// Gets the parent <see cref="Parsing.Format"/>.
        /// </summary>
        public Format parent { get; }

        /// <summary>
        /// Gets or sets the nesting level the <see cref="Placeholder"/>.
        /// </summary>
        public int NestedDepth { get; set; }

        /// <summary>
        /// Gets a list of all <see cref="Selector"/> within the <see cref="Placeholder"/>.
        /// </summary>
        public List<Selector> Selectors { get; }
        
        /// <summary>
        /// Gets or sets the <see cref="Alignment"/> of the result string,
        /// used like with string.Format("{0,-10}"), where -10 is the alignment.
        /// </summary>
        public int Alignment { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the formatter.
        /// </summary>
        public string FormatterName { get; set; }

        /// <summary>
        /// Gets the formatter option string unescaped.
        /// To get the raw formatter option string, <see cref="FormatterOptionsRaw"/>.
        /// </summary>
        public string FormatterOptions => EscapedLiteral
            .UnescapeCharLiterals(SmartSettings.Parser.CharLiteralEscapeChar, FormatterOptionsRaw.AsSpan(), true).ToString();
        
        /// <summary>
        /// Gets the raw formatter option string as in the input format string (unescaped).
        /// </summary>
        public string FormatterOptionsRaw { get; internal set; }
        
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
            var result = new StringBuilder(endIndex - startIndex);
            result.Append(SmartSettings.Parser.PlaceholderBeginChar);
            foreach (var s in Selectors) result.Append(s.baseString, s.operatorStart, s.endIndex - s.operatorStart);
            if (Alignment != 0)
            {
                result.Append(SmartSettings.Parser.AlignmentOperator);
                result.Append(Alignment);
            }

            if (FormatterName != "")
            {
                result.Append(SmartSettings.Parser.FormatterNameSeparator);
                result.Append(FormatterName);
                if (FormatterOptions != "")
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