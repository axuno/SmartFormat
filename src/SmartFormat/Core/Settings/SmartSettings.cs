using System;
using System.Collections.Generic;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// <see cref="SmartFormat" /> settings to be applied for parsing and formatting.
    /// </summary>
    public class SmartSettings
    {
        internal SmartSettings()
        {
            CaseSensitivity = CaseSensitivityType.CaseSensitive;
            ConvertCharacterStringLiterals = true;
            FormatErrorAction = ErrorAction.Ignore;
            ParseErrorAction = ErrorAction.Ignore;
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormatter" />.
        /// </summary>
        public ErrorAction FormatErrorAction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormat.Core.Parsing.Parser" />.
        /// </summary>
        public ErrorAction ParseErrorAction { get; set; }

        /// <summary>
        /// Determines whether placeholders are case-sensitive or not.
        /// </summary>
        public CaseSensitivityType CaseSensitivity { get; set; }

        /// <summary>
        /// This setting is relevant for the <see cref="Parsing.LiteralText" />.
        /// If true (the default), character string literals are treated like in "normal" string.Format:
        /// string.Format("\t")   will return a "TAB" character
        /// If false, character string literals are not converted, just like with this string.Format:
        /// string.Format(@"\t")  will return the 2 characters "\" and "t"
        /// </summary>
        public bool ConvertCharacterStringLiterals { get; set; }

        internal IEqualityComparer<string> GetCaseSensitivityComparer()
        {
            {
                switch (CaseSensitivity)
                {
                    case CaseSensitivityType.CaseSensitive:
                        return StringComparer.CurrentCulture;
                    case CaseSensitivityType.CaseInsensitive:
                        return StringComparer.CurrentCultureIgnoreCase;
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
                        return StringComparison.CurrentCulture;
                    case CaseSensitivityType.CaseInsensitive:
                        return StringComparison.CurrentCultureIgnoreCase;
                    default:
                        throw new InvalidOperationException(
                            $"The case sensitivity type [{CaseSensitivity}] is unknown.");
                }
            }
        }
    }
}