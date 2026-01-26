
using System;
using System.IO;
using System.Collections.Generic;

using GreyAnnouncer.Util;


namespace GreyAnnouncer.AnnouncerAPI;

public static class AnnouncerIndex
{
    private static string indexPath = PathHelper.GetCurrentPluginPath("index.json");
    
    private static readonly Dictionary<string, string> _data = Read();

    private static string _announcersPath;

    public static string announcersPath
    {
        get
        {
            if (_announcersPath == null)
            {
                _announcersPath = PathHelper.GetCurrentPluginPath("announcers");
            }
            return _announcersPath;
        }
        set
        {
            _announcersPath = value;
        }
    }

    public static void Set(string guid, string fullPath)
    {
        string folder = announcersPath.EndsWith(Path.DirectorySeparatorChar.ToString()) 
                    ? announcersPath 
                    : announcersPath + Path.DirectorySeparatorChar;

        Uri pathUri = new Uri(fullPath);
        Uri folderUri = new Uri(folder);
        
        string relativePath = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString())
                                 .Replace('/', Path.DirectorySeparatorChar);

        _data[guid] = relativePath;
        Save();
    }

    public static string Get(string guid, string defaultAnnouncerRelativePath)
    {
        if (_data.TryGetValue(guid, out var relativePath))
        {
            return Path.Combine(announcersPath, relativePath);
        }

        _data[guid] = Path.Combine(announcersPath, defaultAnnouncerRelativePath);
        Save();

        return Path.Combine(announcersPath, defaultAnnouncerRelativePath);
    }
    public static bool Remove(string guid)
    {
        if (_data.Remove(guid))
        {
            Save();
            return true;
        }
        return false;
    }

    private static Dictionary<string,string> Read()
    {
        Dictionary<string, string> dict = null;

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
    {
        JsonManager.WriteJson(indexPath, _data);
    }
}
