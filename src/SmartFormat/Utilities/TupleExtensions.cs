using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// Extensions for <see cref="ValueTuple"/>s.
    /// </summary>
    /// <remarks>
    /// Credits to James Esh for the code posted on
    /// https://stackoverflow.com/questions/46707556/detect-if-an-object-is-a-valuetuple
    /// </remarks>
    public static class TupleExtensions
    {
        // value tuples can have a maximum of 7 elements
        private static readonly HashSet<Type> ValueTupleTypes = new HashSet<Type>(new Type[]
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        });

        /// <summary>
        /// Extension method to check whether an object is of type <see cref="ValueTuple"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns <c>true</c>, if the object is of type <see cref="ValueTuple"/>.</returns>
        public static bool IsValueTuple(this object obj) => IsValueTupleType(obj.GetType());

        /// <summary>
        /// Extension method to check whether the given type is a <see cref="ValueTuple"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Returns <c>true</c>, if the type is a <see cref="ValueTuple"/>.</returns>
        public static bool IsValueTupleType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && ValueTupleTypes.Contains(type.GetGenericTypeDefinition());
        }

        /// <summary>
        /// A list of <see cref="object"/>s with the values for each <see cref="ValueTuple"/> field.
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns>Returns a list of <see cref="object"/>s with the values for each <see cref="ValueTuple"/> field.</returns>
        public static IEnumerable<object> GetValueTupleItemObjects(this object tuple) => GetValueTupleItemFields(tuple.GetType()).Select(f => f.GetValue(tuple));

        /// <summary>
        /// A list of <see cref="Type"/>s for the fields of a <see cref="ValueTuple"/>.
        /// </summary>
        /// <param name="tupleType"></param>
        /// <returns>Returns of list of <see cref="Type"/>s with the fields of a <see cref="ValueTuple"/>.</returns>
        public static IEnumerable<Type> GetValueTupleItemTypes(this Type tupleType) => GetValueTupleItemFields(tupleType).Select(f => f.FieldType);

        /// <summary>
        /// A list of <see cref="FieldInfo"/>s with the fields of a <see cref="ValueTuple"/>.
        /// </summary>
        /// <param name="tupleType"></param>
        /// <returns>Returns of list of <see cref="FieldInfo"/>s with the fields of a <see cref="ValueTuple"/>.</returns>
        public static List<FieldInfo> GetValueTupleItemFields(this Type tupleType)
        {
            var items = new List<FieldInfo>();

            FieldInfo field;
            var nth = 1;
            while ((field = tupleType.GetRuntimeField($"Item{nth}")) != null)
            {
                nth++;
                items.Add(field);
            }

            return items;
        }

        public static IEnumerable<object> GetValueTupleItemObjectsFlattened(this object tuple)
        {
            foreach (var theTuple in tuple.GetValueTupleItemObjects())
            {
                if (theTuple.IsValueTuple())
                {
                    foreach (var innerTuple in theTuple.GetValueTupleItemObjectsFlattened())
                    {
                        yield return innerTuple;
                    }
                }
                else
                {
                    yield return theTuple;
                }
            }
        }
    }
}
