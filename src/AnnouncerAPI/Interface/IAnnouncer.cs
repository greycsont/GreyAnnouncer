using GreyAnnouncer.Config;

namespace GreyAnnouncer.AnnouncerAPI;

public interface IAnnouncer
{
    string title { get; }
    string GUID { get; }
    string announcerPath { get; set; }
    AnnouncerConfig announcerConfig { get; }
    void EditExternally();
}
