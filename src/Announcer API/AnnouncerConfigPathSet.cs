using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerAPI;

public class AnnouncerConfiggPathSet
{
    public string[] announcerConfigPaths { get; set; } = new string[] { PathManager.GetCurrentPluginPath() };
    public int currentConfigIndex { get; set; } = 0;
}
