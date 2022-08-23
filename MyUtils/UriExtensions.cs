using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MyUtils
{
    public static class UriExtensions
    {
        /// <summary>
        /// Skip common tracking query strings like fbclid, utm_source, utm_medium, etc.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SkipTrackingQueryStrings(this string url) => url.SkipQueryStrings(RegexOptions.IgnoreCase, "^utm_", "fbclid");

        public static string SkipQueryStrings(this string url, RegexOptions regexOptions, params string[] regexPatternsToSkip)
        {
            var urlSplits = url.Split(new[] { '?' }, 2);
            var urlBase = urlSplits[0];
            var urlQueryString = urlSplits.Length > 1 ? urlSplits[1] : string.Empty;

            var oriQueryString = urlQueryString.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries);
            // CAUTION: DO NOT use Dictionary as QueryString key can be duplicated
            var oriParams = oriQueryString
                .Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => x.Length == 2 && !string.IsNullOrWhiteSpace(x[1]))
                .Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]))
                .ToArray();
            var resultParams = oriParams
                .Where(x => !regexPatternsToSkip.Any(pattern => Regex.IsMatch(x.Key, pattern, regexOptions)))
                .ToArray();

            return resultParams.Any() ? $"{urlBase}?{resultParams.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}").JoinWithSeparator("&")}" : urlBase;
        }

        /// <summary>
        /// Consider <see cref="Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString()"/> cannot handle special character ';', it's our implementation to handle it.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramDict"></param>
        /// <returns></returns>
        public static string AddQueryString(this string url, Dictionary<string, string> paramDict)
        {
            var urlSplits = url.Split(new[] { '?' }, 2);
            var urlBase = urlSplits[0];
            var urlQueryString = urlSplits.Length > 1 ? urlSplits[1] : string.Empty;

            var oriQueryString = urlQueryString.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries);
            // CAUTION: DO NOT use Dictionary as QueryString key can be duplicated
            var oriParams = oriQueryString
                .Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => x.Length == 2 && !string.IsNullOrWhiteSpace(x[1]))
                .Where(pair => !paramDict.Keys.InvariantContains(pair[0]))
                .Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]))
                .ToArray();
            var resultParams = paramDict
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Concat(oriParams)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return resultParams.Any() ? $"{urlBase}?{resultParams.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}").JoinWithSeparator("&")}" : urlBase;
        }

        public static string UrlCombine(this string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }
    }
}
