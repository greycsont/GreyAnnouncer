using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace GreyAnnouncer.Util;

public static class ProcessHelper
{
    /*
     * Unity 什么时候支持 .NET Core?
     */
    [Description("Reference : (C# 判断操作系统是 Windows 还是 Linux - 青叶煮酒 - 博客园, 11/1/2022) https://www.cnblogs.com/dhqy/p/15787463.html (Accessed in 25/4/2025)")]
    public static void OpenDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                System.Diagnostics.Process.Start("explorer.exe", path);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                System.Diagnostics.Process.Start("xdg-open", path);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                System.Diagnostics.Process.Start("open", path);
            else
                LogHelper.LogWarning("Unsupported OS platform.");
        }
        else
        {
            LogHelper.LogWarning("The path is not valid or the directory does not exist.");
        }
    }

    internal static string FindExecutable(string executableName, string fallbackPath = null)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string exeName = isWindows ? executableName + ".exe" : executableName;
        char pathSeparator = isWindows ? ';' : ':';

        // Enviroment path
        var envPath = Environment.GetEnvironmentVariable(executableName);
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            var exe = Path.Combine(envPath, exeName);
            if (File.Exists(exe))
                return envPath;
        }

        // PATH variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var dirs = pathEnv.Split(pathSeparator);
            foreach (var dir in dirs)
            {
                if (string.IsNullOrWhiteSpace(dir))
                    continue;

                var exe = Path.Combine(dir, exeName);
                if (File.Exists(exe))
                    return dir;
            }
        }

        // fallback
        if (!string.IsNullOrEmpty(fallbackPath) && Directory.Exists(fallbackPath))
            return fallbackPath;

        return null;
    }
}