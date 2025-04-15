using BepInEx;
using System.IO;
using System.Reflection;

namespace greycsont.GreyAnnouncer{
    public class PathManager{
        public static string GetCurrentPluginPath(string filePath = null)
        {
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(pluginDirectory, filePath ?? string.Empty);
        }

        public static string GetGamePath(string filePath)
        {
            string gameRoot = Paths.GameRootPath;
            return Path.Combine(gameRoot, filePath);
        }
    }
}