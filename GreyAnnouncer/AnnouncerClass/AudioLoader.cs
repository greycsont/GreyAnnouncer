using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace greycsont.GreyAnnouncer;

[Description("The AudioLoader should and only be used as a audioClips entity, " +
             "I tried to add cooldown counter in here but I just realize it's only a audio loader")]
public class AudioLoader                                          
{
    #region Properties
    public string[]                         audioCategories       { get; private set; }
    public HashSet<string>                  categoryFailedLoading { get; private set; } = new HashSet<string>();
    public Dictionary<int, List<AudioClip>> audioClips            { get; private set; } = new Dictionary<int, List<AudioClip>>();

    private Dictionary<string, List<string>> m_audioFileNames;
    private string                           m_audioPath;
    #endregion

    #region Constructor
    public AudioLoader(string audioPath, string[] audioCategories, AnnouncerJsonSetting jsonSetting)
    {
        this.m_audioPath      = audioPath;
        this.audioCategories  = audioCategories;
        this.m_audioFileNames = GetAudioFileNames(jsonSetting);
    }
    #endregion

    #region Public API
    public void UpdateAudioPath(string newAudioPath)
    {
        if (string.IsNullOrEmpty(newAudioPath))
        {
            Plugin.log.LogError("Cannot update with empty or null paths");
            return;
        }

        Plugin.log.LogInfo($"Updating audio paths and reloading audio...");
        this.m_audioPath = newAudioPath;
    }

    public async Task FindAvailableAudioAsync()
    {
        ClearCache();
        ValidateAndPrepareDirectory();
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }

    public AudioClip TryToGetAudioClip(int key)
    {
        return GetClipFromAudioClips(key);
    }

    public void UpdateAudioFileNames(AnnouncerJsonSetting jsonSetting)
    {
        this.m_audioFileNames = GetAudioFileNames(jsonSetting);
    }

    public void ClearCache()
    {
        ClearAudioClipCache();
        categoryFailedLoading.Clear();
    }
    #endregion

    #region Loading Logic
    private void ValidateAndPrepareDirectory()
    {
        if (!Directory.Exists(m_audioPath))
        {
            Plugin.log.LogWarning($"Audio directory not found: {m_audioPath}");
            Directory.CreateDirectory(m_audioPath);
        }
    }

    private async Task LoadAllCategoriesAsync()
    {
        var loadingTasks = new List<Task<(int index, List<AudioClip> clips)>>();
        
        for (int i = 0; i < audioCategories.Length; i++)
        {
            int categoryIndex = i; // Capture for closure
            loadingTasks.Add(LoadCategoryAsync(categoryIndex).ContinueWith(task => (categoryIndex, task.Result)));
        }
        
        var results = await Task.WhenAll(loadingTasks);
        
        // Add all loaded clips to the audioClips dictionary
        foreach (var (index, clips) in results)
        {
            if (clips != null && clips.Count > 0)
            {
                audioClips[index] = clips;
            }
        }
    }
    
    private async Task<List<AudioClip>> LoadCategoryAsync(int index)
    {
        var category = audioCategories[index];
        if (!m_audioFileNames.TryGetValue(category, out var fileNames))
        {
            LogCategoryFailure(category, "No file names configured");
            return null;
        }

        var validFiles = fileNames
            .Select(name => PathManager.GetFileWithExtension(m_audioPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
        {
            LogCategoryFailure(category, "No valid files found");
            return null;
        }

        Plugin.log.LogInfo($"Loading category {category} with {validFiles.Count} files");
        
        var clipLoadingTasks = validFiles.Select(path => LoadAudioClipAsync(path));
        var loadedClips = new List<AudioClip>();
        
        try
        {
            var results = await Task.WhenAll(clipLoadingTasks);
            
            // Filter out any null results (loading failures)
            loadedClips = results.Where(c => c != null).ToList();
            
            if (loadedClips.Count > 0)
            {
                Plugin.log.LogInfo($"Successfully loaded {loadedClips.Count}/{validFiles.Count} clips for {category}");
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

    private async Task<AudioClip> LoadAudioClipAsync(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        AudioType? unityAudioType = GetUnityAudioType(extension);

        if (!unityAudioType.HasValue)
        {
            Plugin.log.LogError($"Unsupported audio format: 「{extension}」 for {path}");
            return null;
        }

        try
        {
            Plugin.log.LogInfo($"Started loading audio: {path}");
            return await LoadWithUnityAsync(path, unityAudioType.Value);
        }
        catch (Exception ex)
        {
            Plugin.log.LogError($"Error loading {path}: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region Audio Loading Helpers
    private async Task<AudioClip> LoadWithUnityAsync(string path, AudioType audioType)
    {
        string url = new Uri(path).AbsoluteUri;
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            var operation = www.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Delay(10); // Small delay to not block the thread
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.log.LogError($"UnityRequest Failed to load audio: {www.error}");
                return null;
            }
            
            var clip = DownloadHandlerAudioClip.GetContent(www);
            Plugin.log.LogInfo($"Loaded audio: {Path.GetFileName(path)}");
            return clip;
        }
    }
    #endregion

    #region Utility Methods
    private AudioType? GetUnityAudioType(string extension)
    {
        return extension switch
        {
            ".wav"  => AudioType.WAV,
            ".mp3"  => AudioType.MPEG,
            ".ogg"  => AudioType.OGGVORBIS,
            ".aiff" => AudioType.AIFF,
            ".aif"  => AudioType.AIFF,
            ".acc"  => AudioType.ACC,
            _       => null
        };
    }
    #endregion

    #region Cache Management
    private void ClearAudioClipCache()
    {
        foreach (var clipList in audioClips.Values)
        {
            foreach (var clip in clipList)
            {
                if (clip != null)
                    UnityEngine.Object.Destroy(clip);
            }
        }
        audioClips.Clear();
    }
    #endregion

    #region Logging
    private void LogCategoryFailure(string category, string reason)
    {
        categoryFailedLoading.Add(category);
        Plugin.log.LogWarning($"Failed to load category 「{category}」: {reason}");
    }

    private void LogLoadingResults()
    {
        if (categoryFailedLoading.Count == 0)
        {
            Plugin.log.LogInfo("All audio categories successfully loaded");
        }
        else
        {
            Plugin.log.LogWarning("Failed to load audio categories: " + string.Join(", ", categoryFailedLoading));
        }
    }
    #endregion

    #region Dictionary Helper
    private Dictionary<string, List<string>> GetAudioFileNames(AnnouncerJsonSetting jsonSetting)
    {
        return jsonSetting.CategoryAudioMap.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.AudioFiles
        );
    }
    #endregion

    #region Get Audio Clip
    private AudioClip GetClipFromAudioClips(int key)
    {
        if (key < 0 || key >= audioCategories.Length)
        {
            Plugin.log.LogWarning($"Invalid audio key: {key}");
            return null;
        }

        if (!audioClips.TryGetValue(key, out var clips) || clips.Count == 0)
            return null;
            
        int randomIndex = UnityEngine.Random.Range(0, clips.Count);
        return clips[randomIndex];
    }

    public async Task<AudioClip> LoadAudioClipAsync(int key)
    {
        var clips = await LoadCategoryAsync(key);
        if (clips == null || clips.Count == 0)
        {
            Plugin.log.LogError($"Failed to load audio clip for key: {key}");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, clips.Count);
        return clips[randomIndex];
    }
    #endregion
}