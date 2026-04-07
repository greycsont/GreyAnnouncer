using System.Collections.Generic;
using System.Linq;

using GreyAnnouncer.Util.Ini;
using GreyAnnouncer.Base;

namespace GreyAnnouncer.AnnouncerAPI;

/// <summary>
/// Holds the full configuration for a single announcer instance,
/// including general settings and per-category audio settings.
/// Implements <see cref="NotifyBase"/> to propagate change notifications
/// to subscribers such as the UI and audio loader.
/// </summary>
public class AnnouncerConfig : NotifyBase
{
    private bool _randomizeAudioOnPlay;

    /// <summary>
    /// When true, a random category will be selected on each playback
    /// instead of using the requested category directly.
    /// </summary>
    [IniKey("RandomizeAudioOnPlay")]
    public bool RandomizeAudioOnPlay
    {
        get => _randomizeAudioOnPlay;
        set => SetField(ref _randomizeAudioOnPlay, value);
    }

    /// <summary>
    /// Per-category settings keyed by category name.
    /// Use <see cref="AddCategory"/> or <see cref="SetCategorySettingMap"/> to modify —
    /// direct dictionary manipulation will bypass change tracking.
    /// </summary>
    public ObservableDictionary<string, CategorySetting> CategorySetting { get; } = new();

    /// <param name="announcerGUID">
    /// The unique identifier for this announcer. Must match the GUID
    /// used when registering with <see cref="AnnouncerIndex"/>.
    /// </param>
    public AnnouncerConfig()
    {
        // Bubble up any collection or property changes from CategorySetting
        CategorySetting.CollectionChanged += (s, e) => RaiseChanged(nameof(CategorySetting));
        CategorySetting.PropertyChanged += (s, e) => RaiseChanged(nameof(CategorySetting));
    }

    /// <summary>
    /// Adds or replaces a category entry.
    /// Always use this instead of indexing <see cref="CategorySetting"/> directly
    /// to ensure change notifications are fired correctly.
    /// </summary>
    public void AddCategory(string key, CategorySetting setting)
        => CategorySetting[key] = setting;

    /// <summary>
    /// Clears and reinitializes <see cref="CategorySetting"/> from the given map.
    /// Use this when building a fresh config from a category list.
    /// </summary>
    public AnnouncerConfig SetCategorySettingMap(Dictionary<string, CategorySetting> map)
    {
        CategorySetting.Clear();

        foreach (var kv in map)
            AddCategory(kv.Key, kv.Value);

        return this;
    }

    /// <summary>
    /// Applies all values from <paramref name="src"/> into this instance in-place,
    /// preserving existing <see cref="CategorySetting"/> object references where possible.
    /// New categories are added, removed categories are cleaned up,
    /// and all changes are batched into a single notification via BeginUpdate/EndUpdate.
    /// Note: <see cref="AnnouncerGUID"/> is not overwritten as it identifies this instance.
    /// </summary>
    public void ApplyFrom(AnnouncerConfig src)
    {
        base.BeginUpdate();

        if (src == null)
        {
            base.EndUpdate();
            return;
        }

        // --- General ---
        RandomizeAudioOnPlay = src.RandomizeAudioOnPlay;

        // --- Categories: add or update ---
        foreach (var kv in src.CategorySetting)
        {
            var key = kv.Key;
            var srcCat = kv.Value;

            if (!CategorySetting.TryGetValue(key, out var dstCat))
            {
                dstCat = new CategorySetting();
                CategorySetting[key] = dstCat;
            }

            dstCat.ApplyFrom(srcCat);
        }

        // --- Categories: remove entries no longer present in src ---
        var toRemove = CategorySetting.Keys
            .Where(k => !src.CategorySetting.ContainsKey(k))
            .ToList();

        foreach (var key in toRemove)
            CategorySetting.Remove(key);

        base.EndUpdate();
    }
}

/// <summary>
/// Configuration for a single audio category, controlling
/// whether it is active, its volume scaling, cooldown duration,
/// and which audio files belong to it.
/// </summary>
public class CategorySetting : NotifyBase
{
    /// <summary>Whether this category is allowed to play.</summary>
    [IniKey("Enabled")]
    public bool Enabled
    {
        get => field;
        set => SetField(ref field, value);
    } = true;

    /// <summary>Scales the playback volume relative to the global announcer volume.</summary>
    [IniKey("VolumeMultiplier")]
    public float VolumeMultiplier
    {
        get => field;
        set => SetField(ref field, value);
    } = 1.0f;

    /// <summary>Minimum seconds that must pass before this category can play again.</summary>
    [IniKey("Cooldown")]
    public float Cooldown
    {
        get => field;
        set => SetField(ref field, value);
    } = 1.5f;

    private List<string> _audioFiles = new();

    /// <summary>
    /// List of audio file paths or names assigned to this category.
    /// A random entry is selected at playback time.
    /// Not expected to change at runtime.
    /// </summary>
    [IniKey("AudioFiles")]
    public List<string> AudioFiles
    {
        get => _audioFiles;
        set => _audioFiles = value ?? new List<string>();
    }

    /// <summary>
    /// Copies all values from <paramref name="src"/> into this instance in-place.
    /// </summary>
    public void ApplyFrom(CategorySetting src)
    {
        if (src == null)
            return;

        Enabled = src.Enabled;
        VolumeMultiplier = src.VolumeMultiplier;
        Cooldown = src.Cooldown;

        AudioFiles.Clear();
        AudioFiles.AddRange(src.AudioFiles);
    }
}


/*
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
        // 如果 Ini 反序列化后再填充，这里可以后处理 <- this is a vibe-coding product, but I thought It's a useful note
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
}*/