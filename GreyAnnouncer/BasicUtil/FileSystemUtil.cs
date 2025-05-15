using System.IO;



namespace greycsont.GreyAnnouncer;

public static class FileSystemUtil
{
    public static void ValidateAndPrepareDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Plugin.log.LogWarning($"Directory not found: {directoryPath}");
            Directory.CreateDirectory(directoryPath);
        }
    }
}