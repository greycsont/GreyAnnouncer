using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GreyAnnouncer.AudioLoading;

public static class AudioClipLoader
{
    public static async Task<AudioClip> LoadAudioClipAsync(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        AudioType? unityAudioType = GetUnityAudioType(extension);
        AudioClip clip;
        try
        {
            if (unityAudioType.HasValue)
            {
                LogManager.LogInfo($"Started loading audio: {path}");
                clip = await LoadWithUnityAsync(path, unityAudioType.Value);
            }
            else if (true == true)
            {
                LogManager.LogInfo($"Start to load via FFmpegSupport : {path}");
                clip = await FFmpegSupport.DecodeAndLoad(path);
            }
            else
            {
                LogManager.LogError($"Unsupported audio format: 「{extension}」 for {path}");
            }

            if (clip == null) return null;
        
            LogManager.LogInfo($"Loaded audio: {Path.GetFileName(path)}");
            return clip;
        }
        catch (Exception ex)
        {
            LogManager.LogError($"Error loading {path}: {ex.Message}\n{ex.StackTrace}");
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
                LogManager.LogError($"UnityRequest Failed to load audio: {www.error}");
                return null;
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);
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