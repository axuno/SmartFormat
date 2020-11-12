using System;

namespace SmartFormat
{
    /// <summary>
    /// Supplies information about formatting errors.
    /// </summary>
    public class FormattingErrorEventArgs : EventArgs
    {
        internal FormattingErrorEventArgs(string rawText, int errorIndex, bool ignoreError)
        {
            Placeholder = rawText;
            ErrorIndex = errorIndex;
            IgnoreError = ignoreError;
        }

        /// <summary>
        /// Placeholder which caused an error.
        /// </summary>
        public string Placeholder { get; internal set; }

        /// <summary>
        /// Location where the error occurred.
        /// </summary>
        public int ErrorIndex { get; internal set; }

        /// <summary>
        /// Information whether error will throw an exception.
        /// </summary>
        public bool IgnoreError { get; internal set; }
    }
}