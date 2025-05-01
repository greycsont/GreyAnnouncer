using System.IO;
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
    #region Public Properties
    public string[]                         audioCategories       { get; private set; }
    public HashSet<string>                  categoryFailedLoading { get; private set; } = new HashSet<string>();
    public Dictionary<int, List<AudioClip>> audioClips            { get; private set; } = new Dictionary<int, List<AudioClip>>();
    #endregion


    #region Private Fields
    private readonly string[]                      m_supportedExtensions = new string[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };
    private           Dictionary<string, string[]> m_audioFileNames;
    private          string                        m_audioPath;
    #endregion

    #region Constructor
    public AudioLoader(string audioPath, string[] audioCategories, Dictionary<string, string[]> audioFileNames)
    {
        this.m_audioPath     = audioPath;
        this.audioCategories = audioCategories;
        this.m_audioFileNames  = audioFileNames;
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
        StartLoading();
    }

    public AudioClip TryToGetAudioClip(int key)
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

    public void UpdateAudioFileNames(Dictionary<string, string[]> newAudioFileNames)
    {
        this.m_audioFileNames = newAudioFileNames;
    }
    #endregion


    #region Loading Logic
    private void ClearCache()
    {
        ClearAudioClipCache();
        categoryFailedLoading.Clear();
    }


    private void ValidateAndPrepareDirectory()
    {
        if (!Directory.Exists(m_audioPath))
        {
            Plugin.Log.LogWarning($"Audio directory not found: {m_audioPath}");
            Directory.CreateDirectory(m_audioPath);
        }
    }

    private void StartLoading()
    {
        TryToFetchAudios(m_audioPath);
        LoggingAudioLodingResults();

        if (categoryFailedLoading.SetEquals(audioCategories))
        {
            Plugin.Log.LogWarning($"No audio files found in the directory : {m_audioPath}.");
        }
    }
    
    private Dictionary<string, List<IEnumerator>> _pendingLoads = new Dictionary<string, List<IEnumerator>>();

    private void TryToFetchAudios(string audioPath)
    {
        _pendingLoads.Clear();
        
        for (int i = 0; i < audioCategories.Length; i++)
        {
            string category = audioCategories[i];
            if (!m_audioFileNames.TryGetValue(category, out var categoryAudios) || categoryAudios.Length == 0)
            {
                Plugin.Log.LogWarning($"You forget to set the audio name of {category}");
                continue;
            }

            var audioList = new List<AudioClip>();
            var coroutines = new List<IEnumerator>();
            
            foreach (var audioName in categoryAudios)
            {
                var filePath = CheckAudioWithExtension(audioPath, audioName);
                if (File.Exists(filePath))
                {
                    var coroutine = LoadAudioClip(filePath, i, audioList);
                    coroutines.Add(coroutine);
                    CoroutineRunner.Instance.StartCoroutine(coroutine);
                    Plugin.Log.LogInfo($"Started loading audio: {filePath}"); // Debug log
                }
                else
                {
                    Plugin.Log.LogWarning($"Audio file not found for {category}: {filePath}");
                }
            }

            if (coroutines.Count > 0)
            {
                _pendingLoads[category] = coroutines;
            }
            else
            {
                categoryFailedLoading.Add(category);
            }
        }

        CoroutineRunner.Instance.StartCoroutine(WaitForAllLoads());
    }

    private IEnumerator WaitForAllLoads()
    {
        while (_pendingLoads.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        LoggingAudioLodingResults();
    }

    private string CheckAudioWithExtension(string audioPath, string audioName)  // Changed parameter type
    {
        string filePath = null;
        foreach (var ext in m_supportedExtensions)
        {
            string potentialPath = Path.Combine(audioPath, audioName + ext);
            if (File.Exists(potentialPath))
            {
                filePath = potentialPath;
                break;
            }
        }
        return filePath;
    }

    private AudioType GetAudioTypeFromExtension(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        switch (extension)
        {
            case ".wav": return AudioType.WAV;
            case ".mp3": return AudioType.MPEG;
            case ".ogg": return AudioType.OGGVORBIS;
            case ".aiff": case ".aif": return AudioType.AIFF;
            default:
                Plugin.Log.LogWarning($"Unsupported audio format: {extension}, defaulting to WAV");
                return AudioType.WAV;
        }

    }

    private IEnumerator LoadAudioClip(string path, int key, List<AudioClip> audioList)
    {
        string url = new Uri(path).AbsoluteUri;
        AudioType audioType = GetAudioTypeFromExtension(url);
        
        Plugin.Log.LogInfo($"Loading audio : {string.Join(", ", m_audioFileNames[audioCategories[key]])} for {audioCategories[key]} from {Uri.UnescapeDataString(url)}");
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Plugin.Log.LogError($"Failed to Load audio : {key}, Error message : {www.error}");
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                audioList.Add(clip);
                if (!audioClips.ContainsKey(key))
                    audioClips[key] = audioList;

                    
            }
        }
    }

    private void LoggingAudioLodingResults()
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

    private void ClearAudioClipCache()
    {
        foreach (var clipList in audioClips.Values)
        {
            foreach (var clip in clipList)
            {
                if (clip != null)
                    UnityEngine.Object.Destroy(clip);   //clear individual clips
            }
        }
        audioClips.Clear(); //clear dictionary
    }
    #endregion


}


