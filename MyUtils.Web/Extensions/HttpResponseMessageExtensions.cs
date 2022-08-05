using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MyUtils.Web.Extensions
{
    public static class HttpResponseMessageExtensions
    {
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
                    logger.LogError($"{httpResponseMessage.RequestMessage.Method} {httpResponseMessage.RequestMessage.RequestUri} return {httpResponseMessage.StatusCode} with content: {contentText}");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"Fail to read response text for {httpResponseMessage.RequestMessage.Method} {httpResponseMessage.RequestMessage.RequestUri}");
                }

                throw;
            }
        }
    }
}
