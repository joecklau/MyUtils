using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace MyUtils
{
    public static class CollectionUtil
    {
        /// <summary>
        /// Maximums the or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static T MaxOrDefault<T>(this IEnumerable<T> list) => (list != null && list.Any()) ? list.Max() : default(T);

        /// <summary>
        /// Minimums the or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static T MinOrDefault<T>(this IEnumerable<T> list) => (list != null && list.Any()) ? list.Min() : default(T);

        /// <summary>
        /// Add or Update the (Concurrent) List
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TSource, TKey>(this IProducerConsumerCollection<TSource> list, Func<TSource, TKey> keySelector, TSource valueOnAdd, Action<TSource> mapOnUpdate)
            where TKey : IComparable
        {
            var target = list.SingleOrDefault(x => keySelector.Invoke(x).Equals(keySelector.Invoke(valueOnAdd)));
            if (target == null)
            {
                list.TryAdd(valueOnAdd);
            }
            else
            {
                mapOnUpdate(target);
            }
        }

        /// <summary>
        /// Add or Update the List
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TSource, TKey>(this IList<TSource> list, Func<TSource, TKey> keySelector, TSource valueOnAdd, Action<TSource> mapOnUpdate)
            where TKey : IComparable
        {
            var target = list.SingleOrDefault(x => keySelector.Invoke(x).Equals(keySelector.Invoke(valueOnAdd)));
            if (target == null)
            {
                list.Add(valueOnAdd);
            }
            else
            {
                mapOnUpdate(target);
            }
        }

        /// <summary>
        /// Add or Update the List
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TSource, TKey>(this IList<TSource> list, Func<TSource, TKey> keySelector, TSource value)
            where TKey : IComparable
        {
            var target = list.SingleOrDefault(x => keySelector.Invoke(x).Equals(keySelector.Invoke(value)));
            if (target == null)
            {
                list.Add(value);
            }
            else
            {
                var ind = list.IndexOf(target);
                list[ind] = value;
            }
        }
        
        public static TValue TryGetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, object defaultValue = null)
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = (TValue)defaultValue;
            }

            return result;
        }


        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
            }
        }

        //public static bool ContainsAll<TValue>(this IEnumerable<TValue> list, IEnumerable<TValue> list2)
        //{
        //    if ((list2?.Count() ?? 0) == 0)
        //    {
        //        return false;
        //    }
        //    return list.Intersect(list2).Distinct().Count() == list2.Distinct().Count();
        //}

    }
}
