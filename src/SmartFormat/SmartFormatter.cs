//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.ZString;

namespace SmartFormat;

/// <summary>
/// This class contains the Format method that constructs
/// the composite string by invoking each extension.
/// <para>
/// <b>Thread-safety</b>:<br/>
/// <see cref="Smart"/>.Format methods are thread-safe when <see cref="SmartSettings.IsThreadSafeMode"/> is <see langword="true"/>.
/// Other methods (e.g. AddExtensions, changing SmartSettings) are not thread-safe.
/// </para>
/// </summary>
public class SmartFormatter
{
    /// <summary>
    /// Creates a new instance of a <see cref="SmartFormatter"/>.
    /// <para>
    /// <b>Thread-safety</b>:<br/>
    /// <see cref="Smart"/>.Format methods are thread-safe when <see cref="SmartSettings.IsThreadSafeMode"/> is <see langword="true"/>.
    /// Other methods (e.g. AddExtensions, changing SmartSettings) are not thread-safe.
    /// </para>
    /// </summary>
    /// <param name="settings">
    /// The <see cref="SmartSettings"/> to use, or <see langword="null"/> for default settings.
    /// Any changes after passing settings as a parameter may not have effect.
    /// </param>
    public SmartFormatter(SmartSettings? settings = null)
    {
        Settings = settings ?? new SmartSettings();
        Parser = new Parser(Settings);

        Registry = new Registry(this);
        Evaluator = new Evaluator(this);
    }

    /// <summary>
    /// Event is raising, if an error occurs during evaluation of values or formats.
    /// </summary>
    public event EventHandler<FormattingErrorEventArgs>? OnFormattingFailure;

    /// <summary>
    /// The methods gets called from <see cref="SmartFormat.Evaluator"/> to signal an error.
    /// </summary>
    /// <param name="args"></param>
    internal void FormatError(FormattingErrorEventArgs args)
    {
        OnFormattingFailure?.Invoke(this, args);
    }

    #region: Wrappers for ExtensionRegistry :

    /// <summary>
    /// Gets the list of <see cref="ISource" /> source extensions.
    /// </summary>
    internal List<ISource> SourceExtensions => Registry.SourceExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="ISource" /> source extensions.
    /// </summary>
    public IReadOnlyList<ISource> GetSourceExtensions() => Registry.GetSourceExtensions();

    /// <summary>
    /// Gets the list of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    internal List<IFormatter> FormatterExtensions => Registry.FormatterExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    public IReadOnlyList<IFormatter> GetFormatterExtensions() => Registry.GetFormatterExtensions();

    /// <summary>
    /// Adds <see cref="ISource"/> extensions to the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before. <see cref="WellKnownExtensionTypes.Sources"/> are inserted
    /// at the recommended position, all others are added at the end of the list.
    /// <para>
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </para>
    /// <para>
    /// Extensions implementing <see cref="ISource"/> <b>and</b> <see cref="IFormatter"/>
    /// will be auto-registered for both.
    /// </para>
    /// </summary>
    /// <param name="sourceExtensions"><see cref="ISource"/> extensions in an arbitrary order.</param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    public SmartFormatter AddExtensions(params ISource[] sourceExtensions)
    {
        Registry.AddExtensions(sourceExtensions);
        return this;
    }

