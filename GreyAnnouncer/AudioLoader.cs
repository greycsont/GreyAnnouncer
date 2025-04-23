using System.IO;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace greycsont.GreyAnnouncer;

public class AudioLoader
{
    private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();

    private static readonly string[] supportedExtensions = new string[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };

    private static void TryToFindDirectoryOfAudioFolder(string audioPath, string[] dictionaryPointer)
    {
        if (!Directory.Exists(audioPath))
        {
            Plugin.Log.LogWarning($"audio directory not found : {audioPath}");
            Directory.CreateDirectory(audioPath);
            return;
        }

        TryToFetchAudios(audioPath, dictionaryPointer);
    }
    private static void TryToFetchAudios(string audioPath, string[] dictionaryPointer)
    {

        for (int i = 0; i < dictionaryPointer.Length; i++)
        {
            if (JsonSetting.Settings.RankSettings.audioNames[i] == "")
                Plugin.Log.LogWarning($"You forget to set the audio name of rank {dictionaryPointer[i]} ");

            var filePath = CheckAudioWithExtension(audioPath, i);

            if (File.Exists(filePath))
            {
                CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(filePath, i, dictionaryPointer));
            }
        }
    }

    private static string CheckAudioWithExtension(string audioPath, int index)
    {
        string filePath = null;
        foreach (var ext in supportedExtensions)
        {
            string potentialPath = Path.Combine(audioPath, JsonSetting.Settings.RankSettings.audioNames[index] + ext);
            if (File.Exists(potentialPath))
            {
                filePath = potentialPath;
                break;
            }
        }
        return filePath;
    }

    private static AudioType GetAudioTypeFromExtension(string path)
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

    private static IEnumerator LoadAudioClip(string path, int key, string[] dictionaryPointer)
    {
        string url = new Uri(path).AbsoluteUri;

        AudioType audioType = GetAudioTypeFromExtension(url);
        Plugin.Log.LogInfo($"Loading audio : {JsonSetting.Settings.RankSettings.audioNames[key]} for {dictionaryPointer[key]} from {Uri.UnescapeDataString(url)}");
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Plugin.Log.LogError($"Failed to Load audio : {key}, Error message : {www.error}");
            else
                audioClips[key] = DownloadHandlerAudioClip.GetContent(www);
        }
    }
}