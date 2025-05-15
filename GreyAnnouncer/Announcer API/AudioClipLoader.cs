using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace greycsont.GreyAnnouncer;

public static class AudioClipLoader
{
    public static async Task<AudioClip> LoadAudioClipAsync(string path)
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

    private static async Task<AudioClip> LoadWithUnityAsync(string path, AudioType audioType)
    {
        string url = new Uri(path).AbsoluteUri;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(10);
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
    
    private static AudioType? GetUnityAudioType(string extension)
    {
        return extension switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            ".aiff" => AudioType.AIFF,
            ".aif" => AudioType.AIFF,
            ".acc" => AudioType.ACC,
            _ => null
        };
    }
}