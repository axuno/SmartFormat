//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Creates a new instance of <see cref="FormatDetails"/>
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="originalFormat">The the original <see cref="Format"/> returned by the <see cref="Parser"/>.</param>
        /// <param name="originalArgs">The original set of arguments passed to the format method.</param>
        /// <param name="provider">The <see cref="IFormatProvider"/> that can be used to determine how to format items.</param>
        /// <param name="output">The <see cref="IOutput"/> where the result is written.</param>
        public FormatDetails(SmartFormatter formatter, Format originalFormat, IList<object?> originalArgs,
            IFormatProvider? provider, IOutput output)
        {
            Formatter = formatter;
            OriginalFormat = originalFormat;
            OriginalArgs = originalArgs;
            Provider = provider;
            Output = output;
        }

        /// <summary>
        /// The original formatter responsible for formatting this item.
        /// It can be used for evaluating nested formats.
        /// </summary>
        public SmartFormatter Formatter { get; }

        /// <summary>
        /// Gets the <see cref="Format"/> returned by the <see cref="Parser"/>.
        /// </summary>
        public Format OriginalFormat { get; }

        /// <summary>
        /// The original set of arguments passed to the format method.
        /// These provide global-access to the original arguments.
        /// </summary>
        public IList<object?> OriginalArgs { get; }

        /// <summary>
        /// The <see cref="IFormatProvider"/> that can be used to determine how to
        /// format items such as numbers, dates, and anything else that
        /// might be culture-specific.
        /// </summary>
        public IFormatProvider? Provider { get; }

        /// <summary>
        /// Gets the <see cref="IOutput"/> where the result is written.
        /// </summary>
        public IOutput Output { get; }

        /// <summary>
        /// If ErrorAction is set to OutputErrorsInResult, this will
        /// contain the exception that caused the formatting error.
        /// </summary>
        public FormattingException? FormattingException { get; set; }

        /// <summary>
        /// Contains case-sensitivity and other settings.
        /// </summary>
        public SmartSettings Settings => Formatter.Settings;
    }
}