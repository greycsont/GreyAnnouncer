using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Diagnostics;

namespace GreyAnnouncer.AudioLoading;

public static class AudioClipLoader
{
    public static async Task<AudioClip> LoadAudioClipAsync(string path, bool FFmpegFlag)
    {
        string extension = Path.GetExtension(path).ToLower();
        AudioType? unityAudioType = GetUnityAudioType(extension);
        AudioClip clip = null;
        try
        {
            if (!FFmpegFlag && unityAudioType.HasValue)
            {
                clip = await UnitySupport.LoadWithUnityAsync(path, unityAudioType.Value);
            }
            /*if (unityAudioType.HasValue)
            {
                LogManager.LogInfo($"Started loading audio: {path}");
                clip = await LoadWithUnityAsync(path, unityAudioType.Value);
            }*/
            else if (FFmpegFlag)
            {
                clip = await FFmpegSupport.DecodeAndLoad(path);
            }
            else
            {
                LogManager.LogError($"Unsupported audio format: 「{extension}」 for {path}");
            }

            if (clip == null) return null;

            //LogManager.LogInfo($"Loaded audio: {Path.GetFileName(path)}");
            return clip;
        }
        catch (Exception ex)
        {
            LogManager.LogError($"Error loading {path}: {ex.Message}\n{ex.StackTrace}");
            return null;
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