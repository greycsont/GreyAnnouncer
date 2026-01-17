using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerAPI;

public class AnnouncerConfig
{
    public string AudioPath { get; set; } = PathManager.GetCurrentPluginPath();

    public bool RandomizeAudioOnPlay { get; set; } = false;

    public Dictionary<string, CategoryAudioSetting> CategoryAudioMap { get; set; }

}

public class CategoryAudioSetting
{
    public bool Enabled { get; set; } = true;

    public string DisplayName { get; set; }

    public float VolumeMultiplier { get; set; } = 1.0f;

    public List<string> AudioFiles  { get; set; } = new List<string>();

    public float Cooldown { get; set; } = 1.5f;

}