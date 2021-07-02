using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace MyUtils.Web.Middlewares
{
    /// <summary>
    /// Reverse Proxy Middleware basing on auth0's version
    /// </summary>
    /// <remarks>See https://auth0.com/blog/building-a-reverse-proxy-in-dot-net-core </remarks>
    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;
        private readonly ReverseProxyOptions _options;

        /// <summary>
        /// Create a Reverse Proxy Middleware
        /// </summary>
        /// <param name="nextMiddleware">Next Middleware to be invoked (CAUTION: It won't be invoked in short-circuiting case)</param>
        /// <param name="options"></param>
        public ReverseProxyMiddleware(RequestDelegate nextMiddleware,
            ReverseProxyOptions options)
        {
            _nextMiddleware = nextMiddleware;
            _options = options;
        }

        public class ReverseProxyOptions
        {
            public readonly Dictionary<Regex, string> UrlRegexToFormatMap;
            
            public ReverseProxyOptions()
            {
                UrlRegexToFormatMap = new Dictionary<Regex, string>();
            }
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request);

            if (targetUri != null)
            {
                var targetRequestMessage = CreateTargetMessage(context, targetUri);

                using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    CopyFromTargetResponseHeaders(context, responseMessage);
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                }
                return; // return to short-circuiting
            }

            await _nextMiddleware(context);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private Uri BuildTargetUri(HttpRequest request)
        {
            Uri targetUri = null;
            Uri oriUri = GetAbsoluteUri(request, withQuery:false, withScheme:true);
            string oriUriText = oriUri.ToString().ToLower();

            foreach (var urlRegexToTargetFormat in _options.UrlRegexToFormatMap)
            {
                if (!urlRegexToTargetFormat.Key.IsMatch(oriUriText))
                {
                    continue;
                }

                var resultGroupNames = urlRegexToTargetFormat.Key.GetGroupNames();
                var resultGroups = urlRegexToTargetFormat.Key.Match(oriUriText).Groups;
                var resultGroupValues = resultGroupNames.Select(key => resultGroups[key].Value).ToArray();
                string targetUriText = string.Format(urlRegexToTargetFormat.Value, resultGroupValues);
                Uri tempTargetUri = new Uri(targetUriText);

                UriBuilder targetUriBuilder = new UriBuilder(tempTargetUri.Scheme, tempTargetUri.Host, tempTargetUri.Port);
                targetUriBuilder.Path = tempTargetUri.AbsolutePath;
                if (request.Query.Count != 0)
                {
                    targetUriBuilder.Query = request.QueryString.Value;
                }
                targetUri = targetUriBuilder.Uri;
            }

            //if (request.Path.StartsWithSegments("/googleforms", out var remainingPath))
            //{
            //    targetUri = new Uri("https://docs.google.com/forms" + remainingPath);
            //}

            return targetUri;
        }
        
        private static Uri GetAbsoluteUri(HttpRequest request, bool withQuery = true, bool withScheme = false)
        {
            UriBuilder uriBuilder = new UriBuilder();
            if (withScheme)
            {
                uriBuilder.Scheme = request.Scheme;
            }
            uriBuilder.Host = request.Host.Host;
            uriBuilder.Path = request.Path.ToString();
            if (withQuery)
            {
                uriBuilder.Query = request.QueryString.ToString();
            }

            return uriBuilder.Uri;
        }
    }

    public static class ReverseProxyMiddlewareExtensions
    {
        public static IApplicationBuilder UseReverseProxy(
            this IApplicationBuilder app, Action<ReverseProxyMiddleware.ReverseProxyOptions> configureOptions)
        {
            var options = new ReverseProxyMiddleware.ReverseProxyOptions();
            configureOptions(options);

            return app.UseMiddleware<ReverseProxyMiddleware>(options);
        }
    }
}
