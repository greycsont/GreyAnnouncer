using System;
using System.Collections.Generic;
using System.Linq;

namespace GreyAnnouncer.AnnouncerCore;

public class PackConfig
{
    public bool RandomizeAudioOnPlay { get; set; }

    public Dictionary<string, CategorySetting> CategorySetting { get; } = new();

    public event Action Changed;

    public void Edit(Action<PackConfig> edit)
    {
        if (edit == null)
            throw new ArgumentNullException(nameof(edit));

        edit(this);
        Changed?.Invoke();
    }

    public void AddCategory(string key, CategorySetting setting)
        => Edit(config => config.CategorySetting[key] = setting);

    public PackConfig SetCategorySettingMap(Dictionary<string, CategorySetting> map)
    {
        if (map == null)
            throw new ArgumentNullException(nameof(map));

        Edit(config =>
        {
            config.CategorySetting.Clear();

            foreach (var pair in map)
                config.CategorySetting[pair.Key] = pair.Value;
        });

        return this;
    }

    public void ApplyFrom(PackConfig source)
    {
        if (source == null)
            return;

        Edit(config =>
        {
            config.RandomizeAudioOnPlay = source.RandomizeAudioOnPlay;

            foreach (var pair in source.CategorySetting)
            {
                if (!config.CategorySetting.TryGetValue(pair.Key, out var destination))
                {
                    destination = new CategorySetting();
                    config.CategorySetting[pair.Key] = destination;
                }

                destination.ApplyFrom(pair.Value);
            }

            var categoriesToRemove = config.CategorySetting.Keys
                .Where(key => !source.CategorySetting.ContainsKey(key))
                .ToList();

            foreach (var key in categoriesToRemove)
                config.CategorySetting.Remove(key);
        });
    }
}

public class CategorySetting
{
    public bool Enabled { get; set; } = true;

    public bool ExcludeFromRandom { get; set; }

    public float VolumeMultiplier { get; set; } = 1.0f;

    public float Cooldown { get; set; } = 1.5f;

    public List<string> AudioFiles { get; set; } = new();

    public void ApplyFrom(CategorySetting source)
    {
        if (source == null)
            return;

        Enabled = source.Enabled;
        ExcludeFromRandom = source.ExcludeFromRandom;
        VolumeMultiplier = source.VolumeMultiplier;
        Cooldown = source.Cooldown;
        AudioFiles = source.AudioFiles?.ToList() ?? new List<string>();
    }
}
