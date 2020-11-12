using System;

namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Supplies information about parsing errors.
    /// </summary>
    public class ParsingErrorEventArgs : EventArgs
    {
        internal ParsingErrorEventArgs(ParsingErrors errors, bool throwsException)
        {
            Errors = errors;
            ThrowsException = throwsException;
        }

        /// <summary>
        /// All parsing errors which occurred during parsing.
        /// </summary>
        public ParsingErrors Errors { get; internal set; }

        /// <summary>
        /// If true, the errors will throw an exception.
        /// </summary>
        public bool ThrowsException { get; internal set; }
    }
}