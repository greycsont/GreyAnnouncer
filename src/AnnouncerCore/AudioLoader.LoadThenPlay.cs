using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerAPI;

public partial class AudioLoader
{
    // Enabled is only checked here — Load then Play respects it, Preload does not.
    public async Task<(string category, AudioClip clip)> LoadAndGetSingleAudioClip(string category)
    {
        if (!announcerConfig.CategorySetting.TryGetValue(category, out var setting)) {
            LogHelper.LogError($"Category 「{category}」 not found in config");
            return (null, null);
        }

        if (!setting.Enabled) {
            LogHelper.LogDebug($"Category 「{category}」 is disabled, skipping");
            return (null, null);
        }

        if (!TryResolveAudioFiles(category, out var validFiles)) return (null, null);

        var selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null)
            LogHelper.LogError($"Selected file failed to load for category 「{category}」");

        return (category, clip);
    }

    public async Task<(string category, AudioClip clip)> GetRandomClipFromAllAvailableFiles()
    {
        var allValidFiles = new List<(string category, string path)>();

        foreach (var (category, setting) in announcerConfig.CategorySetting) {
            if (!setting.Enabled) continue;
            if (TryResolveAudioFiles(category, out var validFiles))
                allValidFiles.AddRange(validFiles.Select(path => (category, path)));
        }

        if (allValidFiles.Count == 0) return (null, null);

        var selected = allValidFiles[UnityEngine.Random.Range(0, allValidFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selected.path);

        if (clip == null) {
            LogHelper.LogError($"Selected file failed to load for category 「{selected.category}」");
            return (null, null);
        }

        return (selected.category, clip);
    }
}
