using System;
using System.IO;
using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;
namespace GreyAnnouncer.Config;

public static class Setting
{
    public static Action syncUI;
    public static float audioSourceVolume
    {
        get => BepInExConfig.audioSourceVolume.Value;
        set
        {
            BepInExConfig.audioSourceVolume.Value = value;
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(value);
            syncUI?.Invoke();
        }
    }

    public static bool isLowPassFilterEnabled
    {
        get => BepInExConfig.isLowPassFilterEnabled.Value;
        set
        {
            BepInExConfig.isLowPassFilterEnabled.Value = value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
            syncUI?.Invoke();
        }
    }

    public static int audioPlayOptions
    {
        get => BepInExConfig.audioPlayOptions.Value;
        set {
            BepInExConfig.audioPlayOptions.Value = value;
            syncUI?.Invoke();
        }
    }

    public static int audioLoadingStrategy
    {
        get => BepInExConfig.audioLoadingStrategy.Value;
        set
        {
            if (value.Equals((int)AudioLoadOptions.Load_then_Play))
            {
                LogHelper.LogInfo("Clear audio clip cache");
                ClearAudioClipsCache();
            }
            if (value.Equals((int)AudioLoadOptions.Preload_and_Play))
            {
                LogHelper.LogInfo("Reloading all announcer audio");
                ReloadAllAnnouncers();
            }
            BepInExConfig.audioLoadingStrategy.Value = value;
            syncUI?.Invoke();
        } 
    }

    public static string announcersPath
    {
        get => BepInExConfig.announcersPath.Value;
        set 
        {
            if (!Directory.Exists(Path.GetDirectoryName(value)))
            {
                LogHelper.LogWarning("Given Directory not exists");
                return;
            }
            BepInExConfig.announcersPath.Value = value;
            MoveAnnouncersToTargetDirectory(value);
            syncUI?.Invoke();
        }
    }


    public static bool isFFmpegSupportEnabled
    {
        get => BepInExConfig.isFFmpegSupportEnabled.Value;
        set 
        {
            BepInExConfig.isFFmpegSupportEnabled.Value = value;
            syncUI?.Invoke();
        }
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

    private static void MoveAnnouncersToTargetDirectory(string targetRoot)
    {
        foreach (var announcer in AnnouncerIndex.GetAnnouncers())
        {
           FileSystemUtil.CopyDirectoryParallel(announcer, targetRoot);
        }
    }
    #endregion
}