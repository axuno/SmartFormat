//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;
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
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, params object[] args)
        {
            return Default.Format(format, args);
        }

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(IFormatProvider provider, string format, params object[] args)
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
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            return Format(format, new[] {arg0, arg1, arg2});
        }

        /// <summary>
        /// Replaces the format items in the specified format string with the string representation or the corresponding object.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns>The format items in the specified format string replaced with the string representation or the corresponding object.</returns>
        public static string Format(string format, object arg0, object arg1)
        {
            return Format(format, new[] {arg0, arg1});
        }

        /// <summary>
        /// Formats 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public static string Format(string format, object arg0)
        {
            return Default.Format(format, arg0); // call Default.Format in order to avoid infinite recursive method call
        }

        #endregion

        #region: Default formatter :

        /// <summary>
        /// Gets or sets the default <see cref="SmartFormatter"/>.
        /// </summary>
        public static SmartFormatter Default { get; set; } = CreateDefaultSmartFormat();

        /// <summary>
        /// Creates a <see cref="SmartFormatter"/> with all extensions registered.
        /// For optimized performance, create a <see cref="SmartFormatter"/> instance and register the
        /// particular extensions that are needed.
        /// </summary>
        /// <returns>A <see cref="SmartFormatter"/> with all extensions registered.</returns>
        public static SmartFormatter CreateDefaultSmartFormat()
        {
            // Register all default extensions here:
            var formatter = new SmartFormatter();
            // Add all extensions:
            // Note, the order is important; the extensions
            // will be executed in this order:

            var listSourceAndFormatter = new ListFormatter(formatter);

            // sources for specific types must be in the list before ReflectionSource
            formatter.AddExtensions(
                (ISource) listSourceAndFormatter, // ListFormatter MUST be the first source extension
                new DictionarySource(formatter),
                new ValueTupleSource(formatter),
                new JsonSource(formatter),
                new XmlSource(formatter),
                new ReflectionSource(formatter),

                // The DefaultSource reproduces the string.Format behavior:
                new DefaultSource(formatter)
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