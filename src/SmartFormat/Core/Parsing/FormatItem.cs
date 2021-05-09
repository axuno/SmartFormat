//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Base class that represents a substring
    /// of text from a parsed format string.
    /// </summary>
    public abstract class FormatItem
    {
        public readonly string baseString;
        
        /// <summary>
        /// The end index is pointing to ONE POSITION AFTER the last character of item.
        ///  </summary>
        /// <example>
        /// Format string: {0}{1}ABC
        /// Index:         012345678
        /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
        /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
        /// </example>
        public int endIndex;
        
        /// <summary>
        /// The start index is pointing to the first character of item.
        ///  </summary>
        /// <example>
        /// Format string: {0}{1}ABC
        /// Index:         012345678
        /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
        /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
        /// </example>
        public int startIndex;

        /// <summary>
        /// The settings for formatter and parser.
        /// </summary>
        protected SmartSettings SmartSettings;

        protected FormatItem(SmartSettings smartSettings, FormatItem parent, int startIndex) : this(smartSettings,
            parent.baseString, startIndex, parent.baseString.Length)
        {
        }

        protected FormatItem(SmartSettings smartSettings, FormatItem parent, int startIndex, int endIndex) : this(smartSettings,
            parent.baseString, startIndex, endIndex)
        {
        }

        protected FormatItem(SmartSettings smartSettings, string baseString, int startIndex, int endIndex)
        {
            SmartSettings = smartSettings;
            this.baseString = baseString;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        /// <summary>
        /// Retrieves the raw text that this item represents.
        /// </summary>
        public string RawText => baseString.Substring(startIndex, endIndex - startIndex);

        public override string ToString()
        {
            return endIndex <= startIndex
                ? $"Empty ({baseString.Substring(startIndex)})"
                : $"{baseString.Substring(startIndex, endIndex - startIndex)}";
        }
    }
}