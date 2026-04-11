using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerAPI;

public partial class AudioLoader
{
    private readonly HashSet<string> _categoryFailed = [];
    private readonly Dictionary<string, List<AudioClip>> _audioClips = [];


    public (string category, AudioClip clip) GetClipFromCache(string category)
    {
        if (_categoryFailed.Contains(category)) {
            LogHelper.LogError($"Category 「{category}」 previously failed to load");
            return (null, null);
        }

        if (!announcerConfig.CategorySetting.ContainsKey(category)) {
            LogHelper.LogError($"Category 「{category}」 not found in config");
            return (null, null);
        }

        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0) {
            LogHelper.LogError($"No clips in cache for category 「{category}」");
            return (null, null);
        }

        var clip = clips[UnityEngine.Random.Range(0, clips.Count)];
        return (category, clip);
    }

    public (string category, AudioClip clip) GetRandomClipFromAudioClips()
    {
        var validEntries = _audioClips
            .Where(kvp => announcerConfig.CategorySetting.ContainsKey(kvp.Key))
            .SelectMany(kvp => kvp.Value
                .Where(clip => clip != null)
                .Select(clip => (kvp.Key, clip)))
            .ToList();

        if (validEntries.Count == 0)
            return (null, null);

        (string category, AudioClip clip) = validEntries[UnityEngine.Random.Range(0, validEntries.Count)];
        return (category, clip);
    }


    private async Task LoadAllCategoriesAsync()
    {
        async Task<(string, List<AudioClip>)> WrapWithCategory(string cat)
        {
            var clips = await LoadCategoryAsync(cat);
            return (cat, clips);
        }

        var tasks = announcerConfig.CategorySetting.Keys.Select(WrapWithCategory);
        var results = await Task.WhenAll(tasks);

        foreach (var (category, clips) in results) {
            if (clips != null && clips.Count > 0)
                _audioClips[category] = clips;
        }
    }

    private async Task<List<AudioClip>> LoadCategoryAsync(string category)
    {
        if (!TryResolveAudioFiles(category, out var validFiles)) {
            _categoryFailed.Add(category);
            return null;
        }

        LogHelper.LogInfo($"Loading category 「{category}」 with {validFiles.Count} files");

        try {
            var results = await Task.WhenAll(validFiles.Select(AudioClipLoader.LoadAudioClipAsync));
            var loadedClips = results.Where(c => c != null).ToList();

            if (loadedClips.Count > 0) {
                LogHelper.LogInfo($"Successfully loaded {loadedClips.Count}/{validFiles.Count} clips for 「{category}」");
                return loadedClips;
            }

            _categoryFailed.Add(category);
            LogHelper.LogDebug($"Failed to load category 「{category}」: All audio files failed to load");
            return null;
        }
        catch (Exception ex) {
            _categoryFailed.Add(category);
            LogHelper.LogDebug($"Failed to load category 「{category}」: Exception during loading: {ex.Message}");
            return null;
        }
    }


    private void ClearAudioClipCache()
    {
        foreach (var clip in _audioClips.Values.SelectMany(list => list).Where(c => c != null))
            UnityEngine.Object.Destroy(clip);

        _audioClips.Clear();
    }

    private void LogLoadingResults()
    {
        LogHelper.LogDebug($"{_announcer.title} Loading directory: {announcerPath}");

        if (_categoryFailed.Count > 0)
            LogHelper.LogWarning("Failed to load categories: " + string.Join(", ", _categoryFailed));
        else
            LogHelper.LogInfo("All categories successfully loaded");
    }
}
