/* 
 * 基本上就是依靠 category 加载音频
 * 以及配套的获取 audioclip 的函数
 */

using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.ComponentModel;

using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.Config;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioLoader : IAudioLoader
{
    public AnnouncerConfig announcerConfig { get; set; }
    public string announcerPath { get; private set; }

    public static Action<string> onPluginConfiguratorLogUpdated;

    // 替换原来的 categoryFailedLoading HashSet
    private Dictionary<string, CategoryStatus> _categoryStatus = new Dictionary<string, CategoryStatus>();
    private Dictionary<string, List<AudioClip>> _audioClips = new Dictionary<string, List<AudioClip>>();

    private enum CategoryStatus
    {
        Loaded,
        Disabled,
        Failed
    }

    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader() { }


    public async Task<Sound> LoadAudioClip(string category)
    {
        (string, AudioClip) clipWithCategory;

        if (Setting.audioLoadingStrategy == 0)
        {
            var currentRequestId = ++AnnouncerManager.playRequestId;

            clipWithCategory = announcerConfig.RandomizeAudioOnPlay
                ? await GetRandomClipFromAllAvailableFiles()
                : await LoadAndGetSingleAudioClipAsync(category);

            if (currentRequestId != AnnouncerManager.playRequestId && Setting.audioPlayOptions == 0)
                return null;
        }
        else
        {
            clipWithCategory = announcerConfig.RandomizeAudioOnPlay
                ? GetRandomClipFromAudioClips()
                : GetClipFromCache(category);
        }

        if (clipWithCategory.Item2 == null)
        {
            LogHelper.LogWarning($"No audio clip available for category 「{category}」, strategy: {Setting.audioLoadingStrategy}");
            return null;
        }

        return new Sound(
            clipWithCategory.Item1,
            clipWithCategory.Item2,
            announcerConfig.CategorySetting[clipWithCategory.Item1].VolumeMultiplier
        );
    }


    #region Preload_and_Play API

    public (string category, AudioClip clip) GetClipFromCache(string category)
    {
        if (_categoryStatus.TryGetValue(category, out var status))
        {
            if (status == CategoryStatus.Disabled)
            {
                LogHelper.LogDebug($"Category 「{category}」 is disabled, skipping");
                return (null, null);
            }
            if (status == CategoryStatus.Failed)
            {
                LogHelper.LogError($"Category 「{category}」 previously failed to load");
                return (null, null);
            }
        }

        if (!announcerConfig.CategorySetting.ContainsKey(category))
        {
            LogHelper.LogError($"Category 「{category}」 not found in config");
            return (null, null);
        }

        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0)
        {
            LogHelper.LogError($"No clips in cache for category 「{category}」");
            return (null, null);
        }

        var clip = clips[UnityEngine.Random.Range(0, clips.Count)];
        return (category, clip);
    }

    public (string category, AudioClip clip) GetRandomClipFromAudioClips()
    {
        var validEntries = _audioClips
            .Where(kvp => announcerConfig.CategorySetting.ContainsKey(kvp.Key)
                       && announcerConfig.CategorySetting[kvp.Key].Enabled)
            .SelectMany(kvp => kvp.Value
                .Where(clip => clip != null)
                .Select(clip => (kvp.Key, clip)))
            .ToList();

        if (validEntries.Count == 0)
            return (null, null);

        (string category, AudioClip clip) = validEntries[UnityEngine.Random.Range(0, validEntries.Count)];
        return (category, clip);
    }

    #endregion


    #region Load_then_Play API

    public async Task<(string category, AudioClip clip)> LoadAndGetSingleAudioClipAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles)) return (null, null);

        var selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) MarkCategoryFailed(category, "Selected file failed to load");

        return (category, clip);
    }

    public async Task<(string category, AudioClip clip)> GetRandomClipFromAllAvailableFiles()
    {
        var allValidFiles = new List<(string category, string path)>();

        foreach (var category in announcerConfig.CategorySetting.Keys)
        {
            if (TryGetValidAudioFiles(category, out var validFiles))
                allValidFiles.AddRange(validFiles.Select(path => (category, path)));
        }

        if (allValidFiles.Count == 0) return (null, null);

        var selected = allValidFiles[UnityEngine.Random.Range(0, allValidFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selected.path);

        if (clip == null)
        {
            MarkCategoryFailed(selected.category, "Selected file failed to load");
            return (null, null);
        }

        return (selected.category, clip);
    }

    #endregion


    #region Public API

    public async Task FindAvailableAudioAsync()
    {
        LogHelper.LogInfo("Starting to find available audio asynchronously.");
        ClearCache();
        if (Setting.audioLoadingStrategy == 0) return;
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }

    public void UpdateSetting(AnnouncerConfig newAnnouncerConfig, string announcerPath)
    {
        this.announcerPath = announcerPath;
        this.announcerConfig = newAnnouncerConfig;
        _ = FindAvailableAudioAsync();
    }

    public void ClearCache()
    {
        ClearAudioClipCache();
        _categoryStatus.Clear();
    }

    #endregion


    #region Loading Logic

    private async Task LoadAllCategoriesAsync()
    {
        async Task<(string, List<AudioClip>)> WrapWithCategory(string cat)
        {
            var clips = await LoadCategoryAsync(cat);
            return (cat, clips);
        }

        var tasks = announcerConfig.CategorySetting.Keys.Select(WrapWithCategory);
        var results = await Task.WhenAll(tasks);

        foreach (var (category, clips) in results)
        {
            if (clips != null && clips.Count > 0)
            {
                _audioClips[category] = clips;
                _categoryStatus[category] = CategoryStatus.Loaded;
            }
        }
    }

    private async Task<List<AudioClip>> LoadCategoryAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles))
            return null;

        LogHelper.LogInfo($"Loading category 「{category}」 with {validFiles.Count} files");

        try
        {
            var results = await Task.WhenAll(validFiles.Select(AudioClipLoader.LoadAudioClipAsync));

            var loadedClips = results.Where(c => c != null).ToList();

            if (loadedClips.Count > 0)
            {
                LogHelper.LogInfo($"Successfully loaded {loadedClips.Count}/{validFiles.Count} clips for 「{category}」");
                return loadedClips;
            }

            MarkCategoryFailed(category, "All audio files failed to load");
            return null;
        }
        catch (Exception ex)
        {
            MarkCategoryFailed(category, $"Exception during loading: {ex.Message}");
            return null;
        }
    }

    #endregion


    #region Cache Management

    private void ClearAudioClipCache()
    {
        foreach (var clip in _audioClips.Values.SelectMany(list => list).Where(c => c != null))
            UnityEngine.Object.Destroy(clip);

        _audioClips.Clear();
    }

    #endregion


    #region Status & Logging

    private void MarkCategoryDisabled(string category)
    {
        _categoryStatus[category] = CategoryStatus.Disabled;
        LogHelper.LogDebug($"Category 「{category}」 is disabled by config");
    }

    private void MarkCategoryFailed(string category, string reason)
    {
        _categoryStatus[category] = CategoryStatus.Failed;
        LogHelper.LogDebug($"Failed to load category 「{category}」: {reason}");
    }

    private void LogLoadingResults()
    {
        LogHelper.LogDebug("Loading directory: " + announcerPath);

        var failed   = _categoryStatus.Where(x => x.Value == CategoryStatus.Failed).Select(x => x.Key).ToList();
        var disabled = _categoryStatus.Where(x => x.Value == CategoryStatus.Disabled).Select(x => x.Key).ToList();

        if (disabled.Count > 0)
            LogHelper.LogInfo("Disabled categories: " + string.Join(", ", disabled));

        if (failed.Count > 0)
            LogHelper.LogWarning("Failed to load categories: " + string.Join(", ", failed));
        else
            LogHelper.LogInfo("All enabled categories successfully loaded");
    }

    #endregion


    #region Utility Methods

    private bool TryGetValidAudioFiles(string category, out List<string> validFiles)
    {
        validFiles = null;

        if (!announcerConfig.CategorySetting.TryGetValue(category, out var categorySetting))
        {
            MarkCategoryFailed(category, "Category not found in config");
            return false;
        }

        if (!categorySetting.Enabled)
        {
            MarkCategoryDisabled(category);
            return false;
        }

        if (categorySetting.AudioFiles == null || categorySetting.AudioFiles.Count == 0)
        {
            MarkCategoryFailed(category, "No file names configured");
            return false;
        }

        validFiles = categorySetting.AudioFiles
            .Select(name => PathHelper.GetFile(announcerPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
        {
            MarkCategoryFailed(category, "No valid files found: " + string.Join(", ", categorySetting.AudioFiles));
            return false;
        }

        var missing = categorySetting.AudioFiles
            .Except(validFiles.Select(p => Path.GetFileName(p)))
            .ToList();

        if (missing.Count > 0)
            LogHelper.LogWarning($"「{category}」 missing files: {string.Join(", ", missing)}");

        return true;
    }

    #endregion
}