using System;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Formatting
{
    /// <summary>
    /// Contains extra information about the item currently being formatted.
    /// These objects are not often used, so they are all wrapped up here.
    /// </summary>
    public class FormatDetails
    {
        public FormatDetails(SmartFormatter formatter, Format originalFormat, object[] originalArgs,
            FormatCache formatCache, IFormatProvider provider, IOutput output)
        {
            Formatter = formatter;
            OriginalFormat = originalFormat;
            OriginalArgs = originalArgs;
            FormatCache = formatCache;
            Provider = provider;
            Output = output;
        }

        /// <summary>
        /// The original formatter responsible for formatting this item.
        /// It can be used for evaluating nested formats.
        /// </summary>
        public SmartFormatter Formatter { get; }

        /// <summary>
        /// Gets the original <see cref="Format"/> returned by the parser.
        /// </summary>
        public Format OriginalFormat { get; }

        /// <summary>
        /// The original set of arguments passed to the format function.
        /// These provide global-access to the original arguments.
        /// </summary>
        public object[] OriginalArgs { get; }

        /// <summary>
        /// This object can be used to cache resources between formatting calls.
        /// It will be null unless FormatWithCache is called.
        /// </summary>
        public FormatCache FormatCache { get; }

        /// <summary>
        /// The Format Provider that can be used to determine how to
        /// format items such as numbers, dates, and anything else that
        /// might be culture-specific.
        /// </summary>
        public IFormatProvider Provider { get; }

        /// <summary>
        /// Gets the <see cref="IOutput"/> where the result is written.
        /// </summary>
        public IOutput Output { get; }

        /// <summary>
        /// If ErrorAction is set to OutputErrorsInResult, this will
        /// contain the exception that caused the formatting error.
        /// </summary>
        public FormattingException FormattingException { get; set; }

        /// <summary>
        /// Contains case-sensitivity settings
        /// </summary>
        public SmartSettings Settings => Formatter.Settings;
    }
}