using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;

using GreyAnnouncer.Config;
using System.ComponentModel;

namespace GreyAnnouncer.AudioLoading;

public static partial class AudioClipLoader
{
    public static async Task<AudioClip> LoadAudioClipAsync(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        AudioType unityAudioType = GetUnityAudioType(extension);
        AudioClip clip = null;
        try {
            if (unityAudioType != AudioType.UNKNOWN) {
                clip = await UnitySupport.LoadWithUnityAsync(path, unityAudioType);
                
            } else if (Setting.isFFmpegSupportEnabled) {
                clip = await FFmpegSupport.DecodeAndLoadViaFFmpeg(path);

            } else {
                LogHelper.LogError($"Unsupported audio format: 「{extension}」 for {path}");
                
            }

            if (clip == null) return null;

            //LogManager.LogInfo($"Loaded audio: {Path.GetFileName(path)}");
            return clip;
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Error while loading {path}: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }



    /* for reference:
     * public enum AudioType
     * {
         * UNKNOWN = 0,
         * ACC = 1,
         * AIFF = 2,
         * IT = 10,
         * MOD = 12,
         * MPEG = 13,
         * OGGVORBIS = 14,
         * S3M = 17,
         * WAV = 20,
         * XM = 21,
         * XMA = 22,
         * VAG = 23,
         * AUDIOQUEUE = 24
     * }
     */

    [Description("https://docs.unity3d.com/2022.3/Documentation//ScriptReference/AudioType.html")]
    private static AudioType GetUnityAudioType(string extension)
    {
        return extension switch
        {
            ".aac" => AudioType.ACC, // Advanced Audio Coding https://en.wikipedia.org/wiki/Advanced_Audio_Coding
            ".aiff" => AudioType.AIFF, // Audio Interchange File Format https://en.wikipedia.org/wiki/Audio_Interchange_File_Format
            ".aif" => AudioType.AIFF,
            ".aifc" => AudioType.AIFF,
            ".it" => AudioType.IT, // Impulse Tracker Module https://fileinfo.com/extension/it
            ".mod" => AudioType.MOD, // Module https://en.wikipedia.org/wiki/MOD_%28file_format%29
            ".mp3" => AudioType.MPEG, // MPEG-1/2 Audio Layer III https://en.wikipedia.org/wiki/MP3
            ".mpga" => AudioType.MPEG,
            ".mpeg" => AudioType.MPEG, // https://www.thebroadcastbridge.com/content/entry/20759/standards-part-16-about-mp3-audio-coding-id3-metadata
            ".ogg" => AudioType.OGGVORBIS, // Vorbis https://en.wikipedia.org/wiki/Vorbis
            ".s3m" => AudioType.S3M, // Scream Tracker 3 Module https://en.wikipedia.org/wiki/S3M
            ".wav" => AudioType.WAV, // Waveform
            ".xm" => AudioType.XM,  // Extended Module
            ".xma" => AudioType.XMA, // Xbox Media Audio
            ".vag" => AudioType.VAG, // format for PS1/2/3/4/P https://fileinfo.com/extension/vag
            _ => AudioType.UNKNOWN
        };
    }
}
