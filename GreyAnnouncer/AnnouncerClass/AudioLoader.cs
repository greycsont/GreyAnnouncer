using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;
using UnityEngine.Networking;
using CSCore.Codecs;
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
    private          Dictionary<string, string[]>  m_audioFileNames;
    private          string                        m_audioPath;
    #endregion

    #region Constructor
    public AudioLoader(string audioPath, string[] audioCategories, Dictionary<string, string[]> audioFileNames)
    {
        this.m_audioPath      = audioPath;
        this.audioCategories  = audioCategories;
        this.m_audioFileNames = audioFileNames;
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
        m_LoadingStatus.Clear();
        
        for (int i = 0; i < audioCategories.Length; i++)
        {
            LoadCategory(i, audioCategories[i]);
        }

        CoroutineRunner.Instance.StartCoroutine(MonitorLoadingProgress());
    }
    

    private void LoadCategory(int index, string category)
    {
        if (!m_audioFileNames.TryGetValue(category, out var fileNames))
        {
            LogCategoryFailure(category, "No file names configured");
            return;
        }

        var status = new LoadingStatus { Category = category };
        m_LoadingStatus[category] = status;

        foreach (var fileName in fileNames)
        {
            if (TryStartLoadingFile(fileName, index, status))
            {
                status.ExpectedFiles++;
            }
        }

        if (status.ExpectedFiles == 0)
        {
            LogCategoryFailure(category, "No valid files found");
        }
    }


    private bool TryStartLoadingFile(string fileName, int categoryIndex, LoadingStatus status)
    {
        var filePath = CheckAudioWithExtension(m_audioPath, fileName);
        if (!File.Exists(filePath))
        {
            Plugin.Log.LogWarning($"Audio file not found: {filePath}");
            return false;
        }

        var coroutine = TryLoadWithUnityRequest(filePath, categoryIndex, status);
        CoroutineRunner.Instance.StartCoroutine(coroutine);
        Plugin.Log.LogInfo($"Started loading audio: {filePath}");
        return true;
    }

    private IEnumerator TryLoadWithUnityRequest(string path, int categoryIndex, LoadingStatus status)
    {
        string url = new Uri(path).AbsoluteUri;
        AudioType audioType = GetAudioTypeFromExtension(url);
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                status.HasError = true;
                Plugin.Log.LogError($"Failed to load audio: {www.error}");
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

    private bool TryLoadWithCSCore(string path, int categoryIndex, LoadingStatus status, out AudioClip clip)
    {
        try
        {
            using (var soundSource = CodecFactory.Instance.GetCodec(path))
            {
                // Create a byte buffer to read the data
                var byteBuffer = new byte[soundSource.Length];
                soundSource.Read(byteBuffer, 0, byteBuffer.Length);

                // Convert byte data to float data
                var sampleBuffer = new float[byteBuffer.Length / 2]; // Assuming 16-bit samples, adjust if needed
                for (int i = 0; i < sampleBuffer.Length; i++)
                {
                    // Convert the byte data (e.g., 16-bit PCM) to float (-1.0f to 1.0f range)
                    sampleBuffer[i] = BitConverter.ToInt16(byteBuffer, i * 2) / 32768f;
                }

                // Create AudioClip
                clip = AudioClip.Create(
                    Path.GetFileNameWithoutExtension(path),
                    sampleBuffer.Length,
                    soundSource.WaveFormat.Channels,
                    soundSource.WaveFormat.SampleRate,
                    false
                );

                // Set audio data directly as float array
                clip.SetData(sampleBuffer, 0);

                status.LoadedClips.Add(clip);
                status.LoadedFiles++;

                if (!audioClips.ContainsKey(categoryIndex))
                {
                    audioClips[categoryIndex] = status.LoadedClips;
                }

                Plugin.Log.LogInfo($"Loaded audio with CSCore: {Path.GetFileName(path)}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"CSCore loading failed: {ex.Message}");
            clip = null;
            return false;
        }
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

    
    private void LogCategoryFailure(string category, string reason)
    {
        categoryFailedLoading.Add(category);
        Plugin.Log.LogWarning($"Failed to load category {category}: {reason}");
    }

    private IEnumerator MonitorLoadingProgress()
    {
        while (m_LoadingStatus.Values.Any(s => s.LoadedFiles < s.ExpectedFiles && !s.HasError))
        {
            yield return new WaitForEndOfFrame();
        }

        FinalizeLoading();
    }

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


    private class LoadingStatus
    {
        public string          Category      { get; set;         }
        public int             ExpectedFiles { get; set;         }
        public int             LoadedFiles   { get; set;         }
        public bool            HasError      { get; set;         }
        public List<AudioClip> LoadedClips   { get; private set; } = new List<AudioClip>();
    }

    private Dictionary<string, LoadingStatus> m_LoadingStatus = new Dictionary<string, LoadingStatus>();

}


