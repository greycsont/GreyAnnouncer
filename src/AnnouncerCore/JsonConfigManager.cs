using System.IO;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerCore;

public class JsonConfigManager : IConfigManager
{
    public string ConfigPath(string directory) => Path.Combine(directory, "config.json");

    public PackConfig Load(string directory)
        => JsonManager.ReadJson<PackConfig>(ConfigPath(directory));

    public void Save(string directory, PackConfig config)
        => JsonManager.WriteJson(ConfigPath(directory), config);
}
