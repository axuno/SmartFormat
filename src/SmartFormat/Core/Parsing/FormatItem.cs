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
        public int endIndex;
        protected SmartSettings SmartSettings;
        public int startIndex;

        protected FormatItem(SmartSettings smartSettings, FormatItem parent, int startIndex) : this(smartSettings,
            parent.baseString, startIndex, parent.baseString.Length)
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