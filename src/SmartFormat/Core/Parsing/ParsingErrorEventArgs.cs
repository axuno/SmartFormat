namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Supplies information about parsing errors.
    /// </summary>
    public class ParsingErrorEventArgs : System.EventArgs
    {
        internal ParsingErrorEventArgs(string rawText, int startIndex, int endIndex, Parser.ParsingError parsingError, bool ignoreError)
        {
            RawText = rawText;
            StartIndex = startIndex;
            EndIndex = endIndex;
            ParsingError = parsingError;
            IgnoreError = ignoreError;
        }

        /// <summary>
        /// Raw ext part which caused an error.
        /// </summary>
        public string RawText { get; internal set; }

        /// <summary>
        /// Location where the error started.
        /// </summary>
        public int StartIndex { get; internal set; }

        /// <summary>
        /// Location where the error ended.
        /// </summary>
        public int EndIndex { get; internal set; }

        /// <summary>
        /// ParseError category.
        /// </summary>
        public Parser.ParsingError ParsingError { get; internal set; }

        /// <summary>
        /// Information whether error will throw an exception.
        /// </summary>
        public bool IgnoreError { get; internal set; }

        /// <summary>
        /// Gets the string representation of the ParsingError enum.
        /// </summary>
        /// <returns>The string representation of the ParsingError enum.</returns>
        public string GetParsingErrorText()
        {
            return new Parser.ParsingErrorText()[ParsingError];
        }
    }
}
