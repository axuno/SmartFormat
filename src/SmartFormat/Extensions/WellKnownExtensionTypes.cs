// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions;

/// <summary>
/// Helper class for dealing with well-known <see cref="ISource"/> and <see cref="IFormatter"/> extensions.
/// </summary>
public static class WellKnownExtensionTypes
{
    private static HashSet<(Type ExtensionType, bool IsSingleton)>? _formatterTypes;
    private static HashSet<(Type ExtensionType, bool IsSingleton)>? _sourceTypes;

    /// <summary>
    /// Well-known <see cref="ISource"/> implementations in the sequence how they should (not must!) be invoked.
    /// </summary>
    public static Dictionary<string, int> Sources { get; } = new(StringComparer.Ordinal) {
        { "SmartFormat.Extensions.GlobalVariablesSource", 1000 },
        { "SmartFormat.Extensions.PersistentVariablesSource", 2000 },
        { "SmartFormat.Extensions.StringSource", 3000 },
        { "SmartFormat.Extensions.ListFormatter", 4000 },
        { "SmartFormat.Extensions.DictionarySource", 5000 },
        { "SmartFormat.Extensions.ValueTupleSource", 6000 },
        { "SmartFormat.Extensions.SystemTextJsonSource", 7000 },
        { "SmartFormat.Extensions.NewtonsoftJsonSource", 8000 },
        { "SmartFormat.Extensions.XmlSource", 9000 },
        // sources for specific types must be in the list before ReflectionSource
        { "SmartFormat.Extensions.ReflectionSource", 10000 },
        { "SmartFormat.Extensions.KeyValuePairSource", 11000 },
        { "SmartFormat.Extensions.DefaultSource", 12000 }
    };

    /// <summary>
    /// Well-known <see cref="IFormatter"/> implementations in the sequence how they should (not must!) be invoked.
    /// </summary>
    public static Dictionary<string, int> Formatters { get; } = new(StringComparer.Ordinal)
    {
        { "SmartFormat.Extensions.ListFormatter", 1000 },
        { "SmartFormat.Extensions.PluralLocalizationFormatter", 2000 },
        { "SmartFormat.Extensions.ConditionalFormatter", 3000 },
        { "SmartFormat.Extensions.TimeFormatter", 4000 },
        { "SmartFormat.Extensions.XElementFormatter", 5000 },
        { "SmartFormat.Extensions.IsMatchFormatter", 6000 },
        { "SmartFormat.Extensions.NullFormatter", 7000 },
        { "SmartFormat.Extensions.LocalizationFormatter", 8000 },
        { "SmartFormat.Extensions.TemplateFormatter", 9000 },
        { "SmartFormat.Extensions.ChooseFormatter", 10000 },
        { "SmartFormat.Extensions.SubStringFormatter", 11000 },
        { "SmartFormat.Extensions.LogiCalcFormatter", 12000 },
        { "SmartFormat.Extensions.DefaultFormatter", 13000 }
    };

    /// <summary>
    /// Determines where a new extension should be inserted in the
    /// list of existing extensions.
    /// </summary>
    /// <typeparam name="T">A type implementing <see cref="ISource"/> or <see cref="IFormatter"/>.</typeparam>
    /// <param name="currentExtensions"></param>
    /// <param name="extensionToInsert"></param>
    /// <returns></returns>
    internal static int GetIndexToInsert<T>(IList<T> currentExtensions, T extensionToInsert) where T:class
    {
        // It's the first extensions
        if (!currentExtensions.Any()) return 0;

        var wellKnownList = typeof(T).IsAssignableFrom(typeof(ISource)) ? Sources : Formatters;

        // Unknown extensions will add to the end
        if (!wellKnownList.TryGetValue(extensionToInsert.GetType().FullName!, out var indexOfNewExt))
            return currentExtensions.Count;
            
        for (var i = currentExtensions.Count - 1; i >= 0; i--)
        {
            var found = wellKnownList.TryGetValue(currentExtensions[i].GetType().FullName!, out var index);
            if (!found) continue;

            if (index > indexOfNewExt)
                continue;

            return i + 1;
        }

        // Add as first
        return 0;
    }

