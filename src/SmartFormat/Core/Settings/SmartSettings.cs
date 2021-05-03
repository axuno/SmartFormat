//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// <see cref="SmartFormat" /> settings to be applied for parsing and formatting.
    /// </summary>
    public class SmartSettings
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        internal SmartSettings()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormatter" />.
        /// The default is <see cref="ErrorAction.ThrowError"/>.
        /// </summary>
        [Obsolete("Use 'SmartSettings.Formatter.ErrorAction' instead.", false)]
        public ErrorAction FormatErrorAction
        {
            get => (ErrorAction) Formatter.ErrorAction;
            set => Formatter.ErrorAction = (FormatErrorAction) value;
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormat.Core.Parsing.Parser" />.
        /// The default is <see cref="ErrorAction.ThrowError"/>.
        /// </summary>
        [Obsolete("Use 'SmartSettings.Parser.ErrorAction' instead.", false)]
        public ErrorAction ParseErrorAction
        {
            get => (ErrorAction) Parser.ErrorAction;
            set => Parser.ErrorAction = (ParseErrorAction) value;
        }

        /// <summary>
        /// Determines whether placeholders are case-sensitive or not.
        /// The default is <see cref="CaseSensitivityType.CaseSensitive"/>.
        /// </summary>
        public CaseSensitivityType CaseSensitivity { get; set; } = CaseSensitivityType.CaseSensitive;

        /// <summary>
        /// This setting is relevant for the <see cref="Parsing.LiteralText" />.
        /// If true (the default), character string literals are treated like in "normal" string.Format:
        /// string.Format("\t")   will return a "TAB" character
        /// If false, character string literals are not converted, just like with this string.Format:
        /// string.Format(@"\t")  will return the 2 characters "\" and "t"
        /// </summary>
        [Obsolete("Use SmartSettings.Parser.ConvertCharacterStringLiterals instead", false)]
        public bool ConvertCharacterStringLiterals
        {
            get => Parser.ConvertCharacterStringLiterals;
            set => Parser.ConvertCharacterStringLiterals = value;
        }

        internal IEqualityComparer<string> GetCaseSensitivityComparer()
        {
            {
                switch (CaseSensitivity)
                {
                    case CaseSensitivityType.CaseSensitive:
                        return StringComparer.Ordinal;
                    case CaseSensitivityType.CaseInsensitive:
                        return StringComparer.OrdinalIgnoreCase;
                    default:
                        throw new InvalidOperationException(
                            $"The case sensitivity type [{CaseSensitivity}] is unknown.");
                }
            }
        }

        internal StringComparison GetCaseSensitivityComparison()
        {
            {
                switch (CaseSensitivity)
                {
                    case CaseSensitivityType.CaseSensitive:
                        return StringComparison.Ordinal;
                    case CaseSensitivityType.CaseInsensitive:
                        return StringComparison.OrdinalIgnoreCase;
                    default:
                        throw new InvalidOperationException(
                            $"The case sensitivity type [{CaseSensitivity}] is unknown.");
                }
            }
        }

        /// <summary>
        /// Gets the settings for the parser.
        /// </summary>
        public ParserSettings Parser { get; } = new();

        /// <summary>
        /// Gets the settings for the formatter.
        /// </summary>
        public FormatterSettings Formatter { get; } = new();
    }
}