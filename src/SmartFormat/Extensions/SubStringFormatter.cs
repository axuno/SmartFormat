// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;

namespace SmartFormat.Extensions;

/// <summary>
/// Formatter lets you output part of an input string. 
/// </summary>
public class SubStringFormatter : IFormatter
{
    private char _splitChar = ',';

    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    public string[] Names { get; set; } = {"substr"};

    ///<inheritdoc/>
    public string Name { get; set; } = "substr";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = false;

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
    /// Get or set the string to display for NULL values, defaults to <see cref="string.Empty"/>.
    /// <para>
    /// It will <b>not</b> be used, if a format option is provided to the formatter.
    /// In this case, the child formatter must handle the NULL result.
    /// </para>
    /// </summary>
    public string NullDisplayString { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the behavior when start index and/or length are too big, defaults to <see cref="SubStringOutOfRangeBehavior.ReturnEmptyString"/>.
    /// </summary>
    public SubStringOutOfRangeBehavior OutOfRangeBehavior { get; set; } = SubStringOutOfRangeBehavior.ReturnEmptyString;

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var parameters = formattingInfo.FormatterOptions.Split(SplitChar);
        if (formattingInfo.CurrentValue is not (string or null) || parameters.Length == 1 && parameters[0].Length == 0)
        {
            // Auto detection calls just return a failure to evaluate
            if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                return false;

            // throw, if the formatter has been called explicitly
            throw new FormatException(
                $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 1 formatter option and a string? argument.");
        }

        var currentValue = formattingInfo.CurrentValue?.ToString();
            
        var substring = currentValue == null ? ReadOnlySpan<char>.Empty : GetSubstring(currentValue.AsSpan(), parameters);

        var format = formattingInfo.Format;
        // A format was supplied, so use it if valid
        if (format is not null && format.Length > 0)
        {
            if (!format.HasNested)
                throw new FormattingException(formattingInfo.Format, "The format requires a nested placeholder",
                    format.StartIndex);

            formattingInfo.FormatAsChild(format, currentValue == null ? null : substring.ToString());
            return true;
        }

        // Just output the substring directly
        if (currentValue == null)
        {
            formattingInfo.Write(NullDisplayString);
            return true;
        }

        formattingInfo.Write(substring);

        return true;
    }

    private ReadOnlySpan<char> GetSubstring(ReadOnlySpan<char> currentValue, string[] parameters)
    {
        var (startPos, length) = GetStartAndLength(currentValue, parameters);

        switch(OutOfRangeBehavior)
        {
            case SubStringOutOfRangeBehavior.ReturnEmptyString:
                if (startPos + length > currentValue.Length)
                    length = 0;
                break;
            case SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString:
                if (startPos + length > currentValue.Length)
                    length = currentValue.Length - startPos;
                break;
        }

        // SubStringOutOfRangeBehavior.ThrowException:
        // Without prior adjustments, this may throw 
        return parameters.Length > 1
            ? currentValue.Slice(startPos, length)
            : currentValue.Slice(startPos);
    }

    private static (int startPos, int length) GetStartAndLength(ReadOnlySpan<char> currentValue, string[] parameters)
    {
        var startPos = int.Parse(parameters[0]);
        var length = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
        if (startPos < 0)
            startPos = currentValue.Length + startPos;
        if (startPos > currentValue.Length)
            startPos = currentValue.Length;
        if (length < 0)
            length = currentValue.Length - startPos + length;

        return (startPos, length);
    }

    /// <summary>
    /// Specify behavior when start index and/or length is out of range
    /// </summary>
    public enum SubStringOutOfRangeBehavior
    {
        /// <summary>
        /// Returns string.Empty
        /// </summary>
        ReturnEmptyString,
        /// <summary>
        /// Returns the remainder of the string, starting at StartIndex
        /// </summary>
        ReturnStartIndexToEndOfString,
        /// <summary>
        /// Throws <see cref="SmartFormat.Core.Formatting.FormattingException"/> 
        /// </summary>
        ThrowException
    }
}