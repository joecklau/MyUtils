using System;

namespace MyUtils
{
    public static class MathUtil
    {
        /// <summary>
        /// Greatest Common Divisor from https://stackoverflow.com/a/20824923/4684232
        /// </summary>
        public static int Gfc(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Least Common Multiple from https://stackoverflow.com/a/20824923/4684232
        /// </summary>
        public static int Lcm(int a, int b)
        {
            return (a / Gfc(a, b)) * b;
        }

        /// <summary>
        /// Greatest Common Divisor from https://stackoverflow.com/a/20824923/4684232
        /// </summary>
        public static decimal Gfc(decimal a, decimal b)
        {
            while (b != 0)
            {
                decimal temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Least Common Multiple from https://stackoverflow.com/a/20824923/4684232
        /// </summary>
        public static decimal Lcm(decimal a, decimal b)
        {
            return (a / Gfc(a, b)) * b;
        }

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
        /// Limit the <paramref name="value"/> to be within <paramref name="minValue"/> and <paramref name="maxValue"/> inclusively
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static decimal LimitToRange(this decimal value, decimal minValue, decimal maxValue)
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
        public static int AtLeast(this int value, int other) => Math.Max(value, other);

        /// <summary>
        /// Get Larger one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double AtLeast(this double value, double other) => Math.Max(value, other);

        /// <summary>
        /// Get Larger one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static decimal AtLeast(this decimal value, decimal other) => Math.Max(value, other);

        /// <summary>
        /// Get Smaller one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static int AtMost(this int value, int other) => Math.Min(value, other);

        /// <summary>
        /// Get Smaller one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double AtMost(this double value, double other) => Math.Min(value, other);

        /// <summary>
        /// Get Smaller one between <paramref name="value"/> and <paramref name="other"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static decimal AtMost(this decimal value, decimal other) => Math.Min(value, other);

    }
}
