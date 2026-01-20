
using System.IO;
using System.Collections.Generic;

using GreyAnnouncer.Util;


namespace GreyAnnouncer.AnnouncerAPI;

public static class AnnouncerIndex
{
    private static string indexPath = PathManager.GetCurrentPluginPath("index.json");
    private static readonly Dictionary<string, string> _data = Read();

    private static string _announcersPath;

    public static string announcersPath
    {
        get
        {
            if (_announcersPath == null)
            {
                _announcersPath = PathManager.GetCurrentPluginPath("announcers");
            }
            return _announcersPath;
        }
        set
        {
            _announcersPath = value;
        }
    }

    // 后续修改announcersPath时需要同时变更这个
    private static string defaultAnnouncerPath
    {
        get => Path.Combine(announcersPath, "greythroat");
    }

    public static void Set(string guid, string path)
    {
        _data[guid] = path;
        Save();
    }

    public static string Get(string guid)
    {
        if (_data.TryGetValue(guid, out var path))
            return path;

        // 第一次访问：使用 fallback
        _data[guid] = defaultAnnouncerPath;
        Save();

        return defaultAnnouncerPath;
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
        }

        return dict;
    }

    private static void Save()
    {
        JsonManager.WriteJson(indexPath, _data);
    }
}
