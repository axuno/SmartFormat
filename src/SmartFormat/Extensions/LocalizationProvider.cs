//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// The default <see cref="ILocalizationProvider"/> for string localization.
    /// </summary>
    public class LocalizationProvider : ILocalizationProvider
    {
        internal readonly Dictionary<string, ResourceManager> Resources = new();

        /// <summary>
        /// Adds a new resource, if it does exist.
        /// </summary>
        /// <param name="resourceManager"></param>
        /// <param name="caseSensitivity"></param>
        public void AddResource(ResourceManager resourceManager, CaseSensitivityType caseSensitivity = CaseSensitivityType.CaseSensitive)
        {
            if (Resources.ContainsKey(resourceManager.BaseName)) return;
            resourceManager.IgnoreCase = caseSensitivity == CaseSensitivityType.CaseInsensitive;
            Resources.Add(resourceManager.BaseName, resourceManager);
        }

        /// <summary>
        /// Removes a Resource.
        /// </summary>
        /// <param name="resourceBaseName"></param>
        /// <returns><see langword="true"/>, if the resource could be removed, else <see langword="false"/>.</returns>
        public bool Remove(string resourceBaseName)
        {
            if (!Resources.ContainsKey(resourceBaseName)) return false;
            
            Resources.Remove(resourceBaseName);
            return true;

        }

        /// <summary>
        /// Remove all resources.
        /// </summary>
        public void Clear()
        {
            Resources.Clear();
        }

        /// <summary>
        /// Gets the value from one of the registered resources.
        /// If no value was found, the <paramref name="input"/> is returned.
        /// </summary>
        /// <remarks>
        /// <see cref="CultureInfo.CurrentUICulture"/> is used, if no <see cref="CultureInfo"/> is provided as a parameter.
        /// </remarks>
        /// <param name="input"></param>
        /// <returns>The value from one of the registered resources. If no value was found, the <paramref name="input"/> is returned.</returns>
        public string? GetString(string input)
        {
            return GetString(input, default(CultureInfo?));
        }

        /// <summary>
        /// Gets the value from one of the registered resources.
        /// If no value was found, the <paramref name="input"/> is returned.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cultureName"></param>
        /// <returns>The value from one of the registered resources. If no value was found, the <paramref name="input"/> is returned.</returns>
        public string? GetString(string input, string cultureName)
        {
            return GetString(input, CultureInfo.GetCultureInfo(cultureName));
        }

        /// <summary>
        /// Gets the value from one of the registered string resources for the specified culture.
        /// If no value was found, the <paramref name="name"/> is returned.
        /// </summary>
        /// <remarks>
        /// <see cref="CultureInfo.CurrentUICulture"/> is used, if no <see cref="CultureInfo"/> is provided as a parameter.
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="cultureInfo"></param>
        /// <returns>The value from one of the registered string resources for the specified culture. If no value was found, the <paramref name="name"/> is returned.</returns>
        public string? GetString(string name, CultureInfo? cultureInfo)
        {
            foreach (var resourceManager in Resources.Values)
            {
                var value = cultureInfo != null
                    ? resourceManager.GetString(name, cultureInfo)
                    : resourceManager.GetString(name);

                if (value is null) continue;

                return value;
            }

            return null;
        }
    }
}
