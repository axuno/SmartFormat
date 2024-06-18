using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.Tests.TestUtils;
using SmartFormat.ZString;

namespace SmartFormat.Tests;

[TestFixture]
public class EvaluatorTests
{
    private static SmartFormatter GetSimpleFormatter(SmartSettings? settings = null)
    {
        var formatter = new SmartFormatter(settings ?? new SmartSettings()); 
        formatter.AddExtensions(new DefaultFormatter());
        formatter.AddExtensions(new ReflectionSource(), new DefaultSource());
        return formatter;
    }

    [Test]
    public void Events_Using_ReflectionSource_and_DefaultFormatter()
    {
        // This is also a test for EvaluatorEventAggregator from TestUtils

        var smart = GetSimpleFormatter();
        var watcher = new EvaluatorEventAggregator(smart.Evaluator);

        watcher.SubscribeEvents();
        const string name = "Joe";
        var obj = new { name };
        var result = smart.Format("Name: {name,10} * Another Literal", obj);

        /* This is the output of the EvaluatorEventAggregator:
           ---------------------------------------------------
           Events subscribed
           FORMAT:
             string: 'Name: {name,10} * Another Literal'
             items-count: 3
           LITERAL:
             string: 'Name: '
           OUTPUT_WRITTEN: 
             value: 'Name: '
           PLACEHOLDER: 
             string: {name,10}
           SELECTOR: 
             name: 'name'
             value: 'Joe'
             type: 'System.String'
             source: 'SmartFormat.Extensions.ReflectionSource'
           FORMATTING_START: 
             selector: 'name'
             value-type: 'System.String'
             value: 'Joe'
           OUTPUT_WRITTEN: 
             value: '       Joe'
           FORMATTING_END: 
             selector: 'name'
             value-type: 'System.String'
             current-value: 'Joe'
             formatter: 'SmartFormat.Extensions.DefaultFormatter'
           LITERAL:
             string: ' * Another Literal'
           OUTPUT_WRITTEN: 
             value: ' * Another Literal' 
           Events unsubscribed
        */
        Assert.Multiple(() =>
        {
            Assert.That(smart.Evaluator.OnFormattingStart, Is.Not.Null);
            Assert.That(smart.Evaluator.OnOutputWritten, Is.Not.Null);
            Assert.That(smart.Evaluator.OnFormattingEnd, Is.Not.Null);
            Assert.That(smart.Evaluator.OnFormat, Is.Not.Null);
            Assert.That(smart.Evaluator.OnLiteral, Is.Not.Null);
            Assert.That(smart.Evaluator.OnPlaceholder, Is.Not.Null);
            Assert.That(smart.Evaluator.OnSelectorValue, Is.Not.Null);
            Assert.That(smart.Evaluator.OnSelectorFailure, Is.Not.Null);
            Assert.That(watcher.Output, Does.Contain("""
                                                     FORMAT:
                                                       string: 'Name: {name,10} * Another Literal'
                                                       items-count: 3
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     LITERAL:
                                                       string: 'Name: '
                                                     OUTPUT_WRITTEN: 
                                                       value: 'Name: '
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     PLACEHOLDER: 
                                                       string: {name,10}
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     SELECTOR: 
                                                       name: 'name'
                                                       value: 'Joe'
                                                       type: 'System.String'
                                                       source: 'SmartFormat.Extensions.ReflectionSource'
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     FORMATTING_START: 
                                                       selector: 'name'
                                                       value-type: 'System.String'
                                                       value: 'Joe'
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     OUTPUT_WRITTEN: 
                                                       value: '       Joe'
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     FORMATTING_END: 
                                                       selector: 'name'
                                                       value-type: 'System.String'
                                                       current-value: 'Joe'
                                                       formatter: 'SmartFormat.Extensions.DefaultFormatter'
                                                     """));
            Assert.That(watcher.Output, Does.Contain("""
                                                     LITERAL:
                                                       string: ' * Another Literal'
                                                     OUTPUT_WRITTEN: 
                                                       value: ' * Another Literal'
                                                     """));
            Assert.That(result, Is.EqualTo("Name:        Joe * Another Literal"));
            watcher.UnsubscribeEvents();
        });
    }

    [Test]
    public void Events_With_Selector_Failure()
    {
        // This is also a test for EvaluatorEventAggregator from TestUtils

        var smart = Smart.CreateDefaultSmartFormat();
        smart.Registry.RemoveSourceExtension<ReflectionSource>(); // This will cause a Selector Failure
        var watcher = new EvaluatorEventAggregator(smart.Evaluator);

        watcher.SubscribeEvents();
        const string name = "Joe";
        var obj = new { name };
        Assert.Multiple(() =>
        {
            Assert.That(() => { smart.Format("{name,10}", obj); }, Throws.TypeOf<FormattingException>());
            watcher.UnsubscribeEvents();
            Assert.That(watcher.Output, Does.Contain("""
                                                     SELECTOR: 
                                                       name: 'name' 
                                                       source: not found
                                                     """));
        });
    }

