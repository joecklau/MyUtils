using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MyUtils
{
    public static class TextUtil
    {
        /// <summary>
        /// Shorthand of <see cref="string.Format(string, object[])"/>
        /// </summary>
        /// <param name="template"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string template, params object[] args) => string.Format(template, args);

        /// <summary>
        /// https://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<string> WholeChunks(this string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, chunkSize);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxChunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<string> ChunksUpto(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        /// <summary>
        /// Equal to SQL LEFT, take left N characters
        /// </summary>
        /// <param name="src"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string src, int length) => 
            src is null ? null : 
            src.Length <= length ? src :
            src.Substring(0, length);

        /// <summary>
        /// Equal to SQL RIGHT, take right N characters
        /// </summary>
        /// <param name="src"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string src, int length) =>
            src is null ? null :
            src.Length <= length ? src : 
            src.Substring(src.Length - length, length);

        /// <summary>
        /// 10000.0m = 10,000
        /// 10000.1m = 10,000.10
        /// </summary>
        /// <param name="rootUri"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        public static string ToMoney(this decimal numeric)
        {
            return "$" + numeric.ToString("N").Replace(".00", "");
        }

        /// <summary>
        /// Combine the root URI with relative URI. The returned new uri will be {rootUri}/{relativeUri}
        /// </summary>
        /// <param name="rootUri"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        public static string UriPathCombine(this string rootUri, string relativeUri)
        {
            if (String.IsNullOrWhiteSpace(relativeUri))
            {
                return rootUri;
            }
            else if (String.IsNullOrWhiteSpace(rootUri))
            {
                return relativeUri;
            }
            return $"{rootUri.TrimEnd('/')}/{relativeUri.Trim('/')}";
        }

        /// <summary>
        /// Join msg with separator
        /// </summary>
        /// <param name="textList"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string JoinWithSeparator(this IEnumerable<string> textList, string separator = ",")
        {
            //return String.Join($"{separator}", textList).Replace($"{separator}{separator}", separator).TrimEnd();
            return String.Join($"{separator}", textList);
        }

        /// <summary>
        /// Return Default value if text is null or whitespace
        /// </summary>
        /// <param name="text"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string DefaultIfWhitespace(this string text, string defaultValue)
        {
            return String.IsNullOrWhiteSpace(text) ? defaultValue : text;
        }

        /// <summary>
        /// Support String.compare with different StringComparison
        /// </summary>
        /// <remarks>See http://stackoverflow.com/a/17563994/4684232 </remarks>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Tell if the <paramref name="text"/> contains whole word.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wholeWord"></param>
        /// <param name="matchCase"></param>
        /// <returns></returns>
        public static bool ContainsWholeWord(this string text, string wholeWord, bool matchCase = false) =>
            text != null &&
            wholeWord != null &&
            Regex.IsMatch(text, $"\\b{Regex.Escape(wholeWord)}\\b", matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);

        /// <summary>
        /// Support String.compare with different StringComparison
        /// </summary>
        /// <remarks>See https://stackoverflow.com/a/3947156/4684232 </remarks>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this IEnumerable<string> list, string keyword, StringComparison comp)
        {
            return list.Any(s => s.Equals(keyword, comp));
        }

        public static bool AnyStartsWith(this IEnumerable<string> list, string keyword, StringComparison comp)
        {
            return list.Any(s => s.StartsWith(keyword, comp));
        }

        /// <summary>
        /// Trim the string to null
        /// </summary>
        /// <param name="source"></param>
        /// <param name="trimChars"></param>
        /// <returns></returns>
        public static string TrimToNull(this string source, params char[] trimChars) => source?.Trim(trimChars).DefaultIfWhitespace(null);

        /// <summary>
        /// Trim the string to null
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string TrimToNull(this string source) => source?.Trim().DefaultIfWhitespace(null);

        /// <summary>
        /// Trim the string to empty (even the string is null)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string TrimToEmpty(this string source)
        {
            if (source == null)
            {
                return "";
            }
            return source.Trim();
        }

        /// <summary>
        /// See https://stackoverflow.com/a/7170953/4684232
        /// </summary>
        /// <param name="input"></param>
        /// <param name="suffixToRemove"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        public static string TrimStart(this string input, string prefixToRemove, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            if (prefixToRemove != null && input.StartsWith(prefixToRemove, comparisonType))
            {
                return input.Substring(prefixToRemove.Length, input.Length - prefixToRemove.Length);
            }

            return input;
        }

        public static string TrimEndRecursive(this string input, params string[] suffixesToRemove)
        {
            string result = input.ToString();
            foreach (var suffixToRemove in suffixesToRemove.WhereNotNull())
            {
                result = result.TrimEnd(suffixToRemove, StringComparison.InvariantCultureIgnoreCase);
            }

            if (string.Equals(result, input))
            {
                return result;
            }

            return TrimEndRecursive(result, suffixesToRemove);
        }

        /// <summary>
        /// Replace invalid chars found in <see cref="Path.GetInvalidFileNameChars()"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newChar"></param>
        /// <returns></returns>
        public static string ReplaceInvalidFilename(this string source, char newChar = '_') => source.Replace(Path.GetInvalidFileNameChars(), newChar);

        /// <summary>
        /// Replace all <paramref name="charsToReplace"/> by <paramref name="newChar"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="charsToReplace"></param>
        /// <param name="newChar"></param>
        /// <returns></returns>
        public static string Replace(this string source, char[] charsToReplace, char newChar)
        {
            if (source == null)
            {
                return string.Empty;
            }
            foreach (var c in charsToReplace)
            {
                source = source.Replace(c, newChar);
            }

            return source;
        }

        /// <summary>
        /// Remove the targeted value from the string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string Remove(this string source, string target)
        {
            if (source == null)
            {
                return string.Empty;
            }
            return source.Replace(target, string.Empty);
        }

        /// <summary>
        /// Remove the targeted values from the string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public static string Remove(this string source, params string[] targets)
        {
            string result = source;
            foreach (string target in targets)
            {
                result = result.Remove(target);
            }

            return result;
        }
        
        /// <summary>
        /// Compares 2 strings with invariant culture and case ignored
        /// </summary>
        /// <param name="compare">The compare.</param>
        /// <param name="compareTo">The compare to.</param>
        /// <returns></returns>
        public static bool InvariantEquals(this string compare, string compareTo)
        {
            return string.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantStartsWith(this string compare, string compareTo) =>
            string.IsNullOrWhiteSpace(compare) || string.IsNullOrWhiteSpace(compareTo) ? false : compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

        public static bool InvariantEndsWith(this string compare, string compareTo) =>
            string.IsNullOrWhiteSpace(compare) || string.IsNullOrWhiteSpace(compareTo) ? false : 
            compare.EndsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

        public static bool InvariantContains(this string compare, string compareTo) => 
            string.IsNullOrWhiteSpace(compare) || string.IsNullOrWhiteSpace(compareTo) ? false : 
            compare.IndexOf(compareTo, StringComparison.OrdinalIgnoreCase) >= 0;

        public static bool InvariantContains(this IEnumerable<string> compare, string compareTo) =>
            compare == null || !compare.Any() ? false : 
            compare.Contains(compareTo, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// How do I read an attribute on a class at runtime?
        /// See https://stackoverflow.com/a/2656211
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="type"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }

        /// <summary>
        /// Ensure MemberExpression is retrieved even when the input is UnaryExpression.
        /// See https://stackoverflow.com/a/12975480/4684232
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> exp)
        {
            var member = exp.Body as MemberExpression;
            var unary = exp.Body as UnaryExpression;
            return member ?? (unary != null ? unary.Operand as MemberExpression : null);
        }

        /// <summary>
        /// To get the DisplayAttribute from the passed in lambda.
        /// See https://stackoverflow.com/a/28569628/4684232
        /// See https://stackoverflow.com/a/32808219/4684232
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string DisplayNameFor<T>(Expression<Func<T, object>> property) where T : class
        {
            MemberExpression body = GetMemberExpression(property);// (MemberExpression)property.Body;
            var dd = body.Member.GetCustomAttribute<DisplayAttribute>();
            return dd?.Name.DefaultIfWhitespace(body.Member.Name);
            //if (dd != null && !String.IsNullOrWhiteSpace(dd.Name))
            //{
            //    return dd.Name;
            //}
            //else
            //{
            //    return body.Member.Name;
            //}
        }

        /// <summary>
        /// Modified from https://stackoverflow.com/a/4405876/4684232
        /// Change to return null/empty string instead of throwing exception
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                //case null: throw new ArgumentNullException(nameof(input));
                //case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                case null: return null;
                case "": return "";
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}
