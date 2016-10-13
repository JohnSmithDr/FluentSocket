using System;
using System.Collections.Generic;

namespace Tests
{
    static class Utils
    {
        public static T AddInto<T>(this T item, ICollection<T> coll)
        {
            coll.Add(item);
            return item;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action.Invoke(item);
            }
        }
    }
}