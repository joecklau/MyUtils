using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MyUtils
{
    public static class UriExtensions
    {
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
            var oriParamDict = oriQueryString
                .Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => x.Length == 2 && !string.IsNullOrWhiteSpace(x[1]))
                .Where(pair => !paramDict.Keys.InvariantContains(pair[0]))
                .ToDictionary(pair => pair[0], pair => pair[1]);
            var combinedDict = paramDict
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Concat(oriParamDict)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return combinedDict.Any() ? $"{urlBase}?{combinedDict.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}").JoinWithSeparator("&")}" : urlBase;
        }
    }
}
