using Microsoft.Extensions.Logging;
using MyUtils;
using Polly;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyUtils
{
    public static class RetryPolicyHelper
    {
        /// <summary>
        /// For GET-method which is ok to retry. Specific to handle Exception types throw from <see cref="HttpClient"/>
        /// </summary>
        /// <remarks>
        /// <b>DO NOT</b> apply on non-GET method
        /// </remarks>
        /// <returns></returns>
        [Obsolete("Should be replaced by HttpResponseMessageExtensions.RetryForTransientHttpErrorOnQuery()")]
        public static Task<T> RetryPolicyForHttpQuery<T>(this Task<T> getTask,
            ILogger logger = null,
            string actionMsgToLog = null,
            System.Threading.CancellationToken cancellationToken = default,
            [CallerMemberName] string caller = null)
        {
            return Polly.Policy<T>
                //.Handle<Exception>(ex => ex is not TaskCanceledException and not OperationCanceledException and not UnauthorizedAccessException)

                // Refer to HttpPolicyExtensions.HandleTransientHttpError()
                .Handle<HttpRequestException>()

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

                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (result, wait, attempt, ctx) =>
                    {
                        string waitToRetryMsg = $"Wait {wait.TotalSeconds}s for {attempt}th retry";
                        if (result.Exception is null)
                        {
                            logger?.LogWarningWithCaller($"{waitToRetryMsg} for the error on {actionMsgToLog}.", caller: caller);
                        }
                        else
                        {
                            logger?.LogWarningWithCaller(result.Exception, $"{waitToRetryMsg} for the error on {actionMsgToLog}.", caller: caller);
                        }

                    })
                .ExecuteAsync(async token =>
                {
                    token.ThrowIfCancellationRequested();
                    return await getTask;
                }, cancellationToken);
        }
    }
}
