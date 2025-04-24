using BepInEx;
using System.IO;
using System.Reflection;

namespace greycsont.GreyAnnouncer;

public class PathManager
{
    public static string GetCurrentPluginPath(string filePath = null)
    {
        string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return CleanPath(Path.Combine(pluginDirectory, filePath ?? string.Empty));
    }

    public static string GetGamePath(string filePath)
    {
        string gameRoot = Paths.GameRootPath;
        return CleanPath(Path.Combine(gameRoot, filePath));
    }

    public static string CleanPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        return path.TrimStart('\u202A');
        /* 我恨你， 当我用GPT-SOTIVS都是因为这个破东西导致一直说没找到路径,摸摸灰喉（ */
        /* Reference : (因win程序员想偷懒！竟在剪切板插入隐藏字符) https://www.bilibili.com/video/BV1ebLczjEWZ */
    }
}
