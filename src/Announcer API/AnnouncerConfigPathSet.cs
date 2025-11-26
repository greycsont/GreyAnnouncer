using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerAPI;

public class AnnouncerConfigPathSet
{
    public string[] announcerConfigPaths { get; set; } = new string[] {PathManager.GetCurrentPluginPath()};
    public int currentConfigIndex { get; set; } = 0;
}