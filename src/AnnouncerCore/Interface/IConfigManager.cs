
namespace GreyAnnouncer.AnnouncerCore;

public interface IConfigManager
{
    public string ConfigPath(string directory);
    public PackConfig Load(string directory);
    public void Save(string directory, PackConfig config);
}
