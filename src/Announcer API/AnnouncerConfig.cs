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

    public Dictionary<string, CategorySetting> CategorySetting { get; } = new();

    public AnnouncerConfig()
    {
        // 如果 Ini 反序列化后再填充，这里可以后处理 <- this is a vibe-coding product but I thought It's a useful note
    }
    
    /// <summary>
    /// I strongly recommand using SetCategorySettingMap to create th dictionary of CategorySetting
    /// If you really want to add a new CategorySetting in the Dictionary
    /// PLEASE USE THIS METHOD
    /// Otherwise the AnnouncerConfig can't track the value change on that CategorySetting and blow up whole program
    /// </summary>
    /// <param name="key"></param>
    /// <param name="setting"></param>
    public void AddCategory(string key, CategorySetting setting)
    {
        CategorySetting[key] = setting;
        setting.PropertyChanged += OnCategoryChanged;
        RaiseChanged(nameof(CategorySetting));
    }


    /// <summary>
    /// Use this method if you want to initialize the dictionary
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public AnnouncerConfig SetCategorySettingMap(Dictionary<string, CategorySetting> map)
    {
        CategorySetting.Clear();

        foreach (var kv in map)
            AddCategory(kv.Key, kv.Value);

        return this;
    }

    private void OnCategoryChanged(object sender, PropertyChangedEventArgs e)
    {
        LogManager.LogDebug($"Triggerd OnCategoryChanged");
        RaiseChanged(nameof(AnnouncerConfig));
        //RaiseChanged($"Category.{e.PropertyName}");
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

    /// <summary>
    /// This MF maybe just switch to use a single string to deal it
    /// But I don't think
    /// IT COULD CHANGE IN-GAME
    /// </summary>
    private List<string> _audioFiles = new();

    [IniKey("AudioFiles")]
    public List<string> AudioFiles
    {
        get => _audioFiles;
        set => _audioFiles = value ?? new List<string>();
    }
}