    [Test]
    public void Events_With_Formatting_Failure()
    {
        // This is also a test for EvaluatorEventAggregator from TestUtils

        var smart = Smart.CreateDefaultSmartFormat();
        var watcher = new EvaluatorEventAggregator(smart.Evaluator);

        var formattingFailed = false;
        smart.Evaluator.OnFormattingFailure += (sender, e) => formattingFailed = true;

        watcher.SubscribeEvents();
        const string name = "Joe";
        var obj = new { name };
        Assert.Multiple(() =>
        {
            Assert.That(() => { _ = smart.Format("Name: {name,10:no_formatter:}", obj); }, Throws.TypeOf<FormattingException>());
            watcher.UnsubscribeEvents();
            Assert.That(formattingFailed, Is.True);
            Assert.That(watcher.Output, Does.Contain("""
                                                     FORMATTING_END: 
                                                       selector: 'name'
                                                       value-type: 'System.String'
                                                       current-value: 'Joe'
                                                       formatter: 'not found'
                                                     """));
        });
    }

    /// <summary>
    /// <see cref="FormattingInfo.TryGetValue"/> aims to be used by <see cref="SmartFormat.Core.Extensions.IFormatter"/> implementations.
    /// </summary>
    [Test]
    public void Get_Value_For_Placeholder_Using_FormattingInfo()
    {
        var args = new { Name = "Joe" };

        // prepare
        var smart = GetSimpleFormatter();
        var format = smart.Parser.ParseFormat("{unknown}{Name}");

        bool success1 = false, success2 = false, success3 = false;
        object? value2 = null, value3 = null;

        ExecuteFormattingAction(smart, null, format, new List<object?>(), new NullOutput(),
            fi =>
            {
                // Get the value for the first placeholder:
                fi.CurrentValue = args;
                success1 = fi.TryGetValue((Placeholder) format.Items.First(), out _) ||
                           fi.FormatDetails.FormattingException != null;
            });

        ExecuteFormattingAction(smart, null, format, new List<object?>(), new NullOutput(),
            fi =>
            {
                // Get the value for the last placeholder:
                fi.CurrentValue = args;
                success2 = fi.TryGetValue((Placeholder) format.Items.Last(), out value2) && fi.FormatDetails.FormattingException is null;
            });

        ExecuteFormattingAction(smart, null, format, new List<object?>(), new NullOutput(),
            fi =>
            {
                // All items from the input string are Placeholders. Remove their Selectors
                format.Items.ForEach(item => ((Placeholder) item).Selectors.Clear());
                fi.CurrentValue = "no placeholders";
                success3 = fi.TryGetValue((Placeholder) format.Items.First(), out value3);
            });

        Assert.Multiple(() =>
        {
            Assert.That(success1, Is.False);

            Assert.That(success2, Is.True);
            Assert.That(value2, Is.EqualTo("Joe"));

            Assert.That(success3, Is.True);
            Assert.That(value3, Is.EqualTo("no placeholders"));
        });
    }

    /// <summary>
    /// <see cref="FormattingInfo.FormatAsSpan(IFormatProvider, Format, object?)"/> aims to be used by <see cref="SmartFormat.Core.Extensions.IFormatter"/> implementations.
    /// </summary>
    [Test]
    public void Format_To_Span_Using_FormattingInfo()
    {
        var args = new { Name = "Joe" };

        // prepare
        var smart = GetSimpleFormatter();
        var format = smart.Parser.ParseFormat("My name is {Name}");

        ExecuteFormattingAction(smart, null, format, new List<object?>(), new NullOutput(),
            fi =>
            {
                // Get the value for the last placeholder:
                fi.CurrentValue = args;
                var result = fi.FormatAsSpan(null, format, args);
                Assert.That(result.ToString(), Is.EqualTo("My name is Joe"));
            });
    }

    /// <summary>
    /// <see cref="FormattingInfo.FormatAsSpan(IFormatProvider, Placeholder, object?)"/> aims to be used by <see cref="SmartFormat.Core.Extensions.IFormatter"/> implementations.
    /// </summary>
    [Test]
    public void Placeholder_To_Span_Using_FormattingInfo()
    {
        var args = new { Name = "Joe" };

        // prepare
        var smart = GetSimpleFormatter();
        var format = smart.Parser.ParseFormat("My name is {Name}");

        ExecuteFormattingAction(smart, null, format, new List<object?>(), new NullOutput(),
            fi =>
            {
                // Get the value for the last placeholder:
                fi.CurrentValue = args;
                var result = fi.FormatAsSpan(null, (Placeholder) format.Items.Last(), args);
                Assert.That(result.ToString(), Is.EqualTo("Joe"));
            });
    }

    #region ** Helpers **

    private static void ExecuteFormattingAction(SmartFormatter formatter, IFormatProvider? provider, Format formatParsed, IList<object?> args, IOutput output, Action<FormattingInfo> doWork)
    {
        // The first item is the default and will be used for the action,
        // but all args go to FormatDetails.OriginalArgs
        var current = args.Count > 0 ? args[0] : args;

        using var fdo = FormatDetailsPool.Instance.Pool.Get(out var formatDetails);
        formatDetails.Initialize(formatter, formatParsed, args, provider, output);

        using var fio = FormattingInfoPool.Instance.Pool.Get(out var formattingInfo);
        formattingInfo.Initialize(formatDetails, formatParsed, current);

        doWork(formattingInfo);
    }

    #endregion
}
