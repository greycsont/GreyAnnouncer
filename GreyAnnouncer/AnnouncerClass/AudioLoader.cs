using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace greycsont.GreyAnnouncer;


public class AudioLoader                                          
{
    #region Properties
    public string[]                            audioCategories       { get; private set; }
    public HashSet<string>                     categoryFailedLoading { get; private set; } = new HashSet<string>();
    public Dictionary<string, List<AudioClip>> audioClips            { get; private set; } = new Dictionary<string, List<AudioClip>>();

    private Dictionary<string, List<string>>   m_audioFileNames;
    private string                             m_audioPath;
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
        if (InstanceConfig.audioLoadingOptions.Value == 0) return;
        ClearCache();
        ValidateAndPrepareDirectory();
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }

    public AudioClip GetClipFromAudioClips(string category)
    {
        if (audioCategories.Contains(category) == false)
        {
            Plugin.log.LogWarning($"Invalid audio category: {category}");
            return null;
        }

        if (!audioClips.TryGetValue(category, out var clips) || clips.Count == 0)
            return null;
            
        return SelectAudioClipRandomly(clips);
    }

    public async Task<AudioClip> LoadSingleAudioClipAsync(string category)
    {
        var clips = await LoadCategoryAsync(category);
        if (clips == null || clips.Count == 0)
        {
            Plugin.log.LogError($"Failed to load audio clip for key: {category}");
            return null;
        }

        return SelectAudioClipRandomly(clips);
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
    private async Task LoadAllCategoriesAsync()
    {
        var loadingTasks = new List<Task<(string category, List<AudioClip> clips)>>();
        
        foreach ( var category in audioCategories )
        {
            loadingTasks.Add(LoadCategoryAsync(category).ContinueWith(task => (category, task.Result)));
        }
        
        var results = await Task.WhenAll(loadingTasks);
        
        // Add all loaded clips to the audioClips dictionary
        foreach (var (category, clips) in results)
        {
            if (clips != null && clips.Count > 0)
            {
                audioClips[category] = clips;
            }
        }
    }


    
    private async Task<List<AudioClip>> LoadCategoryAsync(string category)
    {
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
        
        var clipLoadingTasks = validFiles
            .Select(path => LoadAudioClipAsync(path));
        
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
        LogForPluginConfigurator();
        
        string logMessage = null;
        if (categoryFailedLoading.Count == 0)
        {
            logMessage = "All audio categories successfully loaded";
            Plugin.log.LogInfo(logMessage);
        }
        else
        {
            logMessage = "Failed to load audio categories: " + string.Join(", ", categoryFailedLoading);
            Plugin.log.LogWarning(logMessage);
        }
  
    }

    private void LogForPluginConfigurator()
    {   
        // Warning : PluginConfigurator
        var builder = new System.Text.StringBuilder();

        foreach (var category in audioCategories)
        {
            int loaded = audioClips.TryGetValue(category, out var clips) ? clips.Count : 0;
            int total = m_audioFileNames.TryGetValue(category, out var files) ? files.Count : 0;
            builder.AppendLine($"{category} ({loaded}/{total})");
        }

        //Reflection maybe
        string logMessage = builder.ToString();
        MainPanelBuilder.logHeader.text = logMessage + "\n";
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

    private AudioClip SelectAudioClipRandomly(List<AudioClip> audioClips)
    {
        int randomIndex = UnityEngine.Random.Range(0, audioClips.Count);
        return audioClips[randomIndex];
    }

    private void ValidateAndPrepareDirectory()
    {
        if (!Directory.Exists(m_audioPath))
        {
            Plugin.log.LogWarning($"Audio directory not found: {m_audioPath}");
            Directory.CreateDirectory(m_audioPath);
        }
    }
    #endregion
}