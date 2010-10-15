using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Tests.Common
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> sources, Action<T> action)
        {
            foreach (var source in sources)
            {
                action(source);
            }
        }
    }
}
