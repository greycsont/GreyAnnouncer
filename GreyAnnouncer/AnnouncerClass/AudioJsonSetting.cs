using System.Collections.Generic;

namespace greycsont.GreyAnnouncer;

public class AnnouncerJsonSetting
{
    public Dictionary<string, CategoryAudioSetting> CategoryAudioMap { get; set; }
}

public class CategoryAudioSetting
{
    public bool         Enabled    { get; set; } = true;
    public List<string> AudioFiles { get; set; } = new List<string>();
}