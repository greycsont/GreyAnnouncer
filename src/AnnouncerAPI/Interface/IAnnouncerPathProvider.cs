using GreyAnnouncer.Config;

namespace GreyAnnouncer.AnnouncerAPI;

public interface IAnnouncerProvider
{
    string title { get; }
    string GUID { get; }
    string announcerPath { get; set; }
    AnnouncerConfig announcerConfig { get; }
    void EditExternally();
}
