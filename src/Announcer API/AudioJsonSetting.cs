using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerAPI;

// shitest name ever named
public class AnnouncerJsonSetting
{
    public Dictionary<string, CategoryAudioSetting> CategoryAudioMap { get; set; }
}

public class CategoryAudioSetting
{
    public bool         Enabled     { get; set; } = true;
    public string       DisplayName { get; set; }
    public float        Pitch       { get; set; } = 1.0f;
    public List<string> AudioFiles  { get; set; } = new List<string>();

}