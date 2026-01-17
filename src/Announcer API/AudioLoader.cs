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
using GreyAnnouncer.Setting;
using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerAPI;


public class AudioLoader : IAudioLoader
{
    public        AnnouncerConfig                     announcerConfig           { get; set; }
    public        HashSet<string>                     categoryFailedLoading { get; private set; } = new HashSet<string>();
    private       Dictionary<string, List<AudioClip>> _audioClips                                 = new Dictionary<string, List<AudioClip>>();
    public static Action<string>                      onPluginConfiguratorLogUpdated;

    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader()
    {
    }

    public async Task<Sound> LoadAudioClip(string category)
    {
        AudioClip clip;

        if (BepInExConfig.audioLoadingOptions.Value == 0)
        {
            var currentRequestId = ++AnnouncerManager.playRequestId;

            if (announcerConfig.RandomizeAudioOnPlay == false)
                clip = await LoadAndGetSingleAudioClipAsync(category);
            else
                clip = await GetRandomClipFromAllAvailableFiles();

            if (
                currentRequestId != AnnouncerManager.playRequestId
                && BepInExConfig.audioPlayOptions.Value == 0
            )
            {
                return null;
            }
        }
        else
        {
            if (announcerConfig.RandomizeAudioOnPlay == false)
            {
                clip = GetClipFromCache(category);
            }
            else
            {
                clip = GetRandomClipFromAudioClips();
            }
        }

        if (clip == null)
        {
            LogCategoryFailure(category, "No audio clip available to play");
            return null;
        }

        Sound sound = new Sound(category, clip, announcerConfig.CategoryAudioMap[category].VolumeMultiplier);

        return sound;
    }



    #region Preload_and_Play API
    public AudioClip GetClipFromCache(string category)
    {
        if (categoryFailedLoading.Contains(category)) return null;


        if (announcerConfig.CategoryAudioMap.Keys.Contains(category) == false) return null;


        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, clips.Count);

        var clip = clips[randomIndex];

        return clip;
    }

    public AudioClip GetRandomClipFromAudioClips()
    {
        var validEntries = _audioClips
            .SelectMany(kvp => kvp.Value.Select(clip => clip))
            .Where(clip => clip != null)
            .ToList();

        if (validEntries.Count == 0) return null;
        
        return validEntries[UnityEngine.Random.Range(0, validEntries.Count)];
    }


    #endregion

    #region Load_then_Play API
    public async Task<AudioClip> LoadAndGetSingleAudioClipAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles)) return null;

        string selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) LogCategoryFailure(category, "Selected file failed to load");

        return clip;
    }
    public async Task<AudioClip> GetRandomClipFromAllAvailableFiles()
    {
        var allValidFiles = new List<(string category, string path)>();
        
        foreach (var category in announcerConfig.CategoryAudioMap.Keys)
        {
            if (TryGetValidAudioFiles(category, out var validFiles))
            {
                allValidFiles.AddRange(validFiles.Select(path => (category, path)));
            }
        }
        
        if (allValidFiles.Count == 0) return null;
        
        var selected = allValidFiles[UnityEngine.Random.Range(0, allValidFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selected.path);
        
        if (clip == null)
        {
            LogCategoryFailure(selected.category, "Selected file failed to load");
            return null;
        }
        
        return clip;
    }
    #endregion

    #region Public API
    public async Task FindAvailableAudioAsync()
    {
        if (BepInExConfig.audioLoadingOptions.Value == 0) return;
        ClearCache();
        FileSystemUtil.ValidateAndPrepareDirectory(announcerConfig.AudioPath);
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }

    public void UpdateAnnouncerConfig(AnnouncerConfig newAnnouncerConfig)
    {
        this.announcerConfig = newAnnouncerConfig;
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

        foreach (var category in announcerConfig.CategoryAudioMap.Keys)
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
        {
            return null;
        }

        LogManager.LogInfo($"Loading category {category} with {validFiles.Count} files");

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
                LogManager.LogInfo($"Successfully loaded {loadedClips.Count}/{validFiles.Count} clips for {category}");
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
        LogManager.LogDebug($"Failed to load category 「{category}」: {reason}");
    }

    private void LogLoadingResults()
    {
        LogManager.LogDebug("Loading directory: " + announcerConfig.AudioPath);
        if (categoryFailedLoading.Count == 0)
            LogManager.LogInfo("All audio categories successfully loaded");
        else
            LogManager.LogWarning("Failed to load audio categories: " + string.Join(", ", categoryFailedLoading));
    }
    #endregion

    #region Utility Methods
    private bool TryGetValidAudioFiles(string category, out List<string> validFiles)
    {
        validFiles = null;

        if (!announcerConfig.CategoryAudioMap.TryGetValue(category, out var categorySetting)
            || categorySetting.AudioFiles == null
            || categorySetting.AudioFiles.Count == 0)
        {
            LogCategoryFailure(category, "No file names configured");
            return false;
        }

        var fileNames = categorySetting.AudioFiles;
        validFiles = fileNames
            .Select(name => PathManager.GetFileWithExtension(announcerConfig.AudioPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
        {
            LogCategoryFailure(category, "No valid files found :" + string.Join(", ", fileNames));
            return false;
        }

        return true;
    }
    #endregion
}
