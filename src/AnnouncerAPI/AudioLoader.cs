/* 
 * 基本上就是依靠 category 加载音频
 * 以及配套的获取 audioclip 的函数
 *
 *
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

    public HashSet<string> categoryFailedLoading { get; private set; } = new HashSet<string>();

    private Dictionary<string, List<AudioClip>> _audioClips = new Dictionary<string, List<AudioClip>>();

    public static Action<string> onPluginConfiguratorLogUpdated;

    public string announcerPath;


    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader()
    {
    }

    public async Task<Sound> LoadAudioClip(string category)
    {
        (string, AudioClip) clipWithCategory;

        if (BepInExConfig.audioLoadingStrategy.Value == 0)
        {
            var currentRequestId = ++AnnouncerManager.playRequestId;

            if (announcerConfig.RandomizeAudioOnPlay == false)
                clipWithCategory = await LoadAndGetSingleAudioClipAsync(category);
            else
                clipWithCategory = await GetRandomClipFromAllAvailableFiles();

            if (
                currentRequestId != AnnouncerManager.playRequestId
                && BepInExConfig.audioPlayOptions.Value == 0
            )
                return null;
        }
        else
        {
            if (announcerConfig.RandomizeAudioOnPlay == false)
                clipWithCategory = GetClipFromCache(category);
            else
                clipWithCategory = GetRandomClipFromAudioClips();
        }

        if (clipWithCategory == (null, null))
        {
            LogCategoryFailure(category, $"No audio clip available to play, current loading strategy: {BepInExConfig.audioLoadingStrategy.Value}");
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
        if (categoryFailedLoading.Contains(category)) {
            LogHelper.LogError("categoryfailedloading");
            return (null, null);
        }
        if (announcerConfig.CategorySetting.Keys.Contains(category) == false) {
            LogHelper.LogError("contains category = false");
            return (null, null);
        }
        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0){
            LogHelper.LogError("no clip in cache"); 
            return (null, null);
        }
        int randomIndex = UnityEngine.Random.Range(0, clips.Count);

        var clip = clips[randomIndex];

        return (category, clip);
    }

    public (string category, AudioClip clip) GetRandomClipFromAudioClips()
    {
        var validEntries = _audioClips
            .Where(kvp => announcerConfig.CategorySetting.TryGetValue(kvp.Key, out var data) && data.Enabled)
            .SelectMany(kvp => kvp.Value
            .Where(clip => clip != null)
            .Select(clip => (kvp.Key, clip)))  // 这里保留 key
            .ToList();

        if (validEntries.Count == 0) 
            return (null, null);

        var (category, clip) = validEntries[UnityEngine.Random.Range(0, validEntries.Count)];

        return (category, clip);
    }


    #endregion

    #region Load_then_Play API
    public async Task<(string category, AudioClip clip)> LoadAndGetSingleAudioClipAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles)) return (null, null);

        var selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) LogCategoryFailure(category, "Selected file failed to load");

        return (category, clip);
    }
    public async Task<(string category, AudioClip clip)> GetRandomClipFromAllAvailableFiles()
    {
        var allValidFiles = new List<(string category, string path)>();
        
        foreach (var category in announcerConfig.CategorySetting.Keys)
        {
            if (TryGetValidAudioFiles(category, out var validFiles))
            {
                allValidFiles.AddRange(validFiles.Select(path => (category, path)));
            }
        }
        
        if (allValidFiles.Count == 0) return (null, null);
        
        var selected = allValidFiles[UnityEngine.Random.Range(0, allValidFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selected.path);
        
        if (clip == null)
        {
            LogCategoryFailure(selected.category, "Selected file failed to load");
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
        if (BepInExConfig.audioLoadingStrategy.Value == 0) return;
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
        categoryFailedLoading.Clear();
    }
    #endregion


    #region Loading Logic
    private async Task LoadAllCategoriesAsync()
    {
        var loadingTasks = new List<Task<(string category, List<AudioClip> clips)>>();

        foreach (var category in announcerConfig.CategorySetting.Keys)
        {
            loadingTasks.Add(LoadCategoryAsync(category).ContinueWith(task => (category, task.Result)));
        }

        var results = await Task.WhenAll(loadingTasks);

        // Add all loaded clips to the audioClips dictionary
        foreach (var (category, clips) in results)
        {
            if (clips != null && clips.Count > 0)
            {
                _audioClips[category] = clips;
            }
        }
    }


    private async Task<List<AudioClip>> LoadCategoryAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles))
            return null;

        LogHelper.LogInfo($"Loading category {category} with {validFiles.Count} files");

        var clipLoadingTasks = validFiles
            .Select(path => AudioClipLoader.LoadAudioClipAsync(path));

        var loadedClips = new List<AudioClip>();

        try
        {
            var results = await Task.WhenAll(clipLoadingTasks);

            // Filter out any null results (loading failures)
            loadedClips = results
                .Where(c => c != null)
                .ToList();

            if (loadedClips.Count > 0)
            {
                LogHelper.LogInfo($"Successfully loaded {loadedClips.Count}/{validFiles.Count} clips for {category}");
                return loadedClips;
            }
            else
            {
                LogCategoryFailure(category, "All audio files failed to load");
                return null;
            }
        }
        catch (Exception ex)
        {
            LogCategoryFailure(category, $"Exception during loading: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region Cache Management
    private void ClearAudioClipCache()
    {
        foreach (var clipList in _audioClips.Values)
        {
            foreach (var clip in clipList)
            {
                if (clip != null)
                    UnityEngine.Object.Destroy(clip);
            }
        }
        _audioClips.Clear();
    }
    #endregion

    #region Logging
    private void LogCategoryFailure(string category, string reason)
    {
        categoryFailedLoading.Add(category);
        LogHelper.LogDebug($"Failed to load category 「{category}」: {reason}");
    }

    private void LogLoadingResults()
    {
        LogHelper.LogDebug("Loading directory: " + announcerPath);
        if (categoryFailedLoading.Count == 0)
            LogHelper.LogInfo("All audio categories successfully loaded");
        else
            LogHelper.LogWarning("Failed to load audio categories: " + string.Join(", ", categoryFailedLoading));
    }
    #endregion

    #region Utility Methods
    private bool TryGetValidAudioFiles(string category, out List<string> validFiles)
    {
        LogHelper.LogDebug($"Loaded category: {category}");
        validFiles = null;

        if (!announcerConfig.CategorySetting[category].Enabled)
        {
            LogCategoryFailure(category, "Stopped by Config");
        }

        if (!announcerConfig.CategorySetting.TryGetValue(category, out var categorySetting)
            || categorySetting.AudioFiles == null
            || categorySetting.AudioFiles.Count == 0)
        {
            LogCategoryFailure(category, "No file names configured");
            return false;
        }

        var fileNames = categorySetting.AudioFiles;
        validFiles = fileNames
            .Select(name => PathHelper.GetFile(announcerPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
        {
            LogCategoryFailure(category, "No valid files found :" + string.Join(", ", fileNames));
            return false;
        }

        if (validFiles.Count != fileNames.Count)
            LogHelper.LogWarning($"{category} not loaded in audioFiles: {string.Join(", ", fileNames.Except(validFiles.Select(p => System.IO.Path.GetFileName(p)).ToList()))}");

        return true;
    }
    #endregion
}
