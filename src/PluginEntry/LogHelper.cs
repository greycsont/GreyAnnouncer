using BepInEx.Logging;
using System.IO;
using System.Runtime.CompilerServices;

internal static class LogHelper
{
    internal static ManualLogSource log { get; set; }

    public static void LogInfo(object data,
        [CallerMemberName] string member = "")
    {
        log?.LogInfo($"[{member}] {data}");
    }

    public static void LogWarning(object data,
        [CallerMemberName] string member = "")
    {
        log?.LogWarning($"[{member}] {data}");
    }

    public static void LogError(object data,
        [CallerMemberName] string member = "")
    {
        log?.LogError($"[{member}] {data}");
    }

    public static void LogDebug(object data,
        [CallerMemberName] string member = "")
    {
        log?.LogDebug($"[{member}] {data}");
    }

    private static string GetClassName(string path)
        => Path.GetFileNameWithoutExtension(path);
}
