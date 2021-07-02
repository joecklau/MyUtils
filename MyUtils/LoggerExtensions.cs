using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyUtils
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Log the message only-if LogLevel accept <see cref="LogLevel.Debug"/>.
        /// It may help on skipping processing on extra debug message on Production Environment if LogLevel is at-least <see cref="LogLevel.Information"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="generateMessage"></param>
        public static void LogDebug(this ILogger logger, Func<string> generateMessage)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(generateMessage());
        }

        /// <summary>
        /// Log the message only-if LogLevel accept <see cref="LogLevel.Debug"/>.
        /// It may help on skipping processing on extra debug message on Production Environment if LogLevel is at-least <see cref="LogLevel.Information"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="generateMessage"></param>
        public static void LogDebug<T>(this ILogger<T> logger, Func<string> generateMessage)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(generateMessage());
            }
        }
    }
}
