//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Extensions;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// <see cref="SmartFormat" /> settings to be applied for parsing and formatting.
    /// <see cref="SmartSettings"/> are used to initialize instances.
    /// Properties should be considered as 'init-only' like implemented in C# 9.
    /// Any changes after passing settings as argument to CTORs may not have effect. 
    /// </summary>
    public class SmartSettings
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        public SmartSettings()
        {
        }

        /// <summary>
        /// Uses <c>string.Format</c>-compatible escaping of curly braces, {{ and }},
        /// instead of the <c>Smart.Format</c> default escaping, \{ and \}.
        /// Custom formatters cannot be parsed.
        /// </summary>
        public bool StringFormatCompatibility { get; set; } = false;

        /// <summary>
        /// Gets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormatter" />.
        /// The default is <see cref="ErrorAction.ThrowError"/>.
        /// </summary>
        [Obsolete("Use 'SmartSettings.Formatter.ErrorAction' instead.", false)]
        public ErrorAction FormatErrorAction
        {
            get => (ErrorAction) Formatter.ErrorAction;
            set => Formatter.ErrorAction = (FormatErrorAction) value;
        }

        /// <summary>
        /// Gets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormat.Core.Parsing.Parser" />.
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
            return CaseSensitivity == CaseSensitivityType.CaseSensitive
                ? StringComparer.Ordinal
                : StringComparer.OrdinalIgnoreCase;
        }

        internal StringComparison GetCaseSensitivityComparison()
        {
            return CaseSensitivity == CaseSensitivityType.CaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Gets the settings for the parser.
        /// Set only during initialization.
        /// </summary>
        public ParserSettings Parser { get; set; } = new();

        /// <summary>
        /// Gets the settings for the formatter.
        /// Set only during initialization.
        /// </summary>
        public FormatterSettings Formatter { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the <see cref="ILocalizationProvider"/> used for localizing strings.
        /// Defaults to <see langword="null"/>.
        /// </summary>
        public ILocalizationProvider? LocalizationProvider { get; set; }
    }
}