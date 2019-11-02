using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Utilities;

namespace SmartFormat
{
    /// <summary>
    /// This class contains the Format method that constructs
    /// the composite string by invoking each extension.
    /// </summary>
    public class SmartFormatter
    {
        #region : EventHandlers :

        /// <summary>
        /// Event raising, if an error occurs during formatting.
        /// </summary>
        public event EventHandler<FormattingErrorEventArgs> OnFormattingFailure;

        #endregion

        #region: Constructor :

        public SmartFormatter()
        {
            Settings = new SmartSettings();
            Parser = new Parser(Settings);
            SourceExtensions = new List<ISource>();
            FormatterExtensions = new List<IFormatter>();
        }

        #endregion

        #region: Extension Registry :

        /// <summary>
        /// Gets the list of <see cref="ISource" /> source extensions.
        /// </summary>
        public List<ISource> SourceExtensions { get; }

        /// <summary>
        /// Gets the list of <see cref="IFormatter" /> formatter extensions.
        /// </summary>
        public List<IFormatter> FormatterExtensions { get; }

        /// <summary>
        /// Gets all names of registered formatter extensions which are not empty.
        /// </summary>
        /// <returns></returns>
        public string[] GetNotEmptyFormatterExtensionNames()
        {
            var names = new List<string>();
            foreach (var extension in FormatterExtensions)
                names.AddRange(extension.Names.Where(n => n != string.Empty).ToArray());
            return names.ToArray();
        }

        /// <summary>
        /// Adds each extensions to this formatter.
        /// Each extension must implement ISource.
        /// </summary>
        /// <param name="sourceExtensions"></param>
        public void AddExtensions(params ISource[] sourceExtensions)
        {
            SourceExtensions.InsertRange(0, sourceExtensions);
        }

        /// <summary>
        /// Adds each extensions to this formatter.
        /// Each extension must implement IFormatter.
        /// </summary>
        /// <param name="formatterExtensions"></param>
        public void AddExtensions(params IFormatter[] formatterExtensions)
        {
            FormatterExtensions.InsertRange(0, formatterExtensions);
        }


        /// <summary>
        /// Searches for a Source Extension of the given type, and returns it.
        /// This can be used to easily find and configure extensions.
        /// Returns null if the type cannot be found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSourceExtension<T>() where T : class, ISource
        {
            return SourceExtensions.OfType<T>().First();
        }

        /// <summary>
        /// Searches for a Formatter Extension of the given type, and returns it.
        /// This can be used to easily find and configure extensions.
        /// Returns null if the type cannot be found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFormatterExtension<T>() where T : class, IFormatter
        {
            return FormatterExtensions.OfType<T>().First();
        }

        #endregion

        #region: Properties :

        /// <summary>
        /// Gets or set the instance of the <see cref="Core.Parsing.Parser" />
        /// </summary>
        public Parser Parser { get; }

        /// <summary>
        /// Get the <see cref="Core.Settings.SmartSettings" /> for Smart.Format
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
            return Format(null, format, args ?? new object[] {null});
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specific object.
        /// </summary>
        /// <param name="provider">The <see cref="IFormatProvider" /> to use.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The object to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string Format(IFormatProvider provider, string format, params object[] args)
        {
            args = args ?? new object[] {null};
            var output = new StringOutput(format.Length + args.Length * 8);
            var formatParsed = Parser.ParseFormat(format, GetNotEmptyFormatterExtensionNames());
            var current = args.Length > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, formatParsed, args, null, provider, output);
            Format(formatDetails, formatParsed, current);

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
            args = args ?? new object[] {null};
            var formatParsed = Parser.ParseFormat(format, GetNotEmptyFormatterExtensionNames());
            var current = args.Length > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, formatParsed, args, null, null, output);
            Format(formatDetails, formatParsed, current);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specific object,
        /// using the <see cref="FormatCache"/>.
        /// </summary>
        /// <param name="cache">The <see cref="FormatCache" /> to use.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>Returns the formatted input with items replaced with their string representation.</returns>
        public string FormatWithCache(ref FormatCache cache, string format, params object[] args)
        {
            args = args ?? new object[] {null};
            var output = new StringOutput(format.Length + args.Length * 8);

            if (cache == null)
                cache = new FormatCache(Parser.ParseFormat(format, GetNotEmptyFormatterExtensionNames()));
            var current = args.Length > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
            Format(formatDetails, cache.Format, current);

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
            args = args ?? new object[] {null};
            if (cache == null)
                cache = new FormatCache(Parser.ParseFormat(format, GetNotEmptyFormatterExtensionNames()));
            var current = args.Length > 0 ? args[0] : args; // The first item is the default.
            var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
            Format(formatDetails, cache.Format, current);
        }

        private void Format(FormatDetails formatDetails, Format format, object current)
        {
            var formattingInfo = new FormattingInfo(formatDetails, format, current);
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
                    var errorIndex = placeholder.Format?.startIndex ?? placeholder.Selectors.Last().endIndex;
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
                    var errorIndex = placeholder.Format?.startIndex ?? placeholder.Selectors.Last().endIndex;
                    FormatError(item, ex, errorIndex, childFormattingInfo);
                }
            }
        }

        private void FormatError(FormatItem errorItem, Exception innerException, int startIndex,
            FormattingInfo formattingInfo)
        {
            OnFormattingFailure?.Invoke(this,
                new FormattingErrorEventArgs(errorItem.RawText, startIndex,
                    Settings.FormatErrorAction != ErrorAction.ThrowError));
            switch (Settings.FormatErrorAction)
            {
                case ErrorAction.Ignore:
                    return;
                case ErrorAction.ThrowError:
                    throw innerException as FormattingException ??
                          new FormattingException(errorItem, innerException, startIndex);
                case ErrorAction.OutputErrorInResult:
                    formattingInfo.FormatDetails.FormattingException =
                        innerException as FormattingException ??
                        new FormattingException(errorItem, innerException, startIndex);
                    formattingInfo.Write(innerException.Message);
                    formattingInfo.FormatDetails.FormattingException = null;
                    break;
                case ErrorAction.MaintainTokens:
                    formattingInfo.Write(formattingInfo.Placeholder.RawText);
                    break;
            }
        }

        private void CheckForExtensions()
        {
            if (SourceExtensions.Count == 0)
                throw new InvalidOperationException(
                    "No source extensions are available. Please add at least one source extension, such as the DefaultSource.");
            if (FormatterExtensions.Count == 0)
                throw new InvalidOperationException(
                    "No formatter extensions are available. Please add at least one formatter extension, such as the DefaultFormatter.");
        }

        private void EvaluateSelectors(FormattingInfo formattingInfo)
        {
            var placeholder = formattingInfo.Placeholder;
            var firstSelector = true;
            foreach (var selector in placeholder.Selectors)
            {
                formattingInfo.Selector = selector;
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
            var formatterName = formattingInfo.Placeholder.FormatterName;

            // Evaluate the named formatter (or, evaluate all "" formatters)
            foreach (var formatterExtension in FormatterExtensions)
            {
                if (!formatterExtension.Names.Contains(formatterName)) continue;
                var handled = formatterExtension.TryEvaluateFormat(formattingInfo);
                if (handled) return true;
            }

            return false;
        }

        #endregion
    }
}