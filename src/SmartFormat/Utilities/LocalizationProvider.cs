//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// The default <see cref="ILocalizationProvider"/> for string localization.
    /// </summary>
    public class LocalizationProvider : ILocalizationProvider
    {
        internal readonly Dictionary<string, ResourceManager> Resources = new();

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="isCaseSensitive"><see langword="true"/>, if finding resource names should be case-sensitive (default).</param>
        /// <param name="resources">Parameters of type <see cref="ResourceManager"/>.</param>
        public LocalizationProvider(bool isCaseSensitive = true, params ResourceManager[] resources)
        {
            foreach (var resourceManager in resources)
            {
                DoAddResource(resourceManager, isCaseSensitive);
            }
        }

        /// <summary>
        /// Adds a new resource, if it does exist.
        /// </summary>
        /// <param name="resourceManager"></param>
        /// <param name="isCaseSensitive"></param>
        public virtual void AddResource(ResourceManager resourceManager, bool isCaseSensitive = true)
        {
            DoAddResource(resourceManager, isCaseSensitive);
        }

        private void DoAddResource(ResourceManager resourceManager, bool isCaseSensitive = true)
        {
            if (Resources.ContainsKey(resourceManager.BaseName)) return;
            resourceManager.IgnoreCase = !isCaseSensitive;
            Resources.Add(resourceManager.BaseName, resourceManager);
        }

        /// <summary>
        /// Removes a Resource.
        /// </summary>
        /// <param name="resourceBaseName"></param>
        /// <returns><see langword="true"/>, if the resource could be removed, else <see langword="false"/>.</returns>
        public virtual bool Remove(string resourceBaseName)
        {
            if (!Resources.ContainsKey(resourceBaseName)) return false;
            
            Resources.Remove(resourceBaseName);
            return true;

        }

        /// <summary>
        /// Remove all resources.
        /// </summary>
        public virtual void Clear()
        {
            Resources.Clear();
        }

        /// <summary>
        /// Gets the value from one of the registered resources.
        /// If no value was found, the <paramref name="name"/> is returned.
        /// </summary>
        /// <remarks>
        /// <see cref="CultureInfo.CurrentUICulture"/> is used, if no <see cref="CultureInfo"/> is provided as a parameter.
        /// </remarks>
        /// <param name="name"></param>
        /// <returns>The value from one of the registered resources. If no value was found, the <paramref name="name"/> is returned.</returns>
        public virtual string? GetString(string name)
        {
            return GetString(name, default(CultureInfo?));
        }

        /// <summary>
        /// Gets the value from one of the registered resources.
        /// If no value was found, the <paramref name="name"/> is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cultureName"></param>
        /// <returns>The value from one of the registered resources. If no value was found, the <paramref name="name"/> is returned.</returns>
        public virtual string? GetString(string name, string cultureName)
        {
            return GetString(name, CultureInfo.GetCultureInfo(cultureName));
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
        public virtual string? GetString(string name, CultureInfo? cultureInfo)
        {
            foreach (var resourceManager in Resources.Values)
            {
                var value = cultureInfo != null
                    ? resourceManager.GetString(name, cultureInfo)
                    : resourceManager.GetString(name);

                if (value is null && FallbackCulture != null) resourceManager.GetString(name, FallbackCulture);
                
                if (value is null) continue;

                return value;
            }

            return ReturnNameIfNotFound ? name : null;
        }

        /// <summary>
        /// Gets or sets the fallback <see cref="CultureInfo"/>, if the localized string cannot be found in the specified culture.
        /// </summary>
        public virtual CultureInfo? FallbackCulture { get; set; }

        /// <summary>
        /// If <see langword="true"/>, the requested name will be returned, instead of null.
        /// </summary>
        /// <remarks>
        /// Same behavior as e.g. <c>Microsoft.Extensions.Localization.ResourceManagerStringLocalizer</c>, if <see langword="true"/>.
        /// </remarks>
        public virtual bool ReturnNameIfNotFound { get; set; } = false;
    }
}
