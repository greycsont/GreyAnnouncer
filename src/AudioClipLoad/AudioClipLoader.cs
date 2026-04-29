using UnityEngine;
using System;
using System.Threading.Tasks;

using GreyAnnouncer.Config;

namespace GreyAnnouncer.AudioClipLoad;

public static partial class AudioClipLoader
{
    public static async Task<AudioClip> LoadAudioClipAsync(string path)
    {
        var audioType = path.TryGetAudioType();
        AudioClip clip = null;
        try {
            if (audioType != AudioType.UNKNOWN)
                clip = await UnitySupport.LoadWithUnityAsync(path, audioType);

            if (clip == null && Setting.isFFmpegSupportEnabled)
                clip = await FFmpegSupport.DecodeAndLoadViaFFmpeg(path);

            if (clip == null)
                LogHelper.LogError($"Failed to load audio clip for {path}");

            return clip;
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Error while loading {path}: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }
}
