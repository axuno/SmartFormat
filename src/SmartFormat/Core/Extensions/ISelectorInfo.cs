//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.ComponentModel;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// Contains all the necessary information for evaluating a selector.
    /// </summary>
    /// <example>
    /// When evaluating "{Items.Length}",
    /// the CurrentValue might be Items, and the Selector would be "Length".
    /// The job of an ISource is to set CurrentValue to Items.Length.
    /// </example>
    public interface ISelectorInfo
    {
        /// <summary>
        /// The current value to evaluate.
        /// </summary>
        object? CurrentValue { get; }

        /// <summary>
        /// The selector to evaluate
        /// </summary>
        string? SelectorText { get; }

        /// <summary>
        /// The index of the selector in a multi-part selector.
        /// Example: {Person.Birthday.Year} has 3 seletors,
        /// and Year has a SelectorIndex of 2.
        /// </summary>
        int SelectorIndex { get; }

        /// <summary>
        /// The operator that came before the selector; typically "."
        /// </summary>
        string? SelectorOperator { get; }

        /// <summary>
        /// Sets the result of evaluating the selector.
        /// </summary>
        object? Result { set; }

        /// <summary>
        /// Contains all the details about the current placeholder.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Placeholder? Placeholder { get; }

        /// <summary>
        /// Infrequently used details, often used for debugging
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FormatDetails FormatDetails { get; }
    }
}