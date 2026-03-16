
using System;

namespace GreyAnnouncer.AnnouncerAPI;

public interface IAnnouncer
{
    public string title { get; }
    public string GUID { get; }
    public string announcerPath { get; set; }
    public AnnouncerConfig announcerConfig { get; }
    public void EditExternally();
    public void ReloadAudio();
    public Action syncUI { get; set; }
}
