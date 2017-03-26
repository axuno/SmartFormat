namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Represents the literal text that is found
    /// in a parsed format string.
    /// </summary>
    public class LiteralText : FormatItem
    {
        public LiteralText(Format parent, int startIndex) : base(parent, startIndex)
        { }
        public LiteralText(Format parent) : base(parent, parent.startIndex)
        { }

        public override string ToString()
        {
            return this.baseString.Substring(startIndex, endIndex - startIndex);
        }
    }
}
