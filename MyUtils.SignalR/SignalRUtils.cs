using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace MyUtils.SignalR
{
    public static class SignalRUtils
    {
        /// <summary>
        /// Used to replace/wrap <see cref="HubConnection.StartAsync(CancellationToken)"/> because 
        /// <see cref="HubConnectionBuilderExtensions.WithAutomaticReconnect(IHubConnectionBuilder)"/> won't configure the HubConnection to retry initial start failures, so start failures need to be handled manually.
        /// Enhanced basing on <![CDATA[https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client?view=aspnetcore-3.1&tabs=visual-studio#automatically-reconnect]]>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="canConnect">Whether we should start the connection attempt</param>
        /// <param name="exceptionHandler">Allow external action to handle exception</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> ConnectWithRetryAsync(this HubConnection connection, Func<bool> canConnect = null, Action<Exception> exceptionHandler = null, CancellationToken token = default)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                if (canConnect?.Invoke() is false)
                {
                    await Task.Delay(100);
                    continue;
                }

                try
                {
                    await connection.StartAsync(token);
                    Debug.Assert(connection.State == HubConnectionState.Connected);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    // Failed to connect, trying again in 5000 ms.
                    Debug.Assert(connection.State == HubConnectionState.Disconnected);
                    exceptionHandler?.Invoke(ex);
                    await Task.Delay(5000);
                }
            }
        }

    }
}
