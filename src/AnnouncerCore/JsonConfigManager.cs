using System.IO;

using Newtonsoft.Json;

namespace GreyAnnouncer.AnnouncerAPI;

public class JsonConfigManager : IConfigManager
{
    private static string ConfigPath(string directory) => Path.Combine(directory, "config.json");

    public AnnouncerConfig Load(string directory)
    {
        var path = ConfigPath(directory);
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<AnnouncerConfig>(json);
    }

    public void Save(string directory, AnnouncerConfig config)
    {
        var path = ConfigPath(directory);
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}
