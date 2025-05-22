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
    private       IAudioClipLoader                    _audioClipLoader;
    public static Action<string>                      onPluginConfiguratorLogUpdated;

    #region Constructor
    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader(string audioPath, AnnouncerJsonSetting jsonSetting, IAudioClipLoader audioClipLoader)
    {
        this._audioPath       = audioPath;
        this.jsonSetting      = jsonSetting;
        this._audioClipLoader = audioClipLoader;
    }
    #endregion

    #region Preload_and_Play API
    public AudioClip GetClipFromCache(string category)
    {
        if (categoryFailedLoading.Contains(category)) return null;


        if (jsonSetting.CategoryAudioMap.Keys.Contains(category) == false) return null;


        if (!_audioClips.TryGetValue(category, out var clips) || clips.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, clips.Count);
        return clips[randomIndex];
    }

    public AudioClip GetRandomClipFromAudioClips()
    {
        var validClips = _audioClips
            .SelectMany(kvp => kvp.Value)
            .Where(clip => clip != null)
            .ToList();

        var clip = validClips[UnityEngine.Random.Range(0, validClips.Count)];
        return clip;
    }


    #endregion

    #region Load_then_Play API
    public async Task<AudioClip> LoadAndGetSingleAudioClipAsync(string category)
    {
        if (!TryGetValidAudioFiles(category, out var validFiles)) return null;

        string selectedPath = validFiles[UnityEngine.Random.Range(0, validFiles.Count)];
        var clip = await _audioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) LogCategoryFailure(category, "Selected file failed to load");

        return clip;
    }
    public async Task<AudioClip> GetRandomClipFromAllAvailableFiles()
    {
        // 以后应该是返回一个category和一个音频名？
        var totalValidFiles = new List<string>();
        foreach (var category in jsonSetting.CategoryAudioMap.Keys)
        {
            TryGetValidAudioFiles(category, out var validFiles);
            foreach (var fileName in validFiles)
            {
                totalValidFiles.Add(fileName);
            }
        }
        if (totalValidFiles.Count == 0) throw new ArgumentOutOfRangeException("114514");

        string selectedPath = totalValidFiles[UnityEngine.Random.Range(0, totalValidFiles.Count)];

        var clip = await _audioClipLoader.LoadAudioClipAsync(selectedPath);

        if (clip == null) LogCategoryFailure(selectedPath, "Selected file failed to load");

        return clip;
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
            .Select(path => _audioClipLoader.LoadAudioClipAsync(path));

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
        LogManager.LogWarning($"Failed to load category 「{category}」: {reason}");
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