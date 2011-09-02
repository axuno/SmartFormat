namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Base class that represents a substring 
    /// of text from a parsed format string.
    /// </summary>
    public abstract class FormatItem
    {
        protected FormatItem(FormatItem parent, int startIndex) : this(parent.baseString, startIndex, parent.baseString.Length)
        { }

        protected FormatItem(string baseString, int startIndex, int endIndex)
        {
            this.baseString = baseString;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        /// <summary>
        /// Retrieves the substring that this item represents.
        /// </summary>
        public string Text
        {
            get
            {
                return this.baseString.Substring(startIndex, endIndex - startIndex);
            }
        }


        public readonly string baseString;
        public int startIndex;
        public int endIndex;

        public override string ToString()
        {
            if (endIndex <= startIndex)
                return string.Format("Empty ({0})", baseString.Substring(startIndex));
            return string.Format("{0}", baseString.Substring(startIndex, endIndex - startIndex));
        }
    }
}
