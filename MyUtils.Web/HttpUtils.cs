using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyUtils;

namespace MyUtils.Web
{
    public static class HttpUtils
    {
        /// <summary>
        /// Resolve the real destination Url no matter HTTP or HTTPS.
        /// Designed to handle change that fotnet core changed the AllowAutoRedirect behaviour for HTTPS-to-HTTP as ".NET Core versions 1.0, 1.1 and 2.0 will not follow a redirection from HTTPS to HTTP even if AllowAutoRedirect is set to true."
        /// </summary>
        /// <param name="requestUriString"></param>
        /// <param name="redirectLimit">Avoid too many redirections</param>
        /// <param name="previouslyResolvedUrls">Avoid infinite loop for A-B-C-...-A or A-A redirection loop cases</param>
        /// <returns></returns>
        public static async Task<string> GetDestinationUrlAsync(string requestUriString, Action<HttpRequestHeaders> requestHeadersConfig = null, CancellationToken stoppingToken = default, int redirectLimit = 100, ICollection<string> previouslyResolvedUrls = null)
        {
            redirectLimit--;
            if (string.IsNullOrWhiteSpace(requestUriString) || redirectLimit < 0)
            {
                return requestUriString;
            }

            using (var httpClientHandler = new HttpClientHandler() { AllowAutoRedirect = false })
            using (var httpClient = new HttpClient(httpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(15),  // Instead of default 100s, which is too long, to avoid one website blocking others, shorten the Timeout.
            })
            {
                requestHeadersConfig?.Invoke(httpClient.DefaultRequestHeaders);
                var requestUri = new Uri(requestUriString, UriKind.Absolute);
                using (var response = await httpClient.GetAsync(requestUriString, stoppingToken))
                {
                    var responseUri = response.Headers?.Location?.ToString();
                    if (!string.IsNullOrWhiteSpace(responseUri))
                    {
                        if (responseUri.StartsWith("//"))
                        {
                            // Handle case like http://apple.co/MrCorma to location: //tv.apple.com/show/mr-corman/umc.cmc.2iams7vr2o0i6mtb6wm6r9q1n?at=1010lGbf&ct=atvp_mrcorman_gen_gen&itscg=80098&itsct=atvp_mrcorman_gen_gen
                            responseUri = $"{requestUri.Scheme}:{responseUri}";
                        }
                        else if (responseUri.StartsWith("/"))
                        {
                            responseUri = $"{requestUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{responseUri}";
                        }
                    }

                    if ((int)response.StatusCode > 300 &&
                        (int)response.StatusCode <= 399 &&
                        !string.IsNullOrWhiteSpace(responseUri) &&
                        !string.Equals(requestUriString, responseUri, StringComparison.InvariantCultureIgnoreCase))
                    {
                        previouslyResolvedUrls = previouslyResolvedUrls ?? new HashSet<string>();
                        if (previouslyResolvedUrls.Any(url => string.Equals(url, responseUri, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return responseUri;
                        }

                        previouslyResolvedUrls.Add(responseUri);
                        return await GetDestinationUrlAsync(responseUri, requestHeadersConfig, stoppingToken, redirectLimit, previouslyResolvedUrls);
                    }
                }
            }

            return requestUriString;
        }
    }
}
