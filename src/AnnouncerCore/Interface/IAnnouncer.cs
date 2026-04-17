
using System;

namespace GreyAnnouncer.AnnouncerCore;

public interface IAnnouncer
{
    public string title { get; }
    public string packPath { get; set; }
    public PackConfig packConfig { get; }
    public void EditExternally();
    public void ReloadPack();
    public Action syncUI { get; set; }
    public bool isConfigLoaded { get;}
    public string configMismatchInfo { get; }
}
