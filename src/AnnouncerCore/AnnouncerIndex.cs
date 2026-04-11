
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.Util;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.AnnouncerAPI;

public static class AnnouncerIndex
{
    private static string _indexPath = PathHelper.GetCurrentPluginPath("index.json");
    
    private static readonly Dictionary<string, string> _data = Read();

    public static void Set(string title, string fullPath)
    {
        var targetAnnouncerPath = Path.Combine(Setting.announcersPath, title);
        
        string folder = targetAnnouncerPath.EndsWith(Path.DirectorySeparatorChar.ToString()) 
                    ? targetAnnouncerPath 
                    : targetAnnouncerPath + Path.DirectorySeparatorChar;

        Uri pathUri = new Uri(fullPath);
        Uri folderUri = new Uri(folder);
        
        string relativePath = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString())
                                 .Replace('/', Path.DirectorySeparatorChar);

        _data[title] = relativePath;
        Save();
    }

    public static string Get(string title, string defaultAnnouncerRelativePath)
    {
        if (_data.TryGetValue(title, out var relativePath))
        {
            return Path.Combine(Setting.announcersPath, title, relativePath);
        }

        _data[title] = defaultAnnouncerRelativePath;
        Save();

        return Path.Combine(Setting.announcersPath, title, defaultAnnouncerRelativePath);
    }

    // No One uses this right?
    // RIGHT??
    public static bool Remove(string title)
    {
        if (_data.Remove(title))
        {
            Save();
            return true;
        }
        return false;
    }

    private static Dictionary<string,string> Read()
    {
        Dictionary<string, string> dict;

        try
        {
            dict = JsonManager.ReadJson<Dictionary<string,string>>("index.json");
        }
        catch (FileNotFoundException)
        {
            JsonManager.WriteJson("index.json", new Dictionary<string,string>());
            dict = JsonManager.ReadJson<Dictionary<string,string>>("index.json");
        }

        return dict;
    }

    private static void Save()
        => JsonManager.WriteJson(_indexPath, _data);

    public static List<string> GetAnnouncers()
    {
        return Directory
            .GetDirectories(Setting.announcersPath)
            .Where(p => File.Exists(Path.Combine(p, "config.ini")))
            .Select(Path.GetFileName)
            .ToList();
    }

    public static List<string> GetTargetAnnouncer(string title)
    {
        return Directory
            .GetDirectories(Path.Combine(Setting.announcersPath, title))
            .Where(p => File.Exists(Path.Combine(p, "config.ini")))
            .Select(Path.GetFileName)
            .ToList();
    }
}
