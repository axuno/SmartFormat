using System;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    /// <summary>
    /// Contains extra information about the item currently being formatted.
    /// These objects are not often used, so they are all wrapped up here.
    /// </summary>
    public class FormatDetails
    {
        public FormatDetails(SmartFormatter formatter, object[] originalArgs, FormatCache formatCache)
        {
            Formatter = formatter;
            OriginalArgs = originalArgs;
            FormatCache = formatCache;
        }
        /// <summary>
        /// The original formatter responsible for formatting this item.
        /// It can be used for evaluating nested formats.
        /// </summary>
        public SmartFormatter Formatter { get; internal set; }
        /// <summary>
        /// The original set of arguments passed to the format function.
        /// These provide global-access to the original arguments.
        /// </summary>
        public object[] OriginalArgs { get; internal set; }
        /// <summary>
        /// The placeholder that contains the item being formatted.
        /// Can be null.
        /// </summary>
        public Placeholder Placeholder { get; internal set; }
        /// <summary>
        /// This object can be used to cache resources between formatting calls.
        /// It will be null unless FormatWithCache is called.
        /// </summary>
        public FormatCache FormatCache { get; internal set; }

        /// <summary>
        /// If ErrorAction is set to OutputErrorsInResult, this contains the exception
        /// that was caused by either a parsing error or a formatting error.
        /// </summary>
        public FormatException FormatError { get; set; }
    }
}
