//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Base class that represents a substring
    /// of text from a parsed format string.
    /// </summary>
    public abstract class FormatItem
    {
        /// <summary>
        /// Obsolete. Gets the base format string.
        /// </summary>
        [Obsolete("Use property 'BaseString' instead")]
        public string baseString => BaseString;

        /// <summary>
        /// Gets the base format string.
        /// </summary>
        public string BaseString { get; }
        
        /// <summary>
        /// Obsolete. The end index is pointing to ONE POSITION AFTER the last character of item.
        /// </summary>
        [Obsolete("Use property 'EndIndex' instead")]
        public int endIndex
        {
            get => EndIndex;
            set => EndIndex = value;
        }

        /// <summary>
        /// The end index is pointing to ONE POSITION AFTER the last character of item.
        /// </summary>
        /// <example>
        /// Format string: {0}{1}ABC
        /// Index:         012345678
        /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
        /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
        /// </example>
        public int EndIndex { get; set; }
        
        /// <summary>
        /// Obsolete. The start index is pointing to the first character of item.
        /// </summary>
        [Obsolete("Use property 'StartIndex' instead")]
        public int startIndex
        {
            get => StartIndex;
            set => StartIndex = value;
        }

        /// <summary>
        /// The start index is pointing to the first character of item.
        /// </summary>
        /// <example>
        /// Format string: {0}{1}ABC
        /// Index:         012345678
        /// Start index for 1st placeholder is 0, for the second it's 3, for the literal it's 6.
        /// End index for the 1st placeholder is 3, for the second it's 6, for the literal it's 9.
        /// </example>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets the result of <see cref="EndIndex"/> minus <see cref="StartIndex"/>.
        /// </summary>
        public int Length => EndIndex - StartIndex;

        /// <summary>
        /// The settings for formatter and parser.
        /// </summary>
        protected SmartSettings SmartSettings;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="smartSettings"></param>
        /// <param name="baseString">The base format string.</param>
        /// <param name="startIndex">The start index of the <see cref="FormatItem"/> within the base format string.</param>
        /// <param name="endIndex">The end index of the <see cref="FormatItem"/> within the base format string.</param>
        protected FormatItem(SmartSettings smartSettings, string baseString, int startIndex, int endIndex)
        {
            SmartSettings = smartSettings;
            BaseString = baseString;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        /// <summary>
        /// Retrieves the raw text that this item represents.
        /// </summary>
        public string RawText => BaseString.Substring(StartIndex, Length);

        /// <summary>
        /// Gets the string representation of this <see cref="FormatItem"/>.
        /// </summary>
        /// <returns>The string representation of this <see cref="FormatItem"/></returns>
        public override string ToString()
        {
            return EndIndex <= StartIndex
                ? $"Empty ({BaseString.Substring(StartIndex)})"
                : $"{BaseString.Substring(StartIndex, Length)}";
        }
    }
}