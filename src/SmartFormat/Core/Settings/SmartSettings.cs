// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace SmartFormat.Core.Settings;

/// <summary>
/// <see cref="SmartFormat" /> settings to be applied for parsing and formatting.
/// <see cref="SmartSettings"/> are used to initialize instances.
/// Properties should be considered as 'init-only' like implemented in C# 9.
/// Any changes after passing settings as argument to CTORs may not have effect,
/// unless explicitly mentioned.
/// </summary>
public class SmartSettings
{
    /// <summary>
    /// CTOR.
    /// </summary>
    public SmartSettings()
    {
    }

    /// <summary>
    /// Gets or sets the thread safety mode.
    /// Thread safety is relevant for global caching, lists and object pools,
    /// which can be filled from different threads concurrently.
    /// <para><see langword="true"/> does <b>not</b> guarantee thread safety of all classes.</para>
    /// Default is <see langword="true"/>.
    /// </summary>
    public static bool IsThreadSafeMode { get; set; } = true;

    /// <summary>
    /// Uses <c>string.Format</c>-compatible escaping of curly braces, {{ and }},
    /// instead of the <c>Smart.Format</c> default escaping, \{ and \}.
    /// <para>Custom formatters cannot be parsed / used, if set to <see langword="true"/>.</para>
    /// <para>Default is <see langword="false"/>.</para>
    /// </summary>
    public bool StringFormatCompatibility { get; set; } = false;

    /// <summary>
    /// Gets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormatter" />.
    /// The default is <see cref="ErrorAction.ThrowError"/>.
    /// </summary>
    [Obsolete("Use 'SmartSettings.Formatter.ErrorAction' instead.", true)]
    public ErrorAction FormatErrorAction
    {
        get => (ErrorAction) Formatter.ErrorAction;
        set => Formatter.ErrorAction = (FormatErrorAction) value;
    }

    /// <summary>
    /// Gets the <see cref="ErrorAction" /> to apply for the <see cref="SmartFormat.Core.Parsing.Parser" />.
    /// The default is <see cref="ErrorAction.ThrowError"/>.
    /// </summary>
    [Obsolete("Use 'SmartSettings.Parser.ErrorAction' instead.", true)]
    public ErrorAction ParseErrorAction
    {
        get => (ErrorAction) Parser.ErrorAction;
        set => Parser.ErrorAction = (ParseErrorAction) value;
    }

    /// <summary>
    /// Determines whether placeholders are case-sensitive or not.
    /// The default is <see cref="CaseSensitivityType.CaseSensitive"/>.
    /// </summary>
    public CaseSensitivityType CaseSensitivity { get; set; } = CaseSensitivityType.CaseSensitive;

    /// <summary>
    /// This setting is relevant for the <see cref="Parsing.LiteralText" />.
    /// If true (the default), character string literals are treated like in "normal" string.Format:
    /// string.Format("\t")   will return a "TAB" character
    /// If false, character string literals are not converted, just like with this string.Format:
    /// string.Format(@"\t")  will return the 2 characters "\" and "t"
    /// </summary>
    [Obsolete("Use SmartSettings.Parser.ConvertCharacterStringLiterals instead", true)]
    public bool ConvertCharacterStringLiterals
    {
        get => Parser.ConvertCharacterStringLiterals;
        set => Parser.ConvertCharacterStringLiterals = value;
    }

    /// <summary>
    /// Gets the <see cref="StringComparer"/> that belongs to the <see cref="CaseSensitivity"/> setting.
    /// </summary>
    /// <returns>The <see cref="StringComparer"/> that belongs to the <see cref="CaseSensitivity"/> setting.</returns>
    public IEqualityComparer<string> GetCaseSensitivityComparer()
    {
        return CaseSensitivity == CaseSensitivityType.CaseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;
    }

    /// <summary>
    /// Gets the <see cref="StringComparison"/> that belongs to the <see cref="CaseSensitivity"/> setting.
    /// </summary>
    /// <returns>The <see cref="StringComparison"/> that belongs to the <see cref="CaseSensitivity"/> setting.</returns>
    public StringComparison GetCaseSensitivityComparison()
    {
        return CaseSensitivity == CaseSensitivityType.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
    }

    /// <summary>
    /// Gets the settings for the parser.
    /// Set only during initialization.
    /// </summary>
    public ParserSettings Parser { get; set; } = new();

    /// <summary>
    /// Gets the settings for the formatter.
    /// Set only during initialization.
    /// </summary>
    public FormatterSettings Formatter { get; set; } = new();

    /// <summary>
    /// Gets the settings for <see cref="Localization"/>.
    /// </summary>
    public Localization Localization { get; set; } = new();
        
    /// <summary>
    /// Gets the global static <see cref="PoolSettings"/> for object pooling.
    /// <para>These settings must be defined before any class calling the object pools is instantiated. They cannot be changed later.</para>
    /// </summary>
    public PoolSettings Pooling { get; set; }
}