// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Core.Extensions;

/// <summary>
/// The <see cref="Registry"/> is managing the <see cref="ISource"/> and <see cref="IFormatter"/> extensions.
/// </summary>
internal class Registry
{
    private readonly List<ISource> _sourceExtensions = new();
    private readonly List<IFormatter> _formatterExtensions = new();
    private readonly SmartFormatter _formatter;

    /// <summary>
    /// Creates a new instance of the <see cref="Registry"/>.
    /// </summary>
    public Registry(SmartFormatter formatter)
    {
        Settings = formatter.Settings;
        _formatter = formatter;
    }

    /// <summary>
    /// Gets the list of <see cref="ISource" /> source extensions.
    /// </summary>
    internal List<ISource> SourceExtensions => _sourceExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="ISource" /> source extensions.
    /// </summary>
    public IReadOnlyList<ISource> GetSourceExtensions() => _sourceExtensions.AsReadOnly();

    /// <summary>
    /// Gets the list of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    internal List<IFormatter> FormatterExtensions => _formatterExtensions;

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of <see cref="IFormatter" /> formatter extensions.
    /// </summary>
    public IReadOnlyList<IFormatter> GetFormatterExtensions() => _formatterExtensions.AsReadOnly();

    /// <summary>
    /// Adds <see cref="ISource"/> extensions to the <see cref="GetSourceExtensions()"/> list,
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
    /// <returns>This <see cref="Registry"/> instance.</returns>
    public Registry AddExtensions(params ISource[] sourceExtensions)
    {
        foreach (var source in sourceExtensions)
        {
            var index = WellKnownExtensionTypes.GetIndexToInsert(SourceExtensions, source);
            _ = InsertExtension(index, source);

            // Also add the class as a formatter, if possible
            if (source is IFormatter formatter && FormatterExtensions.TrueForAll(fx => fx.GetType() != formatter.GetType())) AddExtensions(formatter);
        }
        return this;
    }

