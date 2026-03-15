using BepInEx;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Linq;


namespace GreyAnnouncer.Util;

public static class PathHelper
{
    public static string GetCurrentPluginPath(params string[] paths)
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        return CleanPath(
            paths == null || paths.Length == 0
                ? baseDir
                : Path.Combine(baseDir, Path.Combine(paths))
        );
    }

    public static string GetGamePath(string filePath)
        => CleanPath(Path.Combine(Paths.GameRootPath, filePath));


    [Description("Reference : (因win程序员想偷懒! 竟在剪切板插入隐藏字符) https://www.bilibili.com/video/BV1ebLczjEWZ (Accessed in 24/4/2025)")]
    public static string CleanPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        string originalPath = path;
        char[] directionalChars = { '\u202A', '\u202B', '\u202C', '\u202D', '\u202E' };
        string cleanedPath = path.TrimStart(directionalChars);

        if (!originalPath.Equals(cleanedPath))
        {
            LogHelper.LogInfo($"[CleanPath] Path cleaned: Original='{originalPath}', Cleaned='{cleanedPath}'");
        }

        return cleanedPath;
        /* 我恨你， 当我用GPT-SOTIVS都是因为这个破东西导致一直说没找到路径,摸摸灰喉（ */
    }

    public static string GetFile(string filePath, string fileName)
    {
        string combinedPath = Path.Combine(filePath, fileName);

        if (File.Exists(combinedPath))
            return combinedPath;
            
        return null;
    }

    public static bool TryGetExtension(string filePath, out string foundExtension, params string[] extensions)
    {
        foundExtension = null;
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        var exts = extensions.SelectMany(e => e.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            .Select(e => e.Trim())
                            .ToArray();

        string path = filePath.Trim();
        while (!string.IsNullOrEmpty(path))
        {
            var ext = Path.GetExtension(path)?.Trim();
            if (string.IsNullOrEmpty(ext)) break;
            if (exts.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            {
                foundExtension = ext;
                return true;
            }
            path = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));
        }
        return false;
    }
}
