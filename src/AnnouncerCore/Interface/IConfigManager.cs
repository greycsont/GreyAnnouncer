
namespace GreyAnnouncer.AnnouncerCore;

public interface IConfigManager
{
    public AnnouncerConfig Load(string directory);
    public void Save(string directory, AnnouncerConfig config);
}
