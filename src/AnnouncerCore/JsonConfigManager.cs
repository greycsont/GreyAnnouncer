using System.IO;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerCore;

public class JsonConfigManager : IConfigManager
{
    private static string ConfigPath(string directory) => Path.Combine(directory, "config.json");

    public AnnouncerConfig Load(string directory)
        => JsonManager.ReadJson<AnnouncerConfig>(ConfigPath(directory));

    public void Save(string directory, AnnouncerConfig config)
        => JsonManager.WriteJson(ConfigPath(directory), config);
}
