using System.IO;
using System.Reflection;

namespace greycsont.GreyAnnouncer{
    public class PathManager{
        public static string GetCurrentPluginPath(string filePath)
        {
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string targetPath = Path.Combine(pluginDirectory, filePath);
            return targetPath;
        }
    }
}