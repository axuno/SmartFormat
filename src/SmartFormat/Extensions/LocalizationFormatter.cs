//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Localization formatter allows for localizing strings from a language-specific <see cref="ILocalizationProvider"/>.
    /// </summary>
    public class LocalizationFormatter : IFormatter, IInitializer
    {
        private SmartFormatter? _formatter;
        private readonly bool _canHandleAutoDetection = false;
        
        /// <summary>
        /// Storage for localized versions of <see cref="Format"/>s
        /// to avoid repetitive parsing.
        /// </summary>
        internal IDictionary<string, Format>? LocalizedFormatCache;
        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"localize", "L"};

        ///<inheritdoc/>
        public string Name { get; set; } = "L";

        /// <inheritdoc/>
        /// <remarks>
        /// <see cref="TemplateFormatter"/> never can handle auto-detection.
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public bool CanAutoDetect
        {
            get
            {
                return _canHandleAutoDetection;
            }

            set
            {
                if (value) throw new ArgumentException($"{nameof(TemplateFormatter)} cannot handle auto-detection");
            }
        }

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.Format is null || formattingInfo.Format?.Length == 0)
            {
                throw new LocalizationFormattingException(formattingInfo.Format,
                    new ArgumentException($"'{nameof(formattingInfo.Format)}' for localization must not be null or empty.", nameof(formattingInfo)), 0);
            }
            
            if (LocalizationProvider is null)
            {
                throw new LocalizationFormattingException(formattingInfo.Format,
                    new NullReferenceException($"The {nameof(ILocalizationProvider)} must not be null."), 0);
            }

            var cultureInfo = GetCultureInfo(formattingInfo);
            formattingInfo.FormatDetails.Provider = cultureInfo;

            // Get the localized string
            var localized = LocalizationProvider!.GetString(formattingInfo.Format!.RawText, cultureInfo);

            if (localized is null) throw new LocalizationFormattingException(formattingInfo.Format, $"No localized string found for '{formattingInfo.Format!.RawText}'", formattingInfo.Format.StartIndex);
            
            // Use an existing Format from the cache
            if (LocalizedFormatCache!.TryGetValue(localized, out var format))
            {
                formattingInfo.FormatAsChild(format, formattingInfo.CurrentValue);
                return true;
            }

            // Parse the localized string and add it to the cache
            var parsed = _formatter!.Parser.ParseFormat(localized);
            LocalizedFormatCache!.Add(localized, parsed);

            formattingInfo.FormatAsChild(parsed, formattingInfo.CurrentValue);
            return true;
        }

        /// <summary>
        /// The <see cref="ILocalizationProvider"/> that resolves strings to localized versions.
        /// </summary>
        internal ILocalizationProvider? LocalizationProvider;

        ///<inheritdoc/>
        public void Initialize(SmartFormatter smartFormatter)
        {
            _formatter = smartFormatter;
            LocalizationProvider = _formatter.Settings.Localization.LocalizationProvider;
            var stringComparer = _formatter.Settings.GetCaseSensitivityComparer();
            LocalizedFormatCache = new Dictionary<string, Format>(stringComparer);
        }

        private static CultureInfo GetCultureInfo(IFormattingInfo formattingInfo)
        {
            var culture = formattingInfo.FormatterOptions.Trim();
            CultureInfo cultureInfo;
            if (culture == string.Empty)
            {
                if (formattingInfo.FormatDetails.Provider is CultureInfo ci)
                    cultureInfo = ci;
                else
                    cultureInfo = CultureInfo.CurrentUICulture; // also used this way by ResourceManager
            }
            else
            {
                try
                {
                    cultureInfo = CultureInfo.GetCultureInfo(culture);
                }
                catch (Exception e)
                {
                    throw new LocalizationFormattingException(formattingInfo.Format, e, 0);
                }
            }

            return cultureInfo;
        }
    }
}