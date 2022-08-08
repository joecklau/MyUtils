using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyUtils.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// For Query (ReadOnly-method) which is ok to retry
        /// See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
        /// </summary>
        /// <remarks>
        /// <b>DO NOT</b> apply on non-ReadOnly method
        /// </remarks>
        /// <returns></returns>
        public static Task<HttpResponseMessage> RetryForTransientHttpErrorOnQuery(this Task<HttpResponseMessage> getTask,
            ILogger logger = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()

                ////System.Net.Http.HttpRequestException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (demo - api.ig.com:443)
                //// --->System.Net.Sockets.SocketException(10060): A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
                //.OrInner<System.Net.Sockets.SocketException>()

                //System.Threading.Tasks.TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing.
                // ---> System.TimeoutException: The operation was canceled.
                // ---> System.Threading.Tasks.TaskCanceledException: The operation was canceled.
                // ---> System.IO.IOException: Unable to read data from the transport connection: The I/O operation has been aborted because of either a thread exit or an application request..
                // ---> System.Net.Sockets.SocketException (995): The I/O operation has been aborted because of either a thread exit or an application request.
                .Or<TimeoutException>()
                .OrInner<TimeoutException>()

                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (result, wait, attempt, ctx) =>
                    {
                        string waitToRetryMsg = $"Wait {wait.TotalSeconds}s for {attempt}th retry";
                        if (result.Result != null)
                        {
                            var textContent = result.Result.Content.ReadAsStringAsync();
                            if (result.Exception is null)
                            {
                                logger?.LogWarningWithCaller($"{waitToRetryMsg} for the error when {result.Result.RequestMessage.Method} {result.Result.RequestMessage.RequestUri}. StatusCode: {result.Result.StatusCode}. Content: {textContent}");
                            }
                            else
                            {
                                logger?.LogWarningWithCaller(result.Exception, $"{waitToRetryMsg} for the error when {result.Result.RequestMessage.Method} {result.Result.RequestMessage.RequestUri}. StatusCode: {result.Result.StatusCode}. Content: {textContent}");
                            }
                            result.Result.Dispose();
                            return;
                        }

                        if (result.Exception is null)
                        {
                            logger?.LogWarningWithCaller($"{waitToRetryMsg} for the error.");
                        }
                        else
                        {
                            logger?.LogWarningWithCaller(result.Exception, $"{waitToRetryMsg} for the error.");
                        }

                    })
                .ExecuteAsync(async token =>
                {
                    token.ThrowIfCancellationRequested();
                    var responseMessage = await getTask;
                    //responseMessage.EnsureSuccessStatusCodeWithContentLog(logger);

                    return responseMessage;
                }, cancellationToken);
        }


        public static HttpResponseMessage EnsureSuccessStatusCodeWithContentLog(this HttpResponseMessage httpResponseMessage, ILogger logger)
        {
            try
            {
                return httpResponseMessage.EnsureSuccessStatusCode();
            }
            catch
            {
                try
                {
                    var contentText = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    logger?.LogError($"{httpResponseMessage.RequestMessage.Method} {httpResponseMessage.RequestMessage.RequestUri} return {httpResponseMessage.StatusCode} with content: {contentText}");
                }
                catch(Exception ex)
                {
                    logger?.LogError(ex, $"Fail to read response text for {httpResponseMessage.RequestMessage.Method} {httpResponseMessage.RequestMessage.RequestUri}");
                }

                throw;
            }
        }
    }
}