    /// <summary>
    /// Gets all referenced transient and singleton <see cref="IFormatter"/> and <see cref="ISource"/> extensions.
    /// </summary>
    /// <typeparam name="T"><see cref="IFormatter"/> or <see cref="ISource"/>.</typeparam>
    /// <returns>A <see cref="ValueTuple"/> with <see cref="Type"/> <see cref="HashSet{T}"/>s all extensions and a flag, if an extension is a singleton.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static HashSet<(Type ExtensionType, bool IsSingleton)> GetReferencedExtensions<T>()
    {
        // Note: Assembly.GetCallingAssembly() is the assembly calling this method *directly*

        if (typeof(T).IsAssignableFrom(typeof(IFormatter)))
        {
            if (_formatterTypes is null)
                _formatterTypes = FetchReferencedExtensions<T>(Assembly.GetCallingAssembly());

            return _formatterTypes;
        }

        if (typeof(T).IsAssignableFrom(typeof(ISource)))
        {
            if (_sourceTypes is null)
                _sourceTypes = FetchReferencedExtensions<T>(Assembly.GetCallingAssembly());

            return _sourceTypes;
        }

        throw new InvalidOperationException(
            $"The Type argument must be '{nameof(IFormatter)}' or '{nameof(ISource)}'.");
    }

    /// <summary>
    /// Fetches transient and singleton <see cref="IFormatter"/> and <see cref="ISource"/> extensions from assemblies.
    /// Core SmartFormat and referenced extension assemblies are included in the search.
    /// </summary>
    /// <param name="callingAssembly">The assembly which originally invoked this method.</param>
    /// <typeparam name="T"><see cref="IFormatter"/> or <see cref="ISource"/>.</typeparam>
    /// <returns>A <see cref="ValueTuple"/> with <see cref="Type"/> <see cref="HashSet{T}"/>s all extensions and a flag, if an extension is a singleton.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static HashSet<(Type ExtensionType, bool IsSingleton)> FetchReferencedExtensions<T>(Assembly callingAssembly)
    {
        // Select classes implementing T, having a public and parameterless constructor
        static bool TransientCondition(Type t) => typeof(T).IsAssignableFrom(t)
                                                  && !t.IsAbstract && t.IsClass
                                                  && t.GetConstructor(BindingFlags.Instance | BindingFlags.Public,
                                                      null, Type.EmptyTypes, null) != null;

        // Select classes implementing T, a static "Instance" property
        static bool SingletonCondition(Type t) => typeof(T).IsAssignableFrom(t)
                                                  && !t.IsAbstract && t.IsClass
                                                  && t.GetProperty("Instance",
                                                      BindingFlags.Static | BindingFlags.Public) != null;

        var allExtensionTypes = new HashSet<(Type ExtensionType, bool IsSingleton)>();

        // Get all extensions from core SmartFormat and referenced extension package assemblies
        foreach (var assembly in callingAssembly.GetReferencedAssemblies())
        {
            // Loads into the Default Load Context, dependencies are loaded automatically
            var referencedTypes = Assembly.Load(assembly).GetTypes();
            referencedTypes
                .Where(TransientCondition)
                .ToList().ForEach(t => allExtensionTypes.Add((t, false)));
            referencedTypes
                .Where(SingletonCondition)
                .ToList().ForEach(t => allExtensionTypes.Add((t, true)));
        }

        return allExtensionTypes;
    }

    /// <summary>
    /// Creates an instance of the given type.
    /// </summary>
    /// <typeparam name="T"><see cref="IFormatter"/> or <see cref="ISource"/>.</typeparam>
    /// <param name="wellKnown">A <see cref="ValueTuple"/> containing <see cref="Type"/> and <see langword="bool"/>.</param>
    /// <returns>An instance of the given type.</returns>
    internal static T CreateInstanceForType<T>((Type ExtensionType, bool IsSingleton) wellKnown)
    {
        if (wellKnown.IsSingleton)
            return (T) wellKnown.ExtensionType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)!.GetValue(wellKnown)!;

        // It's a transient type
        return (T) Activator.CreateInstance(Type.GetType(wellKnown.ExtensionType.AssemblyQualifiedName!)!)!;
    }
}
