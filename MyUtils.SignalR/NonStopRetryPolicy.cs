using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyUtils.SignalR
{
    /// <summary>
    /// Wait for 2s, 30s & 60s basing on <see cref="RetryContext.PreviousRetryCount"/>
    /// </summary>
    public class NonStopRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            if (retryContext.PreviousRetryCount < 50)
            {
                return new TimeSpan(0, 0, 2);
            }
            else if (retryContext.PreviousRetryCount < 250)
            {
                return new TimeSpan(0, 0, 30);
            }
            else
            {
                return new TimeSpan(0, 1, 0);
            }
        }
    }

}
