using System;
using System.Collections.Generic;
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
    }
}
