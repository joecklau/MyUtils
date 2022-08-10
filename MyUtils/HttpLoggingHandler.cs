using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyUtils
{

    /// <summary>
    /// See https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        private readonly ILogger<HttpLoggingHandler> _logger;

        public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                _logger.LogDebug(() => $"{sw.ElapsedMilliseconds:N0}ms ({sw.Elapsed}) to receive {response.StatusCode}({(int)response.StatusCode}) by {request.Method} {request.RequestUri}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug(() => $"{request.Method} {request.RequestUri} started {sw.Elapsed} ago is now cancelled");
                throw;
            }
            return response;
        }

    }

    public static class EXEX
    {
        /// <summary>
        /// An extended version of <see cref="HttpClientBuilderExtensions.AddHttpMessageHandler{THandler}(IHttpClientBuilder)"/> with duplicated handler prevention
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IHttpClientBuilder TryAddHttpMessageHandler<THandler>(this IHttpClientBuilder builder)
            where THandler : DelegatingHandler
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
            {

                options.HttpMessageHandlerBuilderActions.Add(b =>
                {
                    if (!b.AdditionalHandlers.OfType<THandler>().Any())
                    {
                        b.AdditionalHandlers.Add(b.Services.GetRequiredService<THandler>());
                    }
                });
            });

            return builder;
        }
    }

}