    /// <summary>
    /// Adds <see cref="IFormatter"/> extensions to the <see cref="GetFormatterExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before. <see cref="WellKnownExtensionTypes.Formatters"/> are inserted
    /// at the recommended position, all others are added at the end of the list.
    /// <para>
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </para>
    /// <para>
    /// Extensions implementing <see cref="ISource"/> <b>and</b> <see cref="IFormatter"/>
    /// will be auto-registered for both.
    /// </para>
    /// </summary>
    /// <param name="formatterExtensions"><see cref="ISource"/> extensions in an arbitrary order.</param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    public SmartFormatter AddExtensions(params IFormatter[] formatterExtensions)
    {
        Registry.AddExtensions(formatterExtensions);
        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="sourceExtension"></param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public SmartFormatter InsertExtension(int position, ISource sourceExtension)
    {
        Registry.InsertExtension(position, sourceExtension);
        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list of this formatter,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="formatterExtension"></param>
    /// <returns>This <see cref="SmartFormatter"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public SmartFormatter InsertExtension(int position, IFormatter formatterExtension)
    {
        Registry.InsertExtension(position, formatterExtension);
        return this;
    }

    /// <summary>
    /// Searches for a Source Extension of the given type, and returns it.
    /// Returns <see langword="null"/> if the type cannot be found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The class implementing <see cref="ISource"/> if found, else <see langword="null"/>.</returns>
    public T? GetSourceExtension<T>() where T : class, ISource
    {
        return Registry.GetSourceExtension<T>();
    }

    /// <summary>
    /// Searches for a Formatter Extension of the given type, and returns it.
    /// Returns <see langword="null"/> if the type cannot be found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The class implementing <see cref="IFormatter"/> if found, else <see langword="null"/>.</returns>
    public T? GetFormatterExtension<T>() where T : class, IFormatter
    {
        return Registry.GetFormatterExtension<T>();
    }

    /// <summary>
    /// Removes Source Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveSourceExtension<T>() where T : class, ISource
    {
        return Registry.RemoveSourceExtension<T>();
    }

    /// <summary>
    /// Removes the Formatter Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveFormatterExtension<T>() where T : class, IFormatter
    {
        return Registry.RemoveFormatterExtension<T>();
    }

    #endregion

    /// <summary>
    /// Gets or set the instance of the <see cref="Core.Parsing.Parser" />
    /// </summary>
    public Parser Parser { get; }

    /// <summary>
    /// Get the <see cref="SmartSettings" /> for Smart.Format
    /// </summary>
    public SmartSettings Settings { get; }

    /// <summary>
    /// The <see cref="Core.Extensions.Registry"/> is manages the extensions of the <see cref="SmartFormatter"/>.
    /// </summary>
    internal Registry Registry { get; }

    /// <summary>
    /// The <see cref="SmartFormat.Evaluator"/> class evaluates <see cref="Placeholder"/>s
    /// using <see cref="ISource"/> extensions, and formats these values using <see cref="IFormatter"/> extensions.
    /// </summary>
    internal Evaluator Evaluator { get; }

    #region: Format Overloads :

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(string format, params object?[] args)
    {
        return Format(null, format, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(string format, IList<object?> args)
    {
        return Format(null, format, args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, string format, params object?[] args)
    {
        return Format(provider, format, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, string format, IList<object?> args)
    {
        var formatParsed = Parser.ParseFormat(format);
        try
        {
            var result = Format(provider, formatParsed, args);
            return result;
        }
        finally
        {
            FormatPool.Instance.Return(formatParsed); // The parser gets the Format from the pool 
        }
    }

    #region ** Format overloads with cached Format **

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(Format formatParsed, params object?[] args)
    {
        return Format(null, formatParsed, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in as specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(Format formatParsed, IList<object?> args)
    {
        return Format(null, formatParsed, args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, Format formatParsed, params object?[] args)
    {
        return Format(provider, formatParsed, (IList<object?>) args);
    }

    /// <summary>
    /// Replaces one or more format items in a specified string with the string representation of a specific object.
    /// </summary>
    /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The object to format.</param>
    /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
    public string Format(IFormatProvider? provider, Format formatParsed, IList<object?> args)
    {
        using var zsOutput = new ZStringOutput(ZStringBuilderUtilities.CalcCapacity(formatParsed));
        FormatInto(zsOutput, provider, formatParsed, args);
        return zsOutput.ToString();
    }

    #endregion

    /// <summary>
    /// Format all items of the <see cref="FormattingInfo.Format"/> property of the <paramref name="formattingInfo"/>.
    /// </summary>
    /// <param name="formattingInfo">The <see cref="FormattingInfo.Format"/> must not be null.</param>
    /// <exception cref="ArgumentException">Throws if <see cref="FormattingInfo.Format"/> is null.</exception>
    public void Format(FormattingInfo formattingInfo)
    {
        Registry.ThrowIfNoExtensions();
        Evaluator.WriteFormat(formattingInfo);
    }

    #endregion

    #region: FormatInto Overloads :

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, string format, params object?[] args)
    {
        FormatInto(output, format, (IList<object?>) args);
    }

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, string format, IList<object?> args)
    {
        FormatInto(output, null, format, args);
    }

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, string format, IList<object?> args)
    {
        var formatParsed = Parser.ParseFormat(format);
        try
        {
            FormatInto(output, provider, formatParsed, args);
        }
        finally
        {
            FormatPool.Instance.Return(formatParsed); // The parser gets the Format from the pool    
        }
    }

    #endregion

    #region: FormatInto Overloads with cached Format :

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="format">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, Format format, params object?[] args)
    {
        FormatInto(output, provider, format, (IList<object?>) args);
    }

    #endregion

    /// <summary>
    /// Writes the formatting result into an <see cref="IOutput"/> instance.
    /// </summary>
    /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
    /// <param name="provider"></param>
    /// <param name="formatParsed">An instance of <see cref="Core.Parsing.Format"/> that was returned by <see cref="Parser.ParseFormat"/>.</param>
    /// <param name="args">The objects to format.</param>
    public void FormatInto(IOutput output, IFormatProvider? provider, Format formatParsed, IList<object?> args)
    {
        Registry.ThrowIfNoExtensions();
        ExecuteFormattingAction(this, provider, formatParsed, args, output, Evaluator.WriteFormat);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FormattingInfo"/>
    /// and performs the <see paramref="doWork"/> action
    /// using the <see cref="FormattingInfo"/>.
    /// </summary>
    /// <param name="formatter"></param>
    /// <param name="provider">The <see cref="IFormatProvider"/>, or null for using the default.</param>
    /// <param name="formatParsed"></param>
    /// <param name="args">
    /// The data argument. When it is a an <see cref="IList{T}"/>, the first element will be used for <paramref name="doWork"/>.
    /// The list goes to <see cref="FormatDetails.OriginalArgs"/>.
    /// </param>
    /// <param name="output">The <see cref="IOutput"/> to use, or null using the default.</param>
    /// <param name="doWork">The <see cref="Action{T}"/>to invoke.</param>
    /// <remarks>
    /// The method uses object pooling to reduce GC pressure,
    /// and assures that objects are returned to the pool after
    /// <see paramref="doWork"/> is done (or an exception is thrown).
    /// </remarks>
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
}
