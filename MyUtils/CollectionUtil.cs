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
        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> list) => list.OrderBy(x => x);

        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> list) => list.OrderByDescending(x => x);

        /// <summary>
        /// Take middle <paramref name="ratio"/> part from <paramref name="list"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="ratio">The ratio.</param>
        /// <param name="keepAtLeastOne">Tell to keep at least one item if the ratio is too aggressive</param>
        /// <returns></returns>
        public static IEnumerable<T> MiddlePartByRatio<T>(this IOrderedEnumerable<T> list, double ratio, bool keepAtLeastOne = false)
        {
            if (ratio > 1.0) throw new ArgumentException($"{ratio} is not a valid ratio.", nameof(ratio));

            if (list == null || !list.Any()) return list;

            var originalLength = list.Count();
            var result = list.Skip((int)Math.Round(originalLength * (1.0 - ratio) / 2)).Take((int)Math.Round(originalLength * ratio)).ToList();
            if (result.Count == 0 && keepAtLeastOne)
            {
                return new List<T>() { list.MedianOrDefault() };
            }

            return result;
        }

        /// <summary>
        /// Median or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static T MedianOrDefault<T>(this IOrderedEnumerable<T> list)
        {
            if (list == null || !list.Any()) return default(T);

            if (list.Count() == 1) return list.First();

            return list.ElementAt(list.Count() / 2);
        }

        /// <summary>
        /// Maximums or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static T MaxOrDefault<T>(this IEnumerable<T> list) => (list != null && list.Any()) ? list.Max() : default(T);

        /// <summary>
        /// Minimums or default.
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

        /// <summary>
        /// To support action like <see cref="List{T}.AddRange(IEnumerable{T})"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="inboundList"></param>
        public static void AddRange<T>(this HashSet<T> list, IEnumerable<T> inboundList)
        {
            if (inboundList == null) return;

            foreach (var item in inboundList)
            {
                list.Add(item);
            }
        }

        public static List<T> Intersect<T>(params IEnumerable<T>[] lists)
        {
            if (!lists.Any() || lists.Any(list => !list.Any()))
            {
                return new List<T>();
            }

            var tempList = lists[0];
            for (int i = 1; i < lists.Length; i++)
            {
                tempList = tempList.Intersect(lists[i]);
            }

            return tempList.ToList();
        }

    }
}
