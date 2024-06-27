//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat;

internal partial class Evaluator
{
    /// <summary>
    /// Event is raising, when an error occurs during evaluation of values or formats.
    /// </summary>
    public event EventHandler<FormattingErrorEventArgs>? OnFormattingFailure;

    /// <summary>
    /// Event raised when a <see cref="Format"/> is encountered.
    /// </summary>
    internal EventHandler<FormatEventArgs>? OnFormat;

    /// <summary>
    /// Event raised when a <see cref="LiteralText"/>> is encountered.
    /// </summary>
    internal EventHandler<LiteralEventArgs>? OnLiteral;

    /// <summary>
    /// Event raised when a <see cref="Placeholder"/> is encountered.
    /// </summary>
    internal EventHandler<PlaceholderEventArgs>? OnPlaceholder;

    /// <summary>
    /// Event raised when a <see cref="Selector"/> is evaluated.
    /// </summary>
    internal EventHandler<SelectorValueEventArgs>? OnSelectorValue;

    /// <summary>
    /// Event raised when a <see cref="Selector"/> fails to evaluate.
    /// </summary>
    internal EventHandler<SelectorValueEventArgs>? OnSelectorFailure;

    /// <summary>
    /// Event raised when formatting starts.
    /// </summary>
    internal EventHandler<FormattingEventArgs>? OnFormattingStart;

    /// <summary>
    /// Event raised when output was written by a <see cref="IFormattingInfo"/> instance.
    /// </summary>
    internal EventHandler<OutputWrittenEventArgs>? OnOutputWritten;

    /// <summary>
    /// Event raised when formatting ends.
    /// </summary>
    internal EventHandler<FormattingEventArgs>? OnFormattingEnd;

    /// <summary>
    /// Arguments for the <see cref="OnFormat"/> event.
    /// </summary>
    internal readonly record struct FormatEventArgs(Format Format)
    {
        public Format Format { get; } = Format;
    }

    internal readonly record struct OutputWrittenEventArgs(string WrittenValue)
    {
        public string WrittenValue { get; } = WrittenValue;
    }

    /// <summary>
    /// Arguments for the <see cref="OnLiteral"/> event.
    /// </summary>
    internal readonly record struct LiteralEventArgs(string Text)
    {
        public string Text { get; } = Text;
    }

    /// <summary>
    /// Arguments for the <see cref="OnPlaceholder"/> event.
    /// </summary>
    internal readonly record struct PlaceholderEventArgs(Placeholder Placeholder)
    {
        public Placeholder Placeholder { get; } = Placeholder;
    }

    /// <summary>
    /// Arguments for the <see cref="OnSelectorValue"/> event.
    /// </summary>
    internal readonly record struct SelectorValueEventArgs(Selector Selector, bool Success, Type? SourceType, object? Value)
    {
        public Selector Selector { get; } = Selector;
        public bool Success { get; } = Success;
        public Type? SourceType { get; } = SourceType;
        public object? Value { get; } = Value;
    }

    /// <summary>
    /// Arguments for the <see cref="OnFormattingStart"/> and <see cref="OnFormattingEnd"/> events.
    /// </summary>
    internal readonly record struct FormattingEventArgs(Selector Selector, object? Value, bool Success, Type? FormatterType)
    {
        public Selector Selector { get; } = Selector;
        public object? Value { get; } = Value;
        public bool Success { get; } = Success;
        public Type? FormatterType { get; } = FormatterType;
    }
}
