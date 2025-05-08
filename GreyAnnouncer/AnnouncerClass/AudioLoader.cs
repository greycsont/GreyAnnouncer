using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.ComponentModel;


namespace greycsont.GreyAnnouncer;

[Description("The AudioLoader should and only be used as a audioClips entity, " +
             "I tried to add cooldown counter in here but I just realize it's only a audio loader")]
public class AudioLoader                                          
{
    #region Properties
    public string[]                         audioCategories       { get; private set; }
    public HashSet<string>                  categoryFailedLoading { get; private set; } = new HashSet<string>();
    public Dictionary<int, List<AudioClip>> audioClips            { get; private set; } = new Dictionary<int, List<AudioClip>>();

    private Dictionary<string, List<string>>  m_audioFileNames;
    private string                            m_audioPath;
    private Dictionary<string, LoadingStatus> m_LoadingStatus = new Dictionary<string, LoadingStatus>();
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
        if (newAudioPath == null || newAudioPath.Length == 0)
        {
            Plugin.Log.LogError("Cannot update with empty or null paths");
            return;
        }

        Plugin.Log.LogInfo($"Updating audio paths and reloading audio...");
        this.m_audioPath = newAudioPath;
    }

    public void FindAvailableAudio()
    {
        ClearCache();
        ValidateAndPrepareDirectory();
        StartLoadingAllCategories();
    }

    public AudioClip TryToGetAudioClip(int key)
    {
        return GetClipFromAudioClips(key);
    }

    public void UpdateAudioFileNames(AnnouncerJsonSetting jsonSetting)
    {
        this.m_audioFileNames = GetAudioFileNames(jsonSetting);
    }
    #endregion


    #region Loading Logic
    private void ValidateAndPrepareDirectory()
    {
        if (!Directory.Exists(m_audioPath))
        {
            Plugin.Log.LogWarning($"Audio directory not found: {m_audioPath}");
            Directory.CreateDirectory(m_audioPath);
        }
    }

    private void StartLoadingAllCategories()
    {
        m_LoadingStatus.Clear();

        CoroutineRunner.Instance.StartCoroutine(MonitorLoadingProgress());
        
        for (int i = 0; i < audioCategories.Length; i++)
        {
            LoadCategory(i);
        }
    }
    
    private void LoadCategory(int index)
    {
        var category = audioCategories[index];
        if (!m_audioFileNames.TryGetValue(category, out var fileNames))
        {
            LogCategoryFailure(category, "No file names configured");
            return;
        }

        var status = new LoadingStatus { Category = category };
        m_LoadingStatus[category] = status;

        foreach (var fileName in fileNames)
        {
            var filePath = PathManager.GetFileWithExtension(m_audioPath, fileName);
            if (File.Exists(filePath))
            {
                status.ExpectedFiles++;
            }
        }

        if (status.ExpectedFiles == 0)
        {
            LogCategoryFailure(category, "No valid files found");
            return;
        }

        foreach (var fileName in fileNames)
        {
            TryStartLoadingFile(fileName, index, status);
        }
    }


    private void TryStartLoadingFile(string fileName, int categoryIndex, LoadingStatus status)
    {
        var filePath = PathManager.GetFileWithExtension(m_audioPath, fileName);
        if (!File.Exists(filePath))
        {
            Plugin.Log.LogWarning($"Audio file not found: {filePath}");
        }

        Plugin.Log.LogInfo($"Started loading audio: {filePath}");
        var coroutine = LoadAudioClip(filePath, categoryIndex, status);
        CoroutineRunner.Instance.StartCoroutine(coroutine);
 
    }

    private IEnumerator LoadAudioClip(string path, int categoryIndex, LoadingStatus status)
    {
        
        string extension = Path.GetExtension(path).ToLower();
        AudioType? unityAudioType = GetUnityAudioType(extension);


        if (unityAudioType.HasValue)
        {
            yield return LoadWithUnity(path, categoryIndex, status, unityAudioType.Value);
        }
        else
        {
            status.HasError = true;
            Plugin.Log.LogError($"Unsupported audio format: {extension}");
        }
    }
    #endregion

    #region Audio Loading Helpers
    private IEnumerator LoadWithUnity(string path, int categoryIndex, LoadingStatus status, AudioType audioType)
    {
        string url = new Uri(path).AbsoluteUri;
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                status.HasError = true;
                Plugin.Log.LogError($"UnityRequest Failed to load audio: {www.error}");
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                status.LoadedClips.Add(clip);
                status.LoadedFiles++;
                
                if (!audioClips.ContainsKey(categoryIndex))
                {
                    audioClips[categoryIndex] = status.LoadedClips;
                }
                Plugin.Log.LogInfo($"Loaded audio: {Path.GetFileName(path)} ({status.LoadedFiles}/{status.ExpectedFiles} for {status.Category})");
            }
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
    private void ClearCache()
    {
        ClearAudioClipCache();
        categoryFailedLoading.Clear();
        m_LoadingStatus.Clear();
    }

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
        Plugin.Log.LogWarning($"Failed to load category {category}: {reason}");
    }

    private void LogLoadingResults()
    {
        if (categoryFailedLoading.Count == 0)
        {
            Plugin.Log.LogInfo("All audios successfully loaded");
        }
        else
        {
            Plugin.Log.LogWarning("Failed to load audio files: " + string.Join(", ", categoryFailedLoading));
        }
    }
    #endregion

    #region Process Monitoring
    private void FinalizeLoading()
    {
        foreach (var status in m_LoadingStatus.Values)
        {
            if (status.HasError || status.LoadedFiles == 0)
            {
                categoryFailedLoading.Add(status.Category);
            }
        }
        
        LogLoadingResults();
    }

    private IEnumerator MonitorLoadingProgress()
    {
        while (m_LoadingStatus.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        while (m_LoadingStatus.Values.Any(s => s.LoadedFiles < s.ExpectedFiles && !s.HasError))
        {
            Plugin.Log.LogInfo("Loading audio files...");
            yield return new WaitForEndOfFrame();
        }

        FinalizeLoading();
    }
    #endregion

    #region Shitpost
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
            Plugin.Log.LogWarning($"Invalid audio key: {key}");
            return null;
        }

        if (!audioClips.TryGetValue(key, out var clips) || clips.Count == 0)
            return null;
            
        int randomIndex = UnityEngine.Random.Range(0, clips.Count);
        return clips[randomIndex];
    }
    #endregion



   

    

}

