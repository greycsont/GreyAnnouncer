
namespace GreyAnnouncer.AnnouncerCore;

public interface IConfigManager
{
    public string ConfigPath(string directory);
    public AnnouncerConfig Load(string directory);
    public void Save(string directory, AnnouncerConfig config);
}
