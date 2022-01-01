using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyUtils
{
    public static class LoggerExtensions
    {
        public static void LogDebugWithCaller(this ILogger logger, Func<string> generateMessage, [CallerMemberName] string caller = null) => logger.LogDebug(() => $"[{caller}] {generateMessage()}");

        public static void LogInformationWithCaller(this ILogger logger, Exception exception, string message = "Information", [CallerMemberName] string caller = null) => logger.LogInformation(exception, $"[{caller}] {message}");
        public static void LogInformationWithCaller(this ILogger logger, string message, [CallerMemberName] string caller = null) => logger.LogInformation($"[{caller}] {message}");

        public static void LogWarningWithCaller(this ILogger logger, Exception exception, string message = "Warning", [CallerMemberName] string caller = null) => logger.LogWarning(exception, $"[{caller}] {message}");
        public static void LogWarningWithCaller(this ILogger logger, string message, [CallerMemberName] string caller = null) => logger.LogWarning($"[{caller}] {message}");

        public static void LogErrorWithCaller(this ILogger logger, Exception exception, string message = "Error", [CallerMemberName] string caller = null) => logger.LogError(exception, $"[{caller}] {message}");
        public static void LogErrorWithCaller(this ILogger logger, string message, [CallerMemberName] string caller = null) => logger.LogError($"[{caller}] {message}");


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

        ///// <summary>
        ///// Log the message only-if LogLevel accept <see cref="LogLevel.Debug"/>.
        ///// It may help on skipping processing on extra debug message on Production Environment if LogLevel is at-least <see cref="LogLevel.Information"/>
        ///// </summary>
        ///// <param name="logger"></param>
        ///// <param name="generateMessage"></param>
        //public static void LogDebug<T>(this ILogger<T> logger, Func<string> generateMessage)
        //{
        //    if (logger.IsEnabled(LogLevel.Debug))
        //    {
        //        logger.LogDebugWithCaller(generateMessage());
        //    }
        //}
    }
}
