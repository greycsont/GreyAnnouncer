using System.Collections.Generic;
using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.Config;

public static class Setting
{
    public static float audioSourceVolume
    {
        get => BepInExConfig.audioSourceVolume.Value;
        set
        {
            BepInExConfig.audioSourceVolume.Value = value;
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(value);
        }
    }

    public static bool isLowPassFilterEnabled
    {
        get => BepInExConfig.isLowPassFilterEnabled.Value;
        set
        {
            BepInExConfig.isLowPassFilterEnabled.Value = value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        }
    }

    public static int audioPlayOptions
    {
        get => BepInExConfig.audioPlayOptions.Value;
        set => BepInExConfig.audioPlayOptions.Value = value;
    }

    public static int audioLoadingStrategy
    {
        get => BepInExConfig.audioLoadingStrategy.Value;
        set
        {
            if (value.Equals((audioLoadingOptions.Load_then_Play)))
            {
                LogHelper.LogInfo("Clear audio clip cache");
                ClearAudioClipsCache();
            }
            if (value.Equals(audioLoadingOptions.Preload_and_Play))
            {
                LogHelper.LogInfo("Reloading all announcer audio");
                ReloadAllAnnouncers();
            }
            BepInExConfig.audioLoadingStrategy.Value = value;
        } 
    }

    public static string announcersPath
    {
        get => BepInExConfig.announcersPath.Value;
        set => BepInExConfig.announcersPath.Value = value;
    }

    public static bool isFFmpegSupportEnabled
    {
        get => BepInExConfig.isFFmpegSupportEnabled.Value;
        set => BepInExConfig.isFFmpegSupportEnabled.Value = value;
    }


    
    #region function
    private static void ReloadAllAnnouncers()
    {
        AnnouncerManager.ReloadAllAnnouncers();
    }

    private static void ClearAudioClipsCache()
    {
        AnnouncerManager.ClearAudioClipsCache();
    }
    #endregion
}