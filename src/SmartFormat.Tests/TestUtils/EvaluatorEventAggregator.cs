//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Text;

namespace SmartFormat.Tests.TestUtils;
internal class EvaluatorEventAggregator
{
    private readonly Evaluator _evaluator;
    private StringBuilder _output;

    public EvaluatorEventAggregator(Evaluator evaluator)
    {
        _evaluator = evaluator;
        _output = new StringBuilder(4096);
    }

    public string Output => _output.ToString();

    public void SubscribeEvents()
    {
        _evaluator.OnFormat += OnFormat;
        _evaluator.OnLiteral += OnLiteral;
        _evaluator.OnPlaceholder += OnPlaceholder;
        _evaluator.OnSelectorValue += OnSelectorValue;
        _evaluator.OnSelectorFailure += OnSelectorFailure;
        _evaluator.OnFormattingStart += OnFormattingStart;
        _evaluator.OnFormattingEnd += OnFormattingEnd;
        _evaluator.OnOutputWritten += OnOutputWritten;
        _output.Clear();
        _output.AppendLine("Events subscribed");
    }

    public void UnsubscribeEvents()
    {
        _evaluator.OnFormat -= OnFormat;
        _evaluator.OnLiteral -= OnLiteral;
        _evaluator.OnPlaceholder -= OnPlaceholder;
        _evaluator.OnSelectorValue -= OnSelectorValue;
        _evaluator.OnSelectorFailure -= OnSelectorFailure;
        _evaluator.OnFormattingEnd -= OnFormattingEnd;
        _evaluator.OnOutputWritten -= OnOutputWritten;
        _output.AppendLine("Events unsubscribed");
    }

    private void OnFormat(object? sender, Evaluator.FormatEventArgs e)
    {
        _output.Append($"""
                        FORMAT:
                          string: '{e.Format}'
                          items-count: {e.Format.Items.Count}{Environment.NewLine}
                        """);
    }

    private void OnLiteral(object? sender, Evaluator.LiteralEventArgs e)
    {
        _output.Append($"""
                        LITERAL:
                          string: '{e.Text}'{Environment.NewLine}
                        """);
    }

    private void OnPlaceholder(object? sender, Evaluator.PlaceholderEventArgs e)
    {
        _output.Append($"""
                        PLACEHOLDER: 
                          string: {e.Placeholder}{Environment.NewLine}
                        """);
    }

    private void OnSelectorValue(object? sender, Evaluator.SelectorValueEventArgs e)
    {
        if (!e.Success) return;
        _output.Append($"""
                        SELECTOR: 
                          name: '{e.Selector}'
                          value: '{e.Value ?? "null"}'
                          type: '{(e.Value is null ? "null" : e.Value.GetType())}'
                          source: '{(e.SourceType is null ? "null" : e.SourceType)}'{Environment.NewLine}
                        """);
    }

    private void OnSelectorFailure(object? sender, Evaluator.SelectorValueEventArgs e)
    {
        _output.Append($"""
                        SELECTOR: 
                          name: '{e.Selector}' 
                          source: not found{Environment.NewLine}
                        """);
    }

    private void OnFormattingStart(object? sender, Evaluator.FormattingEventArgs e)
    {
        _output.Append($"""
                        FORMATTING_START: 
                          selector: '{e.Selector}'
                          value-type: '{(e.Value is null ? "null" : e.Value.GetType())}'
                          value: '{e.Value ?? "null"}'{Environment.NewLine}
                        """);
    }

    private void OnOutputWritten(object? sender, Evaluator.OutputWrittenEventArgs e)
    {
        _output.Append($"""
                        OUTPUT_WRITTEN: 
                          value: '{e.WrittenValue}'{Environment.NewLine}
                        """);
    }

    private void OnFormattingEnd(object? sender, Evaluator.FormattingEventArgs e)
    {
        if (e.Success)
            _output.Append($"""
                            FORMATTING_END: 
                              selector: '{e.Selector}'
                              value-type: '{(e.Value is null ? "null" : e.Value.GetType())}'
                              current-value: '{e.Value ?? "null"}'
                              formatter: '{e.FormatterType}'{Environment.NewLine}
                            """);
        else
            _output.Append($"""
                            FORMATTING_END: 
                              selector: '{e.Selector}'
                              value-type: '{(e.Value is null ? "null" : e.Value.GetType())}'
                              current-value: '{e.Value ?? "null"}'
                              formatter: 'not found'{Environment.NewLine}
                            """);
    }
}
