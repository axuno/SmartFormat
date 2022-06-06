// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Pooling.SmartPools;

/// <summary>
/// The class contains static instances of classes which can be used to Clear()
/// pooled Smart objects (which are not <see cref="Nullable{T}"/>).
/// <para>
/// If an instance already contains or is an <see cref="InitializationObject"/>, it should
/// not be returned to the pool.
/// </para>
/// </summary>
internal static class InitializationObject
{
    /// <summary>
    /// A static <see cref="List{T}"/> instance.
    /// </summary>
    public static readonly IList<object?> ObjectList = new List<object?>(1);

    /// <summary>
    /// A static <see cref="List{T}"/> instance.
    /// </summary>
    public static readonly List<int> IntegerList = new List<int>(1);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Settings.SmartSettings"/> instance.
    /// </summary>
    public static readonly SmartSettings SmartSettings = new();

    /// <summary>
    /// A static <see cref="SmartFormat.SmartFormatter"/> instance.
    /// </summary>
    public static readonly SmartFormatter SmartFormatter = new(SmartSettings);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Parsing.Format"/> instance.
    /// </summary>
    public static readonly Format Format = new Format().Initialize(SmartSettings, "init");

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Parsing.LiteralText"/> instance.
    /// </summary>
    public static readonly LiteralText LiteralText = new LiteralText().Initialize(SmartSettings, Format, string.Empty, 0, 0);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Parsing.Placeholder"/> instance.
    /// </summary>
    public static readonly Placeholder Placeholder = new Placeholder().Initialize(Format, 0, 0);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Parsing.Selector"/> instance.
    /// </summary>
    public static readonly Selector Selector = new Selector().Initialize(SmartSettings, Placeholder, string.Empty, 0, 0, 0, 0);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Output.IOutput"/> instance.
    /// </summary>
    public static readonly IOutput Output = new NullOutput();

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Formatting.FormatDetails"/> instance.
    /// </summary>
    public static readonly FormatDetails FormatDetails = new FormatDetails().Initialize(SmartFormatter, Format, ObjectList, null, Output);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Formatting.FormatDetails"/> instance.
    /// </summary>
    public static readonly FormattingInfo FormattingInfo = new FormattingInfo().Initialize(FormatDetails, Format, default);

    /// <summary>
    /// A static <see cref="SmartFormat.Core.Parsing.ParsingErrors"/> instance.
    /// </summary>
    public static readonly ParsingErrors ParsingErrors = new ParsingErrors().Initialize(Format);

    /// <summary>
    /// A static <see cref="Core.Parsing.SplitList"/> instance.
    /// </summary>
    public static readonly SplitList SplitList = new SplitList().Initialize(Format, IntegerList);
}