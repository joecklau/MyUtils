using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MyUtils
{
    public class Common
    {
        //public static string RuntimeDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);   // It returns something else for WorkerService project, e.g. C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\6.0.5\\System.Private.CoreLib.dll
        //public static string RuntimeDir => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);   // null for Linux
        public static string RuntimeDir => Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

    }
}
