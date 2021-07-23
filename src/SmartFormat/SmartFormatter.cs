//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat
{
    /// <summary>
    /// This class contains the Format method that constructs
    /// the composite string by invoking each extension.
    /// </summary>
    public class SmartFormatter
    {
        private readonly List<ISource> _sourceExtensions = new();
        private readonly List<IFormatter> _formatterExtensions = new();

        #region : EventHandlers :

        /// <summary>
        /// Event raising, if an error occurs during formatting.
        /// </summary>
        public event EventHandler<FormattingErrorEventArgs>? OnFormattingFailure;

        #endregion

        #region: Constructor :

        /// <summary>
        /// Creates a new instance of a <see cref="SmartFormatter"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SmartSettings"/> to use, or <see langword="null"/> for default settings.</param>
        public SmartFormatter(SmartSettings? settings = null)
        {
            Settings = settings ?? new SmartSettings();
            Parser = new Parser(Settings);
        }

        #endregion

        #region: Extension Registry :

        /// <summary>
        /// Gets the list of <see cref="ISource" /> source extensions.
        /// </summary>
        public IReadOnlyList<ISource> SourceExtensions => _sourceExtensions;

        /// <summary>
        /// Gets the list of <see cref="IFormatter" /> formatter extensions.
        /// </summary>
        public IReadOnlyList<IFormatter> FormatterExtensions => _formatterExtensions;

        /// <summary>
        /// Adds the extensions at the beginning of the <see cref="SourceExtensions"/> list of this formatter, if the <see cref="Type"/> has not been added before.
        /// Each extension must implement <see cref="ISource"/>.
        /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
        /// </summary>
        public void AddExtensions(params ISource[] sourceExtensions)
        {
            AddExtensions(0, sourceExtensions);
        }

        /// <summary>
        /// Adds the extensions at the <paramref name="position"/> of the <see cref="SourceExtensions"/> list of this formatter, if the <see cref="Type"/> has not been added before.
        /// Each extension must implement <see cref="ISource"/>.
        /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
        /// </summary>
        /// <param name="position">The position in the <see cref="SourceExtensions"/> list where new extensions will be added.</param>
        /// <param name="sourceExtensions"></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///        <paramref name="position" /> is less than 0.
        ///         -or-
        ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
        /// </exception>
        public void AddExtensions(int position, params ISource[] sourceExtensions)
        {
            foreach (var source in sourceExtensions)
            {
                if (_sourceExtensions.All(sx => sx.GetType() != source.GetType()))
                {
                    if(source is IInitializer sourceToInitialize) 
                        sourceToInitialize.Initialize(this);
                    
                    _sourceExtensions.Insert(position, source);
                    position++;
                }
            }
        }

        /// <summary>
        /// Adds the extensions at the beginning of the <see cref="FormatterExtensions"/> list of this formatter, if the <see cref="Type"/> has not been added before.
        /// Each extension must implement <see cref="IFormatter"/>.
        /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
        /// </summary>
        /// <param name="formatterExtensions"></param>
        /// <exception cref="T:System.ArgumentException">
        ///        <paramref name="formatterExtensions" /> have <see cref="IFormatter.Names"/> that already exist.
        /// </exception>
        public void AddExtensions(params IFormatter[] formatterExtensions)
        {
            AddExtensions(0, formatterExtensions);
        }

        /// <summary>
        /// Adds the extensions at the <paramref name="position"/> of the <see cref="FormatterExtensions"/> list of this formatter, if the <see cref="Type"/> has not been added before.
        /// Each extension must implement <see cref="IFormatter"/>.
        /// If the extension implements <see cref="IInitializer"/>, <see cref="IInitializer.Initialize"/> will be invoked.
        /// </summary>
        /// <param name="formatterExtensions"></param>
        /// <param name="position">The position in the <see cref="FormatterExtensions"/> list where new extensions will be added.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///        <paramref name="position" /> is less than 0.
        ///         -or-
        ///         <paramref name="position" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///        <paramref name="formatterExtensions" /> have <see cref="IFormatter.Names"/> that already exist.
        /// </exception>
        public void AddExtensions(int position, params IFormatter[] formatterExtensions)
        {
            foreach (var format in formatterExtensions)
            {
                if (_formatterExtensions.All(fx => fx.GetType() != format.GetType()))
                {
                    if(_formatterExtensions.Any(fx => fx.Names.Where(n => n != string.Empty).Intersect(format.Names.Where(n => n != string.Empty)).Any()))
                        throw new ArgumentException($"Formatter '{format.GetType().Name}' uses existing name.", nameof(formatterExtensions));

                    if(format is IInitializer sourceToInitialize) 
                        sourceToInitialize.Initialize(this);

                    _formatterExtensions.Insert(position, format);
                    position++;
                }
            }
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

        #endregion

        #region: Properties :

        /// <summary>
        /// Gets or set the instance of the <see cref="Core.Parsing.Parser" />
        /// </summary>
        public Parser Parser { get; }

        /// <summary>
        /// Get the <see cref="SmartSettings" /> for Smart.Format
        /// </summary>
        public SmartSettings Settings { get; }

        #endregion

        #region: Format Overloads :

        /// <summary>
        /// Replaces one or more format items in as specified string with the string representation of a specific object.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The object to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string Format(string format, params object[] args)
        {
            return Format(null, format, (IList<object>) args);
        }

        /// <summary>
        /// Replaces one or more format items in as specified string with the string representation of a specific object.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The object to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string Format(string format, IList<object> args)
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
        public string Format(IFormatProvider? provider, string format, params object[] args)
        {
            return Format(provider, format, (IList<object>) args);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specific object.
        /// </summary>
        /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The object to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string Format(IFormatProvider? provider, string format, IList<object> args)
        {
            var output = new StringOutput(format.Length + args.Count * 8);
            var formatParsed = Parser.ParseFormat(format);
            var current = args.Count > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, formatParsed, args, null, provider, output);
            Format(formatDetails, current);

            return output.ToString();
        }

        /// <summary>
        /// Writes the formatting result into an <see cref="IOutput"/> instance.
        /// </summary>
        /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        public void FormatInto(IOutput output, string format, params object[] args)
        {
            FormatInto(output, format, (IList<object>) args);
        }

        /// <summary>
        /// Writes the formatting result into an <see cref="IOutput"/> instance.
        /// </summary>
        /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        public void FormatInto(IOutput output, string format, IList<object> args)
        {
            var formatParsed = Parser.ParseFormat(format);
            var current = args.Count > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, formatParsed, args, null, null, output);
            Format(formatDetails, current);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specific object,
        /// using the <see cref="FormatCache"/>.
        /// </summary>
        /// <param name="cache">The <see cref="FormatCache" /> to use.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string FormatWithCache(ref FormatCache? cache, string format, params object[] args)
        {
            return FormatWithCache(ref cache, format, (IList<object>) args);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specific object,
        /// using the <see cref="FormatCache"/>.
        /// </summary>
        /// <param name="cache">The <see cref="FormatCache" /> to use.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string FormatWithCache(ref FormatCache? cache, string format, IList<object> args)
        {
            var output = new StringOutput(format.Length + args.Count * 8);

            cache ??= new FormatCache(Parser.ParseFormat(format));
            var current = args.Count > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
            Format(formatDetails, current);

            return output.ToString();
        }

        /// <summary>
        /// Writes the formatting result into an <see cref="IOutput"/> instance, using the <see cref="FormatCache"/>.
        /// </summary>
        /// <param name="cache">The <see cref="FormatCache"/> to use.</param>
        /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        public void FormatWithCacheInto(ref FormatCache cache, IOutput output, string format, params object[] args)
        {
            FormatWithCacheInto(ref cache, output, format, (IList<object>) args);
        }

        /// <summary>
        /// Writes the formatting result into an <see cref="IOutput"/> instance, using the <see cref="FormatCache"/>.
        /// </summary>
        /// <param name="cache">The <see cref="FormatCache"/> to use.</param>
        /// <param name="output">The <see cref="IOutput"/> where the result is written to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        public void FormatWithCacheInto(ref FormatCache cache, IOutput output, string format, IList<object> args)
        {
            cache ??= new FormatCache(Parser.ParseFormat(format));
            var current = args.Count > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
            Format(formatDetails, current);
        }

        private void Format(FormatDetails formatDetails, object current)
        {
            var formattingInfo = new FormattingInfo(formatDetails, formatDetails.OriginalFormat, current);
            Format(formattingInfo);
        }

        #endregion

        #region: Format :

        /// <summary>
        /// Format the <see cref="FormattingInfo" /> argument.
        /// </summary>
        /// <param name="formattingInfo"></param>
        public void Format(FormattingInfo formattingInfo)
        {
            // Before we start, make sure we have at least one source extension and one formatter extension:
            CheckForExtensions();
            if (formattingInfo.Format is null) return;

            foreach (var item in formattingInfo.Format.Items)
            {
                if (item is LiteralText literalItem)
                {
                    formattingInfo.Write(literalItem.ToString());
                    continue;
                }

                // Otherwise, the item must be a placeholder.
                var placeholder = (Placeholder) item;
                var childFormattingInfo = formattingInfo.CreateChild(placeholder);
                try
                {
                    EvaluateSelectors(childFormattingInfo);
                }
                catch (Exception ex)
                {
                    // An error occurred while evaluation selectors
                    var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors.Last().EndIndex;
                    FormatError(item, ex, errorIndex, childFormattingInfo);
                    continue;
                }

                try
                {
                    EvaluateFormatters(childFormattingInfo);
                }
                catch (Exception ex)
                {
                    // An error occurred while evaluating formatters
                    var errorIndex = placeholder.Format?.StartIndex ?? placeholder.Selectors.Last().EndIndex;
                    FormatError(item, ex, errorIndex, childFormattingInfo);
                }
            }
        }

        private void FormatError(FormatItem errorItem, Exception innerException, int startIndex,
            FormattingInfo formattingInfo)
        {
            OnFormattingFailure?.Invoke(this,
                new FormattingErrorEventArgs(errorItem.RawText, startIndex,
                    Settings.Formatter.ErrorAction != FormatErrorAction.ThrowError));
            switch (Settings.Formatter.ErrorAction)
            {
                case FormatErrorAction.Ignore:
                    return;
                case FormatErrorAction.ThrowError:
                    throw innerException as FormattingException ??
                          new FormattingException(errorItem, innerException, startIndex);
                case FormatErrorAction.OutputErrorInResult:
                    formattingInfo.FormatDetails.FormattingException =
                        innerException as FormattingException ??
                        new FormattingException(errorItem, innerException, startIndex);
                    formattingInfo.Write(innerException.Message);
                    formattingInfo.FormatDetails.FormattingException = null;
                    break;
                case FormatErrorAction.MaintainTokens:
                    formattingInfo.Write(formattingInfo.Placeholder?.RawText ?? "'null'");
                    break;
            }
        }

        private void CheckForExtensions()
        {
            if (_sourceExtensions.Count == 0)
                throw new InvalidOperationException(
                    "No source extensions are available. Please add at least one source extension, such as the DefaultSource.");
            if (_formatterExtensions.Count == 0)
                throw new InvalidOperationException(
                    "No formatter extensions are available. Please add at least one formatter extension, such as the DefaultFormatter.");
        }

        private void EvaluateSelectors(FormattingInfo formattingInfo)
        {
            if (formattingInfo.Placeholder is null) return;
           
            var firstSelector = true;
            foreach (var selector in formattingInfo.Placeholder.Selectors)
            {
                // Don't evaluate empty selectors
                // (used e.g. for Settings.Parser.NullableOperator and Settings.Parser.ListIndexEndChar final operators)
                if(selector.Length == 0) continue;
                
                formattingInfo.Selector = selector;

                // If we have an alignment selector, we set the Alignment for its placeholder and move on
                if (TrySetPlaceholderAlignment(formattingInfo)) continue;
                formattingInfo.Result = null;
                
                var handled = InvokeSourceExtensions(formattingInfo);
                if (handled) formattingInfo.CurrentValue = formattingInfo.Result;

                if (firstSelector)
                {
                    firstSelector = false;
                    // Handle "nested scopes" by traversing the stack:
                    var parentFormattingInfo = formattingInfo;
                    while (!handled && parentFormattingInfo.Parent != null)
                    {
                        parentFormattingInfo = parentFormattingInfo.Parent;
                        parentFormattingInfo.Selector = selector;
                        parentFormattingInfo.Result = null;
                        handled = InvokeSourceExtensions(parentFormattingInfo);
                        if (handled) formattingInfo.CurrentValue = parentFormattingInfo.Result;
                    }
                }

                if (!handled)
                    throw formattingInfo.FormattingException($"Could not evaluate the selector \"{selector.RawText}\"",
                        selector);
            }
        }

        private bool InvokeSourceExtensions(FormattingInfo formattingInfo)
        {
            foreach (var sourceExtension in SourceExtensions)
            {
                var handled = sourceExtension.TryEvaluateSelector(formattingInfo);
                if (handled) return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get a suitable formatter.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <exception cref="FormattingException"></exception>
        private void EvaluateFormatters(FormattingInfo formattingInfo)
        {
            var handled = InvokeFormatterExtensions(formattingInfo);
            if (!handled)
                throw formattingInfo.FormattingException("No suitable Formatter could be found", formattingInfo.Format);
        }

        /// <summary>
        /// First check whether the named formatter name exist in of the <see cref="FormatterExtensions" />,
        /// next check whether the named formatter is able to process the format.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>True if an FormatterExtension was found, else False.</returns>
        private bool InvokeFormatterExtensions(FormattingInfo formattingInfo)
        {
            if (formattingInfo.Placeholder is null)
            {
                throw new ArgumentException(
                    $"{nameof(formattingInfo)}.{nameof(formattingInfo.Placeholder)} must not be null.");
            }

            var formatterName = formattingInfo.Placeholder.FormatterName;
            var comparer = Settings.GetCaseSensitivityComparer();

            // Compatibility mode does not support formatter extensions except this one:
            if (Settings.StringFormatCompatibility)
            {
                return 
                    FormatterExtensions.First(fe => fe.GetType() == typeof(DefaultFormatter) || fe.GetType().BaseType == typeof(DefaultFormatter))
                    .TryEvaluateFormat(formattingInfo);
            }

            // Try to evaluate using the not empty formatter name from the format string
            if (!string.IsNullOrEmpty(formatterName))
            {
                var extension = FormatterExtensions.FirstOrDefault(fe =>
                    fe.Names.Contains(formatterName, comparer));
                if (extension is null)
                    throw formattingInfo.FormattingException($"No formatter with name '{formatterName}' found",
                        formattingInfo.Format, formattingInfo.Selector?.SelectorIndex ?? -1);

                return extension.TryEvaluateFormat(formattingInfo);
            }
            
            // Go through all (implicit) formatters which contain an empty name
            foreach (var formatterExtension in FormatterExtensions.Where(fe => fe.Names.Contains(string.Empty, comparer)))
            {
                // Return true if handled by an extension
                if (formatterExtension.TryEvaluateFormat(formattingInfo)) return true;
            }
 
            return false;
        }

        /// <summary>
        /// Process pure alignment selectors.
        /// E.g. {Number,10} has an alignment selector containing ',10'
        /// </summary>
        /// <param name="selectorInfo">The <see cref="ISelectorInfo"/> to process.</param>
        /// <returns><see langword="true"/>, if the selector contains valid alignment, else <see langword="false"/>.</returns>
        private bool TrySetPlaceholderAlignment(ISelectorInfo selectorInfo)
        {
            // 1. The current selector belongs to a placeholder
            // 2. The operator character must have a value, usually ','
            // 3. The alignment is an integer value
            if (selectorInfo.Placeholder != null && 
                selectorInfo.SelectorOperator.FirstOrDefault() == Settings.Parser.AlignmentOperator &&
                int.TryParse(selectorInfo.SelectorText, out var selectorValue))
            {
                selectorInfo.Placeholder.Alignment = selectorValue;
                return true;
            }

            return false;
        }

        #endregion
    }
}