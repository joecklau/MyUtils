using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyUtils
{
    public static class ReflectionUtil
    {
        /// <summary>
        /// Get caller function's name. See https://stackoverflow.com/a/38098931/4684232
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetCaller([CallerMemberName] string caller = null) => caller;

        /// <summary>
        /// Refer to https://stackoverflow.com/a/231536/4684232
        /// </summary>
        static public void ParallelInvoke(this EventHandler @event, object sender, EventArgs e)
        {
            @event?.GetInvocationList().AsParallel().ForAll(d => d.DynamicInvoke(sender, e)); //?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Refer to https://stackoverflow.com/a/231536/4684232
        /// </summary>
        static public void ParallelInvoke<T>(this EventHandler<T> @event, object sender, T e)
            //where T : EventArgs
        {
            @event?.GetInvocationList().AsParallel().ForAll(d => d.DynamicInvoke(sender, e)); //?.Invoke(this, new EventArgs());
        }

        static public bool TryParallelInvoke<T>(this EventHandler<T> @event, object sender, T e, ILogger logger, [CallerMemberName] string caller = null)
        {
            try
            {
                @event?.ParallelInvoke(sender, e);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogErrorWithCaller(ex, caller: caller);
                return false;
            }
        }

        static public bool TryInvoke<T>(this EventHandler<T> @event, object sender, T e, ILogger logger, [CallerMemberName] string caller = null)
        {
            try
            {
                @event?.Invoke(sender, e);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogErrorWithCaller(ex, caller: caller);
                return false;
            }
        }

        static public bool TryParallelInvoke(this EventHandler @event, object sender, ILogger logger, [CallerMemberName] string caller = null)
        {
            try
            {
                @event?.ParallelInvoke(sender, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogErrorWithCaller(ex, caller: caller);
                return false;
            }
        }

        static public bool TryInvoke(this EventHandler @event, object sender, ILogger logger, [CallerMemberName] string caller = null)
        {
            try
            {
                @event?.Invoke(sender, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogErrorWithCaller(ex, caller: caller);
                return false;
            }
        }
    }
}
