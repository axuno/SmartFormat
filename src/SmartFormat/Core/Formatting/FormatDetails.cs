// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Core.Formatting;

/// <summary>
/// Contains extra information about the item currently being formatted.
/// These objects are not often used, so they are all wrapped up here.
/// </summary>
public class FormatDetails
{
    #region: Create, initialize, return to pool :

    /// <summary>
    /// CTOR for object pooling.
    /// Immediately after creating the instance, an overload of 'Initialize' must be called.
    /// </summary>
    public FormatDetails()
    {
        // Initialize members which must not be null
        // Will all be overwritten by Initialize(...) methods
        Formatter = InitializationObject.SmartFormatter;
        OriginalFormat = InitializationObject.Format;
        OriginalArgs = InitializationObject.ObjectList;
        Output = InitializationObject.Output;
        Provider = null;
        FormattingException = null;
    }

    /// <summary>
    /// Initializes the <see cref="FormatDetails"/> instance.
    /// </summary>
    /// <param name="formatter"></param>
    /// <param name="originalFormat"></param>
    /// <param name="originalArgs"></param>
    /// <param name="provider"></param>
    /// <param name="output"></param>
    /// <returns>This <see cref="FormatDetails"/> instance.</returns>
    public FormatDetails Initialize(SmartFormatter formatter, Format originalFormat, IList<object?> originalArgs,
        IFormatProvider? provider, IOutput output)
    {
        Formatter = formatter;
        OriginalFormat = originalFormat;
        OriginalArgs = originalArgs;
        Provider = provider;
        Output = output;
        FormattingException = null;

        return this;
    }

    #endregion

    /// <summary>
    /// The original formatter responsible for formatting this item.
    /// It can be used for evaluating nested formats.
    /// </summary>
    public SmartFormatter Formatter { get; private set; }

    /// <summary>
    /// Gets the <see cref="Format"/> returned by the <see cref="Parser"/>.
    /// </summary>
    public Format OriginalFormat { get; private set; }

    /// <summary>
    /// The original set of arguments passed to the format method.
    /// These provide global-access to the original arguments.
    /// </summary>
    public IList<object?> OriginalArgs { get; private set; }

    /// <summary>
    /// The <see cref="IFormatProvider"/> that can be used to determine how to
    /// format items such as numbers, dates, and anything else that
    /// might be culture-specific.
    /// </summary>
    public IFormatProvider? Provider { get; internal set; }

    /// <summary>
    /// Gets the <see cref="IOutput"/> where the result is written.
    /// </summary>
    public IOutput Output { get; private set; }

    /// <summary>
    /// If ErrorAction is set to OutputErrorsInResult, this will
    /// contain the exception that caused the formatting error.
    /// </summary>
    public FormattingException? FormattingException { get; set; }

    /// <summary>
    /// Contains case-sensitivity and other settings.
    /// </summary>
    public SmartSettings Settings => Formatter.Settings;

    /// <summary>
    /// Clears all internal objects.
    /// </summary>
    internal void Clear()
    {
        Formatter = InitializationObject.SmartFormatter;
        OriginalFormat = InitializationObject.Format;
        OriginalArgs = InitializationObject.ObjectList;
        Output = InitializationObject.Output;
        Provider = null;
        FormattingException = null;
    }
}