using BepInEx.Logging;
using System.IO;
using System.Runtime.CompilerServices;

internal static class LogHelper
{
    internal static int LogLevel = 0;
    internal static ManualLogSource log { get; set; }

    public static void LogInfo(object data, int level = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string member = "")
    {
        if (level > LogLevel) return;
        log?.LogInfo($"[{GetClassName(filePath)}.{member}] {data}");
    }

    public static void LogWarning(object data, int level = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string member = "")
    {
        if (level > LogLevel) return;
        log?.LogWarning($"[{GetClassName(filePath)}.{member}] {data}");
    }

    public static void LogError(object data,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string member = "")
    {
        log?.LogError($"[{GetClassName(filePath)}.{member}] {data}");
    }

    public static void LogDebug(object data, int level = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string member = "")
    {
        if (level > LogLevel) return;
        log?.LogDebug($"[{GetClassName(filePath)}.{member}] {data}");
    }

    private static string GetClassName(string path)
        => Path.GetFileNameWithoutExtension(path);
}
