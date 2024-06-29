// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions;

/// <summary>
/// Class to evaluate any <see cref="object"/> using <see cref="System.Reflection"/>.
/// A type cache is used in order to reduce reflection calls.
/// Include this source, if any of these types shall be used.
/// <para/>
/// For <see cref="SmartSettings.CaseSensitivity"/>==<see cref="CaseSensitivityType.CaseInsensitive"/>,
/// when there are multiple members with the same name but different case, the first member is used.
/// </summary>
public class ReflectionSource : Source
{
    private static readonly object[] Empty = Array.Empty<object>();

    private static int _maxCacheSize = DefaultCacheSize;

    /// <summary>
    /// The default cache size.
    /// </summary>
    public const int DefaultCacheSize = 500;

    /// <summary>
    /// Gets or sets the maximum cache size. Default is <see cref="DefaultCacheSize"/>.
    /// </summary>
    public static int MaxCacheSize
    {
        get => _maxCacheSize;
        set => _maxCacheSize = value > 0 ? value : DefaultCacheSize;
    }

    /// <summary>
    /// Gets the type cache <see cref="IDictionary{TKey,TValue}"/>.
    /// It could e.g. be pre-filled or cleared in a derived class.
    /// <para/>
    /// We use the default <see cref="EqualityComparer{T}"/> for the key,
    /// because case-sensitivity is handled in method <see cref="EvaluateMembers"/>.
    /// </summary>
    /// <remarks>
    /// For reading, <see cref="Dictionary{TKey, TValue}"/> and <see cref="ConcurrentDictionary{TKey, TValue}"/> perform equally.
    /// For writing, <see cref="ConcurrentDictionary{TKey, TValue}"/> is slower with more garbage (tested under net5.0).
    /// </remarks>
    protected internal static readonly IDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)> TypeCache =
    SmartSettings.IsThreadSafeMode
        ? new ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>(EqualityComparer<(Type, string?)>.Default)
        : new Dictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)>(MaxCacheSize, EqualityComparer<(Type, string?)>.Default);

#if !NET6_0_OR_GREATER
    /// <summary>
    /// Keeps track of the insertion order into the <see cref="TypeCache"/>.
    /// This is needed before NETCore3.1, where the dictionary is not ordered by insertion order.
    /// </summary>
    protected internal static readonly ConcurrentQueue<(Type, string?)> KeyList = new();
#endif

    /// <summary>
    /// Gets or sets, whether the type cache dictionary should be used.
    /// Enable the cache for significantly better performance.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool IsTypeCacheEnabled { get; set; } = true;
        
    /// <inheritdoc />
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        var current = selectorInfo.CurrentValue;
            
        if (TrySetResultForNullableOperator(selectorInfo)) return true;

        // strings are processed by StringSource
        if (current is null or string) return false; 
            
        var selector = selectorInfo.SelectorText;
        var sourceType = current.GetType();

        // Check the type cache, if enabled
        if (IsTypeCacheEnabled && TypeCache.TryGetValue((sourceType, selector), out var found))
        {
            if (found.field != null)
            {
                selectorInfo.Result = found.field.GetValue(current);
                return true;
            }

            if (found.method != null)
            {
                selectorInfo.Result = found.method.Invoke(current, Empty);
                return true;
            }

            return false;
        }

        if (EvaluateMembers(selectorInfo, selector, current, sourceType)) return true;

        // We also cache failures, so we don't need to call GetMembers again
        AddToCache(sourceType, selector, null, null, IsTypeCacheEnabled);

        return false;
    }

    private bool EvaluateMembers(ISelectorInfo selectorInfo, string selector, object current, Type sourceType)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        var comparison = selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison();

        // Note 1: GetMembers (opposite to GetMember!) returns all members, 
        //         both those defined by the type represented by the current T:System.Type object 
        //         AS WELL AS those inherited from its base types.
        // Note 2: Member names are case-sensitive, so we need to compare using Settings.CaseSensitivity.
        //         When there are multiple members with the same name but different case,
        //         the first member is used.
        var members = sourceType.GetMembers(bindingFlags)
            .Where(m =>
            string.Equals(m.Name, selector, comparison));

        foreach (var member in members)
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    // Selector is a Field; retrieve the value:
                    var field = member as FieldInfo;
                    selectorInfo.Result = field?.GetValue(current);
                    AddToCache(sourceType, selector, field, null, IsTypeCacheEnabled);
                    return true;
                case MemberTypes.Property:
                case MemberTypes.Method:
                    if (!TryGetMethodInfo(member, out var method)) continue;
 
                    // Check that this method is valid -- it needs to return a value and has to be parameter-less:
                    // We are only looking for a parameter-less Function/Property:
                    if (method?.GetParameters().Length > 0) continue;

                    // Make sure that this method is not void! It has to be a Function!
                    if (method?.ReturnType == typeof(void)) continue;

                    // Add to cache
                    AddToCache(sourceType, selector, null, method, IsTypeCacheEnabled);

                    // Retrieve the Selectors/ParseFormat value:
                    selectorInfo.Result = method?.Invoke(current, Array.Empty<object>());
                    return true;
            }

        return false;
    }

    /// <summary>
    /// Adds an item to the type cache, and removes the oldest item
    /// if the new cache size would exceed <see cref="MaxCacheSize"/>.
    /// </summary>
    private static void AddToCache(Type sourceType, string selector, FieldInfo? field, MethodInfo? method, bool isCacheEnabled)
    {
        if (!isCacheEnabled) return;

#if NET6_0_OR_GREATER
        while (TypeCache.Count > 0 && TypeCache.Count >= MaxCacheSize)
        {
            // Starting from NETCore3.1, the dictionary is ordered by insertion order
            TypeCache.Remove(TypeCache.First());
        }

        TypeCache[(sourceType, selector)] = (field, method);
#else
            while (TypeCache.Count > 0 && TypeCache.Count >= MaxCacheSize)
        {
            // For frameworks NETCore3.1, we have to track insertion order by ourselves
            if (KeyList.TryDequeue(out var key))
                TypeCache.Remove(key);
        }

        TypeCache[(sourceType, selector)] = (field, method);
        KeyList.Enqueue((sourceType, selector));
#endif
    }

    private static bool TryGetMethodInfo(MemberInfo member, out MethodInfo? method)
    {
        if (member.MemberType == MemberTypes.Property)
        {
            //  Selector is a Property which is not WriteOnly
            if (member is PropertyInfo { CanRead: true } prop)
            {
                method = prop.GetGetMethod();
                return true;
            }

            method = null;
            return false;
        }

        //  Selector is a method
        method = member as MethodInfo;
        return true;
    }
}
