//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate any <see cref="object"/> using <see cref="System.Reflection"/>.
    /// A type cache is used in order to reduce reflection calls.
    /// Include this source, if any of these types shall be used.
    /// </summary>
    public class ReflectionSource : Source
    {
        private static readonly object[] Empty = Array.Empty<object>();

        /// <summary>
        /// Gets the type cache <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// It could e.g. be pre-filled or cleared in a derived class.
        /// </summary>
        /// <remarks>
        /// Note: For reading, <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> and <see cref="ConcurrentDictionary{TKey, TValue}"/> perform equally.
        /// For writing, <see cref="ConcurrentDictionary{TKey, TValue}"/> is slower (tested under net5.0).
        /// </remarks>
        protected readonly ConcurrentDictionary<(Type, string?), (FieldInfo? field, MethodInfo? method)> TypeCache = new();

        /// <summary>
        /// Gets or sets, whether the type cache dictionary should be used.
        /// Enable the cache for significantly better performance.
        /// Default is <see langword="true"/>.
        /// </summary>
        public bool IsTypeCacheEnabled { get; set; } = true;
        
        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            var current = selectorInfo.CurrentValue;
            
            if (current is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }

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

            // Important:
            // GetMembers (opposite to GetMember!) returns all members, 
            // both those defined by the type represented by the current T:System.Type object 
            // AS WELL AS those inherited from its base types.
            var members = sourceType.GetMembers(bindingFlags).Where(m =>
                string.Equals(m.Name, selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()));
            foreach (var member in members)
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        //  Selector is a Field; retrieve the value:
                        var field = member as FieldInfo;
                        selectorInfo.Result = field?.GetValue(current);
                        if (IsTypeCacheEnabled) TypeCache[(sourceType, selector)] = (field, null);
                        return true;
                    case MemberTypes.Property:
                    case MemberTypes.Method:
                        MethodInfo? method;
                        if (member.MemberType == MemberTypes.Property)
                        {
                            //  Selector is a Property which is not WriteOnly
                            if (member is PropertyInfo { CanRead: true } prop)
                                method = prop.GetGetMethod();
                            else
                                continue;
                        }
                        else
                        {
                            //  Selector is a method
                            method = member as MethodInfo;
                        }

                        //  Check that this method is valid -- it needs to return a value and has to be parameter-less:
                        //  We are only looking for a parameter-less Function/Property:
                        if (method?.GetParameters().Length > 0) continue;

                        //  Make sure that this method is not void! It has to be a Function!
                        if (method?.ReturnType == typeof(void)) continue;

                        // Add to cache
                        if (IsTypeCacheEnabled) TypeCache[(sourceType, selector)] = (null, method);

                        //  Retrieve the Selectors/ParseFormat value:
                        selectorInfo.Result = method?.Invoke(current, Array.Empty<object>());
                        return true;
                }

            // We also cache failures so we don't need to call GetMembers again
            if (IsTypeCacheEnabled) TypeCache[(sourceType, selector)] = (null, null);

            return false;
        }
    }
}