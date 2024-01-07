// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions;

/// <summary>
/// A class to output literals depending on the value of the input variable.
/// </summary>
public class ChooseFormatter : IFormatter
{
    private CultureInfo? _cultureInfo;
    private char _splitChar = '|';

    /// <summary>
    /// Gets or sets the character used to split the option text literals.
    /// Valid characters are: | (pipe) , (comma)  ~ (tilde)
    /// </summary>
    public char SplitChar
    {
        get => _splitChar;
        set => _splitChar = Utilities.Validation.GetValidSplitCharOrThrow(value);
    }

    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    public string[] Names { get; set; } = {"choose", "c"};

    ///<inheritdoc/>
    public string Name { get; set; } = "choose";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = false;

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var chooseOptions = formattingInfo.FormatterOptions.Split(SplitChar);
        var formats = formattingInfo.Format?.Split(SplitChar);
            
        // Check whether arguments can be handled by this formatter
        if (formats is null || formats.Count < 2)
        {
            // Auto detection calls just return a failure to evaluate
            if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                return false;

            // throw, if the formatter has been called explicitly
            throw new FormatException(
                $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format options.");
        }

        _cultureInfo = formattingInfo.FormatDetails.Provider as CultureInfo ?? CultureInfo.CurrentUICulture;

        var chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);

        formattingInfo.FormatAsChild(chosenFormat, formattingInfo.CurrentValue);

        return true;
    }

    private Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats,
        string[] chooseOptions)
    {
        var chosenIndex = GetChosenIndex(formattingInfo, chooseOptions, out var currentValueString);

        // Validate the number of formats:
        if (choiceFormats.Count < chooseOptions.Length)
            throw formattingInfo.FormattingException("You must specify at least " + chooseOptions.Length +
                                                     " choices");
        if (choiceFormats.Count > chooseOptions.Length + 1)
            throw formattingInfo.FormattingException("You cannot specify more than " + (chooseOptions.Length + 1) +
                                                     " choices");
        if (chosenIndex == -1 && choiceFormats.Count == chooseOptions.Length)
            throw formattingInfo.FormattingException("\"" + currentValueString +
                                                     "\" is not a valid choice, and a \"default\" choice was not supplied");

        if (chosenIndex == -1) chosenIndex = choiceFormats.Count - 1;

        var chosenFormat = choiceFormats[chosenIndex];
        return chosenFormat;
    }

    private int GetChosenIndex(IFormattingInfo formattingInfo, string[] chooseOptions, out string currentValueString)
    {
        string valAsString;

        // null and bool types are always case-insensitive
        switch (formattingInfo.CurrentValue)
        {
            case null:
                valAsString = currentValueString = "null";
                return Array.FindIndex(chooseOptions,
                    t => t.Equals(valAsString, StringComparison.OrdinalIgnoreCase));
            case bool boolVal:
                valAsString = currentValueString = boolVal.ToString();
                return Array.FindIndex(chooseOptions,
                    t => t.Equals(valAsString, StringComparison.OrdinalIgnoreCase));
        }
            
        valAsString = currentValueString = formattingInfo.CurrentValue.ToString();
            
        return Array.FindIndex(chooseOptions,
            t => AreEqual(t, valAsString, formattingInfo.FormatDetails.Settings.CaseSensitivity));
    }

    private bool AreEqual(string s1, string s2, CaseSensitivityType caseSensitivityFromSettings)
    {
        System.Diagnostics.Debug.Assert(_cultureInfo is not null);
        var culture = _cultureInfo!;

        var toUse = caseSensitivityFromSettings == CaseSensitivity
            ? caseSensitivityFromSettings
            : CaseSensitivity;

        return toUse == CaseSensitivityType.CaseSensitive
            ? culture.CompareInfo.Compare(s1, s2, CompareOptions.None) == 0
            : culture.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase) == 0;
    }

    /// <summary>
    /// Sets or gets the <see cref="CaseSensitivityType"/> for option strings.
    /// Defaults to <see cref="CaseSensitivityType.CaseSensitive"/>.
    /// Comparison of option strings is culture-aware.
    /// </summary>
    public CaseSensitivityType CaseSensitivity { get; set; } = CaseSensitivityType.CaseSensitive;
}
