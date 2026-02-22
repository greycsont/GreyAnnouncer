using BepInEx;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;


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
                LogHelper.LogWarning("[OpenDirectory] Unsupported OS platform.");
        }
        else
        {
            LogHelper.LogWarning("[OpenDirectory] The path is not valid or the directory does not exist.");
        }
    }

    
    public static string GetFile(string filePath, string fileName)
    {
        string combinedPath = Path.Combine(filePath, fileName);

        if (File.Exists(combinedPath))
            return combinedPath;
            
        return null;
    }

    internal static string FindExecutable(string envVariable, string fallbackPath = null)
    {
        // Enviroment path
        var envPath = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            var exe = Path.Combine(envPath, envVariable + ".exe");
            if (File.Exists(exe))
                return envPath;
        }

        // PATH variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var dirs = pathEnv.Split(';');
            foreach (var dir in dirs)
            {
                if (string.IsNullOrWhiteSpace(dir))
                    continue;

                var exe = Path.Combine(dir, envVariable + ".exe");
                if (File.Exists(exe))
                    return dir;
            }
        }

        // fallback
        if (!string.IsNullOrEmpty(fallbackPath) && Directory.Exists(fallbackPath))
            return fallbackPath;

        return null;
    }


    public static void EnsureDirectoryExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public static void CopyDirectoryParallel(string source, string targetRoot)
    {
        try
        {
            string dirName = Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            
            string destinationPath = Path.Combine(targetRoot, dirName);

            var stack = new Stack<(string s, string t)>();
            stack.Push((source, destinationPath));

            while (stack.Count > 0)
            {
                var (currSrc, currDest) = stack.Pop();

                if (!Directory.Exists(currDest))
                {
                    Directory.CreateDirectory(currDest);
                }

                string[] files = Directory.GetFiles(currSrc);
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (file) =>
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(currDest, fileName);
                    File.Copy(file, destFile, true);
                });

                foreach (var dir in Directory.GetDirectories(currSrc))
                {
                    string subDirName = Path.GetFileName(dir);
                    stack.Push((dir, Path.Combine(currDest, subDirName)));
                }
            }
        }
        catch (Exception e)
        {
            LogHelper.LogError($"[CopyDirectory] failed to copy: {e.Message}");
        }
    }
}
