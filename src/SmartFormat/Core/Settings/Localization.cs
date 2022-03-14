// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Extensions;
using SmartFormat.Utilities;

namespace SmartFormat.Core.Settings
{
    /// <summary>
    /// <see cref="SmartFormat" /> settings to be applied for localization.
    /// <see cref="SmartSettings"/> are used to initialize instances.
    /// Properties should be considered as 'init-only' like implemented in C# 9.
    /// Any changes after passing settings as argument to CTORs may not have effect. 
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        public Localization()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="ILocalizationProvider"/> used for localizing strings.
        /// Defaults to <see langword="null"/>.
        /// </summary>
        public ILocalizationProvider? LocalizationProvider { get; set; }
    }
}