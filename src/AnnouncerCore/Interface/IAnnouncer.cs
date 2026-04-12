
using System;

namespace GreyAnnouncer.AnnouncerCore;

public interface IAnnouncer
{
    public string title { get; }
    public string announcerPath { get; set; }
    public PackConfig announcerConfig { get; }
    public void EditExternally();
    public void ReloadPack();
    public Action syncUI { get; set; }
    public bool isConfigLoaded { get;}
    public string configMismatchInfo { get; }
}
