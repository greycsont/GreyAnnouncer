using System.Collections.Generic;

using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public class AnnouncerConfig
{
    [IniKey("RandomizeAudioOnPlay")]
    public bool RandomizeAudioOnPlay { get; set; } = false;

    public Dictionary<string, CategorySetting> CategoryAudioMap { get; set; } = new Dictionary<string, CategorySetting>();

}

public class CategorySetting
{
    [IniKey("Enabled")]
    public bool Enabled { get; set; } = true;

    [IniKey("VolumeMultiplier")]
    public float VolumeMultiplier { get; set; } = 1.0f;

    [IniKey("AudioFiles")]
    public List<string> AudioFiles { get; set; } = new List<string>();

    [IniKey("Cooldown")]
    public float Cooldown { get; set; } = 1.5f;

}