    /// <summary>
    /// Adds <see cref="IFormatter"/> extensions to the <see cref="GetFormatterExtensions()"/> list,
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
    /// <returns>This <see cref="Registry"/> instance.</returns>
    public Registry AddExtensions(params IFormatter[] formatterExtensions)
    {
        foreach (var formatter in formatterExtensions)
        {
            var index = WellKnownExtensionTypes.GetIndexToInsert(FormatterExtensions, formatter);
            _ = InsertExtension(index, formatter);

            // Also add the class as a source, if possible
            if (formatter is ISource source && SourceExtensions.TrueForAll(sx => sx.GetType() != source.GetType())) AddExtensions(source);
        }

        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="sourceExtension"></param>
    /// <returns>This <see cref="Registry"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public Registry InsertExtension(int position, ISource sourceExtension)
    {
        if (_sourceExtensions.Exists(sx => sx.GetType() == sourceExtension.GetType())) return this;

        if (sourceExtension is IInitializer sourceToInitialize)
            sourceToInitialize.Initialize(_formatter);

        _sourceExtensions.Insert(position, sourceExtension);

        return this;
    }

    /// <summary>
    /// Adds the <see cref="ISource"/> extensions at the <paramref name="position"/> of the <see cref="GetSourceExtensions()"/> list,
    /// if the <see cref="Type"/> has not been added before.
    /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
    /// </summary>
    /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
    /// <param name="formatterExtension"></param>
    /// <returns>This <see cref="Registry"/> instance.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///        <paramref name="position" /> is less than 0.
    ///         -or-
    ///        <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
    /// </exception>
    public Registry InsertExtension(int position, IFormatter formatterExtension)
    {
        if (_formatterExtensions.Exists(sx => sx.GetType() == formatterExtension.GetType())) return this;

        // Extension name is in use by a different type
        if (_formatterExtensions.Exists(fx => fx.Name.Equals(formatterExtension.Name)))
            throw new ArgumentException($"Formatter '{formatterExtension.GetType().Name}' uses existing name.", nameof(formatterExtension));

        if (formatterExtension is IInitializer formatterToInitialize)
            formatterToInitialize.Initialize(_formatter);

        _formatterExtensions.Insert(position, formatterExtension);

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
        return _sourceExtensions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Searches for a Formatter Extension of the given type, and returns it.
    /// Returns <see langword="null"/> if the type cannot be found.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The class implementing <see cref="IFormatter"/> if found, else <see langword="null"/>.</returns>
    public T? GetFormatterExtension<T>() where T : class, IFormatter
    {
        return _formatterExtensions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Removes Source Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveSourceExtension<T>() where T : class, ISource
    {
        var source = _sourceExtensions.OfType<T>().FirstOrDefault();
        return source is not null && _sourceExtensions.Remove(source);
    }

    /// <summary>
    /// Removes the Formatter Extension of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see langword="true"/>, if the extension was found and could be removed.</returns>
    public bool RemoveFormatterExtension<T>() where T : class, IFormatter
    {
        var format = _formatterExtensions.OfType<T>().FirstOrDefault();
        return format is not null && _formatterExtensions.Remove(format);
    }

    /// <summary>
    /// Checks whether the <see cref="Registry"/> has at least one <see cref="ISource"/> and one <see cref="IFormatter"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void ThrowIfNoExtensions()
    {
        if (_sourceExtensions.Count == 0)
            throw new InvalidOperationException(
                "No source extensions are available. Please add at least one source extension, such as the DefaultSource.");
        if (_formatterExtensions.Count == 0)
            throw new InvalidOperationException(
                "No formatter extensions are available. Please add at least one formatter extension, such as the DefaultFormatter.");
    }

    /// <summary>
    /// Try to get a suitable <see cref="ISource"/> extension.
    /// </summary>
    /// <param name="selectorInfo"></param>
    /// <exception cref="FormattingException"></exception>
    internal (bool Success, Type? SourceType) InvokeSourceExtensions(ISelectorInfo selectorInfo)
    {
#pragma warning disable S3267 // Don't use LINQ in favor of less GC
        foreach (var sourceExtension in _sourceExtensions)
        {
            var handled = sourceExtension.TryEvaluateSelector(selectorInfo);
            if (handled) return (true, sourceExtension.GetType());
        }
#pragma warning restore S3267 // Don't use LINQ in favor of less GC
        return (false, null);
    }


    /// <summary>
    /// First check whether the named formatter name exist in of the <see cref="FormatterExtensions" />,
    /// next check whether the named formatter is able to process the format.
    /// </summary>
    /// <param name="formattingInfo"></param>
    /// <returns><see langword="true"/>, if a <see cref="IFormatter"/> extensions was found.</returns>
    internal (bool Success, Type? FormatterType) InvokeFormatterExtensions(FormattingInfo formattingInfo)
    {
        if (formattingInfo.Placeholder is null)
            throw new ArgumentException($"The property {nameof(formattingInfo)}.{nameof(formattingInfo.Placeholder)} must not be null.", nameof(formattingInfo));

        // Compatibility mode does not support formatter extensions except this one:
        if (Settings.StringFormatCompatibility)
        {
            var extension = _formatterExtensions.First(fe => fe is DefaultFormatter);
            return (extension.TryEvaluateFormat(formattingInfo), extension.GetType());
        }

        var formatterName = formattingInfo.Placeholder.FormatterName;
        var comparison = Settings.GetCaseSensitivityComparison();

        // Try to evaluate using the non-empty formatter name from the format string
        if (formatterName != string.Empty)
        {
            IFormatter? formatterExtension = null;

#pragma warning disable S3267 // Don't use LINQ in favor of less GC
            foreach (var fe in _formatterExtensions)
            {
                if (!fe.Name.Equals(formatterName, comparison)) continue;

                formatterExtension = fe;
                break;
            }
#pragma warning restore S3267 // Don't use LINQ in favor of less GC

            return formatterExtension is null
                ? (false, null)
                : (formatterExtension.TryEvaluateFormat(formattingInfo), formatterExtension.GetType());
        }

        // Go through all (implicit) formatters which contain an empty name
        // much higher performance and less GC than using Linq
#pragma warning disable S3267 // Don't use LINQ in favor of less GC
        foreach (var fe in _formatterExtensions)
        {
            if (!fe.CanAutoDetect) continue;
            if (fe.TryEvaluateFormat(formattingInfo)) return (true, fe.GetType());
        }
#pragma warning restore S3267 // Don't use LINQ in favor of less GC
        return (false, null);
    }

    /// <summary>
    /// Gets the <see cref="SmartSettings" /> for Smart.Format
    /// </summary>
    public SmartSettings Settings { get; }
}
