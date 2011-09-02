namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents a single selector
    /// in the text in a <see cref="Placeholder"/>
    /// that comes before the colon.
    /// </summary>
    public class Selector : FormatItem
    {
        public Selector(string baseString, int startIndex, int endIndex, int operatorStart, int selectorIndex)
            : base(baseString, startIndex, endIndex)
        {
            this.SelectorIndex = selectorIndex;
            this.operatorStart = operatorStart;
        }
        /// <summary>
        /// The index of the selector in a multi-part selector.
        /// Example: {Person.Birthday.Year} has 3 seletors, 
        /// and Year has a SelectorIndex of 2.
        /// </summary>
        public int SelectorIndex { get; private set; }
        /// <summary>
        /// Keeps track of where the "operators" started for this item.
        /// </summary>
        internal readonly int operatorStart;

        public string Operator
        {
            get
            {
                return this.baseString.Substring(operatorStart, startIndex - operatorStart);
            }
        }
    }
}
