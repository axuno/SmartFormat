using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#nullable enable
namespace SmartFormat.Demo.Sample_Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Joins all strings together into a single string, separating each item with the separator,
        /// and separating the last item with an alternate separator.
        /// Allows you to specify a cutoff, so that further items will not be displayed.
        /// <para>For example: </para> <c>alphabet.JoinStrings(", ", " and ", 4, " and {0} others...") == "a, b, c, d and 22 others..."</c>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator">This string will be inserted between items.</param>
        /// <param name="lastSeparator">This string will be inserted before the last item, instead of the separator.</param>
        /// <param name="cutoff">
        /// Limits the number of items.
        /// If the limit is reached, then cutoffText is appended.
        /// </param>
        /// <param name="cutoffText">
        /// The text that will be used if the cutoff is reached.
        /// For example, " and {0} others..."
        /// Optional placeholders: {0} = cutoff count, {1} = source count</param>
        public static string JoinStrings(this IEnumerable<string> source, string? separator, string? lastSeparator, string? cutoffText, int cutoff)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            separator ??= string.Empty;
            lastSeparator ??= separator;
            cutoffText ??= string.Empty;

            var sa = source as IList<string> ?? source.ToList(); // (we need the count)
            var sizeGuess = (cutoff > 0 ? Math.Min(cutoff, sa.Count) : sa.Count) * (8 + separator.Length); // Optimize by estimating the output length
            var sb = new StringBuilder(sizeGuess);
            var lastItem = sa.Count - 1;
            for (int i = 0; i < sa.Count; i++)
            {
                // see if we should cutoff:
                if ((cutoff > 0) && (i >= cutoff))
                {
                    sb.AppendFormat(cutoffText, sa.Count - cutoff, sa.Count);
                    break;
                }
                // Append the separator (after the first item)
                if (i > 0) sb.Append((i == lastItem) ? lastSeparator : separator);
                sb.Append(sa[i]);
            }
            return sb.ToString();
        }

        #region: Random Item Selector :

        /// <summary>Chooses one of the items at random.
        ///
        /// Throws an exception if there are no items.
        /// </summary>
        public static T Random<T>(this IEnumerable<T> source) where T : new()
        {
            ArgumentValidator.CheckForEmpty(source, "source");
            return RandomIterator(source);
        }
        /// <summary>Chooses one of the items at random.
        ///
        /// Returns default if there are no items.
        /// </summary>
        public static T RandomOrDefault<T>(this IEnumerable<T> source) where T : new()
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            return RandomIterator(source);
        }

        private static T RandomIterator<T>(this IEnumerable<T> source) where T:new()
        {
            // We need to know the count. We don't want to enumerate twice,
            // so if the source isn't a collection, let's create one:
            var sc = source as ICollection<T> ?? source.ToList();
            var itemCount = sc.Count;
            if (itemCount == 0)
            {
                return new T();
            }

            var index = (new Random()).Next(itemCount);
            return sc.ElementAt(index);
        }

        #endregion

        #region: Split :

        /// <summary>Splits the enumeration into segments,
        /// each segment containing [count] items, and the last segment containing the remainder.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count">The number of items to include in each segment.
        /// The last segment might contain fewer items.</param>
        public static IEnumerable<TSource[]> Split<TSource>(this IEnumerable<TSource> source, int count)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForZeroValue(count, "count");
            return SplitIterator(source, count);
        }
        private static IEnumerable<TSource[]> SplitIterator<TSource>(this IEnumerable<TSource> source, int count)
        {
            // Splits an enumeration into sections of equal size
            using (var enumerator = source.GetEnumerator())
            {
                TSource[] section = {};
                var index = -1;

                while (enumerator.MoveNext())
                {
                    // Group [count] items together:
                    index++;
                    if (index == 0) section = new TSource[count];
                    section[index] = enumerator.Current;

                    // When the section is full, output it:
                    if (index == count - 1)
                    {
                        yield return section;
                        index = -1;
                    }
                }

                // See if there is a partial final section:
                if (0 <= index)
                {
                    count = index + 1;
                    var finalSection = new TSource[count];
                    Array.Copy(section, finalSection, count);
                    yield return finalSection;
                }
                yield break;
            }
        }

        /// <summary>Splits the enumeration into segments,
        /// each segment containing the portion between predicate matches.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="splitPredicate">The condition that will create a new split.
        /// The matching item will be the first element in the new split.
        /// </param>
        public static IEnumerable<TSource[]> Split<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> splitPredicate)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(splitPredicate, "splitPredicate");
            return SplitIterator(source, splitPredicate, null, false);
        }
        /// <summary>Splits the enumeration into segments,
        /// each segment containing the portion between predicate matches.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="splitPredicate">The condition that will create a new split.
        /// The matching item will be the last element in the previous split.
        /// </param>
        public static IEnumerable<TSource[]> SplitAfter<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> splitPredicate)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(splitPredicate, "splitPredicate");
            return SplitIterator(source, splitPredicate, null, true);
        }
        /// <summary>Splits the enumeration into segments,
        /// each segment containing the portion between predicate matches.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="splitPredicate">The condition that will create a new split.
        /// The matching item will be the first element in the new split.
        /// This predicate incorporates the element's index.
        /// </param>
        public static IEnumerable<TSource[]> Split<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> splitPredicate)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(splitPredicate, "splitPredicate");
            return SplitIterator(source, null, splitPredicate, false);
        }
        /// <summary>Splits the enumeration into segments,
        /// each segment containing the portion between predicate matches.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="splitPredicate">The condition that will create a new split.
        /// The matching item will be the last element in the previous split.
        /// This predicate incorporates the element's index.
        /// </param>
        public static IEnumerable<TSource[]> SplitAfter<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> splitPredicate)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(splitPredicate, "splitPredicate");
            return SplitIterator(source, null, splitPredicate, true);
        }

        /// <param name="source"></param>
        /// <param name="splitPredicateWithIndex">Can be null, if splitPredicate is specified.</param>
        /// <param name="splitPredicate">Can be null, if splitPredicateWithIndex is specified</param>
        /// <param name="splitAfterPredicate">Determines if the split will come before or after a matching item.</param>
        private static IEnumerable<TSource[]> SplitIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool>? splitPredicate, Func<TSource, int, bool>? splitPredicateWithIndex, bool splitAfterPredicate)
        {
            // We require EITHER splitPredicate OR splitPredicateWithIndex, NOT both.
            if (splitPredicateWithIndex == null) ArgumentValidator.CheckForNullReference(splitPredicate, "splitPredicate");

            // Split the enumeration into sections
            using (var enumerator = source.GetEnumerator())
            {
                var section = new List<TSource>();
                var index = -1;
                while (enumerator.MoveNext())
                {
                    index++;
                    var current = enumerator.Current;

                    // Either add the current item before or after:
                    if (splitAfterPredicate) section.Add(current);

                    // Evaluate the predicate:
                    var shouldSplit = (splitPredicateWithIndex == null)
                        ? splitPredicate!(current)
                        : splitPredicateWithIndex(current, index);
                    if (shouldSplit && section.Count > 0)
                    {
                        // yield what we've collected so far, and then create a new split:
                        yield return section.ToArray();
                        section = new List<TSource>();
                    }

                    // Either add the current item before or after:
                    if (!splitAfterPredicate) section.Add(current);
                }

                // See if there is a partial final section:
                if (section.Count > 0)
                {
                    yield return section.ToArray();
                }
                yield break;
            }
        }

        #endregion

        #region: ForEach :

        /// <summary>Performs an action for each item in the list.
        /// For simple actions, this provides an easier syntax than the normal foreach loop.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action">This action method takes into account the index of the item.</param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(action, "action");

            // If the item is a List, we can optimize by using a for loop:
            var list = source as IList<TSource>;
            if (list != null)
            {
                // Perform the action on each item:
                for (int i = 0; i < list.Count; i++)
                {
                    action(list[i], i);
                }
                return;
            }

            // Perform the action on each item:
            int itemIndex = 0;
            foreach (var item in source)
            {
                action(item, itemIndex++);
            }
        }
        /// <summary>Performs an action for each item in the list.
        /// For simple actions, this provides an easier syntax than the normal foreach loop.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            ArgumentValidator.CheckForNullReference(source, "source");
            ArgumentValidator.CheckForNullReference(action, "action");

            // If the item is a List, we can optimize by using a for loop:
            var list = source as IList<TSource>;
            if (list != null)
            {
                // Perform the action on each item:
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    action(list[i]);
                }
                return;
            }

            // Perform the action on each item:
            foreach (var item in source)
            {
                action(item);
            }
        }

        #endregion

        #region: Overloads for extensions in System.Linq.Enumerable :

        /// <summary>Concatenates the first sequence with the specified items.
        ///
        /// This method is an overload for Concat that uses the params argument to provide
        /// a better syntax when used with a known set of items.
        /// </summary>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] secondItems)
        {
            return first.Concat((IEnumerable<T>) secondItems);
        }
        /// <summary>Produces the set union of the first sequence and the specified items by using the default equality comparer.
        ///
        /// This method is an overload for Union that uses the params argument to provide
        /// a better syntax when used with a known set of items.
        /// </summary>
        public static IEnumerable<T> Union<T>(this IEnumerable<T> first, params T[] secondItems)
        {
            return first.Union((IEnumerable<T>) secondItems);
        }

        #endregion
    }
}
