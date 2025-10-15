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

namespace GreyAnnouncer.AnnouncerAPI;


public class AudioLoader : IAudioLoader
{
    public        AnnouncerJsonSetting                jsonSetting           { get; set; }
    public        HashSet<string>                     categoryFailedLoading { get; private set; } = new HashSet<string>();
    private       Dictionary<string, List<AudioClip>> _audioClips                                 = new Dictionary<string, List<AudioClip>>();
    private       string                              _audioPath;
    public static Action<string>                      onPluginConfiguratorLogUpdated;

    #region Constructor
    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader(string audioPath)
    {
        this._audioPath = audioPath;
    }
    #endregion

    #region Preload_and_Play API
    public AudioClipWithCategory? GetClipFromCache(string category)
    {
        if (categoryFailedLoading.Contains(category)) return null;


        if (jsonSetting.CategoryAudioMap.Keys.Contains(category) == false) return null;


        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, clips.Count);

        var clip = clips[randomIndex];

        return new AudioClipWithCategory(category, clip);
    }

    public AudioClipWithCategory? GetRandomClipFromAudioClips()
    {
        var validEntries = _audioClips
            .SelectMany(kvp => kvp.Value.Select(clip => new AudioClipWithCategory(kvp.Key, clip)))
            .Where(entry => entry.clip != null)
            .ToList();

        if (validEntries.Count == 0) return null;
        
        return validEntries[UnityEngine.Random.Range(0, validEntries.Count)];
    }


    #endregion

    #region Load_then_Play API
    public async Task<AudioClipWithCategory?> LoadAndGetSingleAudioClipAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles)) return null;

        string selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await AudioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) LogCategoryFailure(category, "Selected file failed to load");

        return new AudioClipWithCategory (category, clip);
    }
    public async Task<AudioClipWithCategory?> GetRandomClipFromAllAvailableFiles()
    {
        var allValidFiles = new List<(string category, string path)>();
        
        foreach (var category in jsonSetting.CategoryAudioMap.Keys)
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
        
        return new AudioClipWithCategory(selected.category, clip);
    }
    #endregion


    #region Public API
    public async Task FindAvailableAudioAsync()
    {
        if (InstanceConfig.audioLoadingOptions.Value == 0) return;
        ClearCache();
        FileSystemUtil.ValidateAndPrepareDirectory(_audioPath);
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }
    public void UpdateAudioPath(string newAudioPath)
    {
        if (string.IsNullOrEmpty(newAudioPath))
        {
            LogManager.LogError("Cannot update with empty or null paths");
            return;
        }
        LogManager.LogInfo($"Updating audio paths and reloading audio...");
        this._audioPath = newAudioPath;
    }

    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting)
    {
        this.jsonSetting = jsonSetting;
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

        foreach (var category in jsonSetting.CategoryAudioMap.Keys)
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
        LogForPluginConfigurator();

        if (categoryFailedLoading.Count == 0)
        {
            LogManager.LogInfo("All audio categories successfully loaded");
        }
        else
        {
            LogManager.LogWarning("Failed to load audio categories: " + string.Join(", ", categoryFailedLoading));
        }

    }

    private void LogForPluginConfigurator()
    {
        // Warning : PluginConfigurator
        var builder = new System.Text.StringBuilder();

        foreach (var category in jsonSetting.CategoryAudioMap.Keys)
        {
            int loaded = _audioClips.TryGetValue(category, out var clips) ? clips.Count : 0;
            int total = jsonSetting.CategoryAudioMap.TryGetValue(category, out var setting) ? setting.AudioFiles.Count : 0;
            builder.AppendLine($"{category} ({loaded}/{total})");
        }

        //Reflection maybe
        string logMessage = builder.ToString();

        onPluginConfiguratorLogUpdated?.Invoke(logMessage);
    }
    #endregion

    #region Utility Methods
    private bool TryGetValidAudioFiles(string category, out List<string> validFiles)
    {
        validFiles = null;

        if (!jsonSetting.CategoryAudioMap.TryGetValue(category, out var categorySetting)
            || categorySetting.AudioFiles == null
            || categorySetting.AudioFiles.Count == 0)
        {
            LogCategoryFailure(category, "No file names configured");
            return false;
        }

        var fileNames = categorySetting.AudioFiles;
        validFiles = fileNames
            .Select(name => PathManager.GetFileWithExtension(_audioPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
        {
            LogCategoryFailure(category, "No valid files found");
            return false;
        }

        return true;
    }
    #endregion
}
