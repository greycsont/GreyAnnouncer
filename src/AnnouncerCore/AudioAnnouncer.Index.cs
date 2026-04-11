using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.Util;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.AnnouncerCore;


public partial class AudioAnnouncer
{
    private string IndexPath
        => Path.Combine(Setting.announcersPath, title, "index.json");

    private string LoadSelectedPack()
    {
        var pack = JsonManager.ReadJson<string>(IndexPath);
        if (pack == null) {
            pack = _defaultAnnouncerConfigPath;
            JsonManager.WriteJson(IndexPath, pack);
        }
        return Path.Combine(Setting.announcersPath, title, pack);
    }

    private void SaveSelectedPack(string fullPath)
        => JsonManager.WriteJson(IndexPath, Path.GetFileName(fullPath));

    public static List<string> GetAvailablePacks(string title)
    {
        var dir = Path.Combine(Setting.announcersPath, title);
        Directory.CreateDirectory(dir);
        return Directory
            .GetDirectories(dir)
            .Select(Path.GetFileName)
            .ToList();
    }
}