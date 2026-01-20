using System.IO;


namespace GreyAnnouncer.Util;

public static class FileSystemUtil
{
    public static void ValidateAndPrepareDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            LogManager.LogWarning($"Directory not found: {directoryPath}");
            Directory.CreateDirectory(directoryPath);
        }
    }
}