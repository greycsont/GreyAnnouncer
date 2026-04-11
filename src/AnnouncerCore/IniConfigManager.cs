using System.IO;

using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public class IniConfigManager : IConfigManager
{
    private static string ConfigPath(string directory) => Path.Combine(directory, "config.ini");

    public AnnouncerConfig Load(string directory)
    {
        var path = ConfigPath(directory);
        if (!File.Exists(path))
            return null;

        var doc = IniReader.Read(path);
        return AnnouncerIniMapper.FromIni(doc);
    }

    public void Save(string directory, AnnouncerConfig config)
    {
        var path = ConfigPath(directory);
        var doc = AnnouncerIniMapper.ToIni(new IniDocument(), config);
        IniWriter.Write(path, doc);
    }
}
