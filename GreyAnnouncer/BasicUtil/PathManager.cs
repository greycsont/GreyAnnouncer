using BepInEx;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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

    [Description("Reference : (因win程序员想偷懒！竟在剪切板插入隐藏字符) https://www.bilibili.com/video/BV1ebLczjEWZ (Accessed in 24/4/2025)")]
    public static string CleanPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        return path.TrimStart('\u202A');
        /* 我恨你， 当我用GPT-SOTIVS都是因为这个破东西导致一直说没找到路径,摸摸灰喉（ */
    }

    [Description("Reference : (C# 判断操作系统是 Windows 还是 Linux - 青叶煮酒 - 博客园, 11/1/2022) https://www.cnblogs.com/dhqy/p/15787463.html (Accessed in 25/4/2025)")]
    public static void OpenDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                System.Diagnostics.Process.Start("xdg-open", path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Diagnostics.Process.Start("open", path);
            }
            else
            {
                Plugin.Log.LogWarning("Unsupported OS platform.");
            }
        }
        else
        {
            Plugin.Log.LogWarning("The path is not valid or the directory does not exist.");
        }
    }
}
