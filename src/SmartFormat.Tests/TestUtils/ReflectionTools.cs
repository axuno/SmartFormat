using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartFormat.Tests.TestUtils;

public static class ReflectionTools
{
    /// <summary>
    /// Gets all subclasses for a generic type definition.
    /// </summary>
    /// <example>
    /// var types = typeof(ObjectPool&lt;&gt;).Assembly.GetSubclassesOf(typeof(ObjectPool&lt;&gt;));
    /// </example>
    /// <param name="assembly"></param>
    /// <param name="genericTypeDefinition"></param>
    /// <returns>The subclasses for a generic type definition.</returns>
    public static IEnumerable<Type> GetSubclassesOf(
        Assembly assembly,
        Type genericTypeDefinition)
    {
        // Scan all base types of the type
        IEnumerable<Type> GetAllAscendants(Type t)
        {
            var current = t;

            while (current is { BaseType: { } } && current.BaseType != typeof(object))
            {
                yield return current.BaseType;
                current = current.BaseType;
            }
        }

#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
        ArgumentNullException.ThrowIfNull(genericTypeDefinition, nameof(genericTypeDefinition));
#else
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        if (genericTypeDefinition == null)
            throw new ArgumentNullException(nameof(genericTypeDefinition));
#endif
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            throw new ArgumentException(
                @"Specified type is not a valid generic type definition.",
                nameof(genericTypeDefinition));

        return assembly.GetTypes()
            .Where(t => GetAllAscendants(t).Any(d =>
                d.IsGenericType &&
                d.GetGenericTypeDefinition() == genericTypeDefinition));
    }

    /// <summary>
    /// Tests whether the <paramref name="typeToTest"/> is a subclass of <paramref name="genericTypeDefinition"/>
    /// in <paramref name="assembly"/>.
    /// </summary>
    /// <param name="typeToTest"></param>
    /// <param name="assembly"></param>
    /// <param name="genericTypeDefinition"></param>
    /// <returns><see langword="true"/>, if the <paramref name="typeToTest"/> is a subclass.</returns>
    public static bool IsSubclassOf(Type typeToTest, Assembly assembly, Type genericTypeDefinition)
    {
        return GetSubclassesOf(assembly, genericTypeDefinition).Any(t => t == typeToTest);
    }
}
