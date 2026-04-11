using System;
using GreyAnnouncer.AnnouncerCore;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.Config;

public static class Setting
{
    public static Action syncUI;

    public static float audioSourceVolume
    {
        get => PluginSettings.Instance.AudioSourceVolume;
        set
        {
            PluginSettings.Instance.AudioSourceVolume = value;
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(value);
            SaveSettings();
        }
    }

    public static bool isLowPassFilterEnabled
    {
        get => PluginSettings.Instance.IsLowPassFilterEnabled;
        set
        {
            PluginSettings.Instance.IsLowPassFilterEnabled = value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
            SaveSettings();
        }
    }

    public static int audioPlayOptions
    {
        get => PluginSettings.Instance.AudioPlayOptions;
        set
        {
            PluginSettings.Instance.AudioPlayOptions = value;
            SaveSettings();
        }
    }

    public static int audioLoadingStrategy
    {
        get => PluginSettings.Instance.AudioLoadingStrategy;
        set
        {
            if (value.Equals((int)AudioLoadOptions.Load_then_Play))
            {
                LogHelper.LogInfo("Clear audio clip cache");
                AnnouncerManager.ClearAudioClipsCache();
            }
            if (value.Equals((int)AudioLoadOptions.Preload_and_Play))
            {
                LogHelper.LogInfo("Reloading all announcer audio");
                AnnouncerManager.ReloadAllAnnouncers();
            }
            PluginSettings.Instance.AudioLoadingStrategy = value;
            SaveSettings();
        }
    }
    public static bool isFFmpegSupportEnabled
    {
        get => PluginSettings.Instance.IsFFmpegSupportEnabled;
        set
        {
            PluginSettings.Instance.IsFFmpegSupportEnabled = value;
            SaveSettings();
        }
    }

    public static string announcersPath => PathHelper.GetCurrentPluginPath("announcers");

    private static void SaveSettings()
    {
        PluginSettings.Save();
        syncUI?.Invoke();
    }

}
