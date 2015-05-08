using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// Source: http://stackoverflow.com/a/23092082
namespace DHXDownloadManager
{ 
    public static class ListExtensions
    {
        /// <summary>
        /// BinarySearch using an arbitrary selector
        /// Source: http://stackoverflow.com/a/23092082
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="tf"></param>
        /// <param name="target"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static int BinarySearch<T, U>(this IList<T> tf, U target, System.Func<T, U> selector)
        {
            var lo = 0;
            var hi = (int)tf.Count - 1;
            var comp = Comparer<U>.Default;

            while (lo <= hi)
            {
                var median = lo + (hi - lo >> 1);
                var num = comp.Compare(selector(tf[median]), target);
                if (num == 0)
                    return median;
                if (num < 0)
                    lo = median + 1;
                else
                    hi = median - 1;
            }

            return ~lo;
        }
    }
}