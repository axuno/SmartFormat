using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    /// <summary>
    /// Contains extra information about the item currently being formatted.
    /// These objects are not often used.
    /// </summary>
    public class FormatDetails
    {
        public SmartFormatter Formatter { get; internal set; }
        public object[] OriginalArgs { get; internal set; }
        public Placeholder Placeholder { get; internal set; }
    }
}
