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
    public           Dictionary<int, AudioClip> audioClips                  = new Dictionary<int, AudioClip>();
    private readonly string[]                   supportedExtensions         = new string[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };
    public           HashSet<string>            categoreFailedLoading       = new HashSet<string>();
    public           string[]                   audioCategories;
    public           string[]                   audioFileNames;
    private          string                     audioPath;


    public AudioLoader(string audioPath, string[] audioCategories, string[] audioFileNames)
    {
        this.audioPath       = audioPath;
        this.audioCategories = audioCategories;
        this.audioFileNames  = audioFileNames;
    }

    #region Methods
    public void UpdateAudioPath(string newAudioPath)
    {
        if (newAudioPath == null || newAudioPath.Length == 0)
        {
            Plugin.Log.LogError("Cannot update with empty or null paths");
            return;
        }

        Plugin.Log.LogInfo($"Updating audio paths and reloading audio...");
        this.audioPath = newAudioPath;
    }

    public void FindAvailableAudio()
    {
        ClearCache();

        TryToFindDirectoryOfAudioFolder(audioPath);
        TryToFetchAudios(audioPath);
        LoggingAudioLodingResults();

        if (categoreFailedLoading.SetEquals(audioCategories))
        {
            Plugin.Log.LogWarning($"No audio files found in the directory : {audioPath}.");
        }
    }

    public AudioClip TryToGetAudioClip(int key)
    {
        return audioClips.TryGetValue(key, out AudioClip clip) ? clip : null;
    }

    public void UpdateAudioFileNames(string[] newAudioFileNames)
    {
        this.audioFileNames = newAudioFileNames;
    }
    #endregion


    #region Find Available Audio related
    private void ClearCache()
    {
        ClearAudioClipCache();
        categoreFailedLoading.Clear();
    }

    public void TryToFindDirectoryOfAudioFolder(string audioPath)
    {
        if (!Directory.Exists(audioPath))
        {
            Plugin.Log.LogWarning($"audio directory not found : {audioPath}");
            Directory.CreateDirectory(audioPath);
            return;
        }

    }
    
    private void TryToFetchAudios(string audioPath)
    {

        for (int i = 0; i < audioCategories.Length; i++)
        {
            if (audioFileNames[i] == "")
                Plugin.Log.LogWarning($"You forget to set the audio name of {audioCategories[i]} ");

            var filePath = CheckAudioWithExtension(audioPath, i);

            if (File.Exists(filePath))
            {
                CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(filePath, i));
            }
            else
            {
                Plugin.Log.LogWarning($"Audio file not found for {audioCategories[i]}: {filePath}");
                categoreFailedLoading.Add(audioCategories[i]);
            }
        }
    }

    private string CheckAudioWithExtension(string audioPath, int index)
    {
        string filePath = null;
        foreach (var ext in supportedExtensions)
        {
            string potentialPath = Path.Combine(audioPath, audioFileNames[index] + ext);
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

    private IEnumerator LoadAudioClip(string path, int key)
    {
        string url = new Uri(path).AbsoluteUri;

        AudioType audioType = GetAudioTypeFromExtension(url);
        Plugin.Log.LogInfo($"Loading audio : {audioFileNames[key]} for {audioCategories[key]} from {Uri.UnescapeDataString(url)}");
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Plugin.Log.LogError($"Failed to Load audio : {key}, Error message : {www.error}");
            else
                audioClips[key] = DownloadHandlerAudioClip.GetContent(www);
        }

    }

    private void LoggingAudioLodingResults()
    {
        if (categoreFailedLoading.Count == 0)
        {
            Plugin.Log.LogInfo("All audios successfully loaded");
        }
        else
        {
            Plugin.Log.LogWarning("Failed to load audio files: " + string.Join(", ", categoreFailedLoading));
        }
    }

    private void ClearAudioClipCache()
    {
        foreach (var clip in audioClips.Values)
        {
            if (clip != null)
                UnityEngine.Object.Destroy(clip);   //clear clip in the unity's assets
        }
        audioClips.Clear(); //clear dictionary
    }
    #endregion




}