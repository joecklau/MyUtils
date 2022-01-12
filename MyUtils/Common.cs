using System.IO;
using System.Reflection;

namespace MyUtils
{
    public class Common
    {
        public static string RuntimeDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    }
}
