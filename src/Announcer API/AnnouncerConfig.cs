using System.Collections.Generic;
using System.ComponentModel;

using GreyAnnouncer.Util.Ini;
using GreyAnnouncer.Base;

namespace GreyAnnouncer.AnnouncerAPI;

public class AnnouncerConfig : NotifyBase
{
    private bool _randomizeAudioOnPlay;
    [IniKey("RandomizeAudioOnPlay")]
    public bool RandomizeAudioOnPlay
    {
        get => _randomizeAudioOnPlay;
        set => SetField(ref _randomizeAudioOnPlay, value);
    }

    public Dictionary<string, CategorySetting> CategoryAudioMap { get; } = new();

    public AnnouncerConfig()
    {
        // 如果 Ini 反序列化后再填充，这里可以后处理
    }

    public void AddCategory(string key, CategorySetting setting)
    {
        CategoryAudioMap[key] = setting;
        setting.PropertyChanged += OnCategoryChanged;
        RaiseChanged(nameof(CategoryAudioMap));
    }

    public AnnouncerConfig SetCategoryAudioMap(Dictionary<string, CategorySetting> map)
    {
        CategoryAudioMap.Clear();

        foreach (var kv in map)
            AddCategory(kv.Key, kv.Value);

        return this;
    }

    private void OnCategoryChanged(object sender, PropertyChangedEventArgs e)
    {
        RaiseChanged($"Category.{e.PropertyName}");
    }
}

public class CategorySetting : NotifyBase
{
    private bool _enabled = true;
    [IniKey("Enabled")]
    public bool Enabled
    {
        get => _enabled;
        set => SetField(ref _enabled, value);
    }

    private float _volumeMultiplier = 1.0f;
    [IniKey("VolumeMultiplier")]
    public float VolumeMultiplier
    {
        get => _volumeMultiplier;
        set => SetField(ref _volumeMultiplier, value);
    }

    private float _cooldown = 1.5f;
    [IniKey("Cooldown")]
    public float Cooldown
    {
        get => _cooldown;
        set => SetField(ref _cooldown, value);
    }

    private List<string> _audioFiles = new();

    [IniKey("AudioFiles")]
    public List<string> AudioFiles
    {
        get => _audioFiles;
        set => _audioFiles = value ?? new List<string>();
    }
}