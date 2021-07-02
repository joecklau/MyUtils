using System;

namespace MyUtils
{
    public static class MathUtil
    {
        /// <summary>
        /// Limit the <paramref name="value"/> to be within <paramref name="minValue"/> and <paramref name="maxValue"/> inclusively
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int LimitToRange(this int value, int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentException($"{nameof(minValue)} {minValue} is not larger than {nameof(maxValue)} {maxValue}, which is invalid.");
            return Math.Max(Math.Min(value, maxValue), minValue);
        }

        /// <summary>
        /// Limit the <paramref name="value"/> to be within <paramref name="minValue"/> and <paramref name="maxValue"/> inclusively
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static double LimitToRange(this double value, double minValue, double maxValue)
        {
            if (minValue > maxValue) throw new ArgumentException($"{nameof(minValue)} {minValue} is not larger than {nameof(maxValue)} {maxValue}, which is invalid.");
            return Math.Max(Math.Min(value, maxValue), minValue);
        }

        /// <summary>
        /// Get Larger one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double Max(this int value, int other) => Math.Max(value, other);

        /// <summary>
        /// Get Larger one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double Max(this double value, double other) => Math.Max(value, other);

        /// <summary>
        /// Get Smaller one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double Min(this int value, int other) => Math.Min(value, other);

        /// <summary>
        /// Get Smaller one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double Min(this double value, double other) => Math.Min(value, other);

    }
}
