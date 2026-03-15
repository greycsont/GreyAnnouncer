using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreyAnnouncer.Util;

public static class FileSystemUtil
{
    public static void ValidateAndPrepareDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            LogHelper.LogWarning($"Directory not found: {directoryPath}");
            Directory.CreateDirectory(directoryPath);
        }
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
            LogHelper.LogError($"failed to copy: {e.Message}");
        }
    }
}