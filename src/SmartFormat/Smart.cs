//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat
{
    /// <summary>
    /// This class holds a Default instance of a <see cref="SmartFormatter"/>.
    /// The default instance has all extensions registered.
    /// For optimized performance, create a <see cref="SmartFormatter"/> instance and register the
    /// particular extensions that are needed.
    /// </summary>
    public static class Smart
    {
        #region: Smart.Format :

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <remarks>Use <see cref="Default"/> or <see cref="SmartFormatter"/> for more <i>Format(...)</i> overloads.</remarks>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, params object?[] args)
        {
            return Default.Format(format, args);
        }

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <remarks>Use <see cref="Default"/> or <see cref="SmartFormatter"/> for more <i>Format(...)</i> overloads.</remarks>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(IFormatProvider provider, string format, params object?[] args)
        {
            return Default.Format(provider, format, args);
        }

        #endregion

        #region: Overloads to match the signature of string.Format, and allow support for programming languages that don't support "params" :

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <remarks>Use <see cref="Default"/> or <see cref="SmartFormatter"/> for more <i>Format(...)</i> overloads.</remarks>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, object? arg0, object? arg1, object? arg2)
        {
            return Default.Format(format,  arg0, arg1, arg2);
        }

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <remarks>Use <see cref="Default"/> or <see cref="SmartFormatter"/> for more <i>Format(...)</i> overloads.</remarks>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, object? arg0, object? arg1)
        {
            return Default.Format(format, arg0, arg1);
        }

        /// <summary>
        /// Formats 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <remarks>Use <see cref="Default"/> or <see cref="SmartFormatter"/> for more <i>Format(...)</i> overloads.</remarks>
        /// <returns></returns>
        public static string Format(string format, object? arg0)
        {
            return Default.Format(format, arg0);
        }

        #endregion

        #region: Default formatter :

        /// <summary>
        /// Gets or sets the default <see cref="SmartFormatter"/>.
        /// If not set, the <see cref="CreateDefaultSmartFormat"/> will be used.
        /// It is recommended to set the <see cref="Default"/> <see cref="SmartFormatter"/> with the extensions that are actually needed.
        /// </summary>
        public static SmartFormatter Default { get; set; } = CreateDefaultSmartFormat();

        /// <summary>
        /// Creates a <see cref="SmartFormatter"/> with all extensions registered.
        /// For optimized performance, create a <see cref="SmartFormatter"/> instance and register the
        /// particular extensions that are needed.
        /// </summary>
        /// <param name="settings">The <see cref="SmartSettings"/> to use, or <see langword="null"/> for default settings.</param>
        /// <returns>A <see cref="SmartFormatter"/> with all extensions registered.</returns>
        public static SmartFormatter CreateDefaultSmartFormat(SmartSettings? settings = null)
        {
            // Register all default extensions here:
            var formatter = new SmartFormatter(settings);
            
            // Add all extensions:
            // Note, the order is important; the extensions
            // will be executed in this order:

            var listSourceAndFormatter = new ListFormatter();

            // sources for specific types must be in the list before ReflectionSource
            formatter.AddExtensions(
                new StringSource(),
                (ISource) listSourceAndFormatter, // ListFormatter MUST be the second source extension
                new DictionarySource(),
                new ValueTupleSource(),
                new SystemTextJsonSource(),
                new NewtonsoftJsonSource(),
                new XmlSource(),
                new ReflectionSource(),

                // The DefaultSource reproduces the string.Format behavior:
                new DefaultSource()
            );
            formatter.AddExtensions(
                (IFormatter) listSourceAndFormatter,
                new PluralLocalizationFormatter("en"),
                new ConditionalFormatter(),
                new TimeFormatter("en"),
                new XElementFormatter(),
                new ChooseFormatter(),
                new SubStringFormatter(),
                new DefaultFormatter()
            );

            return formatter;
        }

        #endregion
    }
}