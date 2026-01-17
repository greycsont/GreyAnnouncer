using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;
using UnityEngine;
using System;

using GreyAnnouncer.AnnouncerAPI;

using GreyAnnouncer.AudioSourceComponent;

using GreyAnnouncer.Commands;

using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

public static class MainPanelBuilder
{
    private static readonly Color m_greyColour = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);

    private static readonly Color m_CyanColour = new UnityEngine.Color(0f, 1f, 1f, 1f);

    private static readonly Color m_OrangeColour = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);

    private static readonly Color m_RedColour = new UnityEngine.Color(1f, 0f, 0f, 1f);

    private static PluginConfigurator m_pluginConfigurator;

    public static ConfigHeader logHeader;

    private static ConfigPanel advancedPanel;


    public static void Build(PluginConfigurator config)
    {
        m_pluginConfigurator = config;

        CreateMainSettingSectionTitle();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
        CreateAnnouncerSection();

        CreateDelegateTextFromBackEnd();
    }

    private static void CreateMainSettingSectionTitle()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        ConfigHeader mainHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Main Settings");
        mainHeader.textColor = m_CyanColour;

    }


    private static void CreateAudioControls()
    {

        var volumeSlider = new FloatSliderField(
            m_pluginConfigurator.rootPanel,
            "Master Volume",
            "Audio_Volume",
            Tuple.Create(0f, 1f),
            BepInExConfig.audioSourceVolume.Value,
            2   // 2nd decimal
        );
        volumeSlider.defaultValue = BepInExConfig.DEFAULT_AUDIO_SOURCE_VOLUME;
        volumeSlider.onValueChange += e =>
        {
            BepInExConfig.audioSourceVolume.Value = e.newValue;
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(e.newValue);
        };

        // It worked, but not working great as there's ton of audio when from low rank directly to the high rank
        // May be add a short cooldown as limitation

        var playOption = new EnumField<PlayOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Play Option",
            "Audio_Play_Option",
            (PlayOptions)BepInExConfig.audioPlayOptions.Value
        );
        playOption.defaultValue = (PlayOptions)BepInExConfig.DEFAULT_AUDIO_PLAY_OPTIONS;
        playOption.onValueChange += e =>
        {
            BepInExConfig.audioPlayOptions.Value = (int)e.value;
        };

        var loadingOption = new EnumField<audioLoadingOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Loading Option",
            "Audio_Loading_Option",
            (audioLoadingOptions)BepInExConfig.audioLoadingOptions.Value
        );
        loadingOption.defaultValue = (audioLoadingOptions)BepInExConfig.DEFAULT_AUDIO_LOADING_OPTIONS;
        loadingOption.onValueChange += e =>
        {
            BepInExConfig.audioLoadingOptions.Value = (int)e.value;
            if (e.value.Equals((audioLoadingOptions.Load_then_Play)))
            {
                LogManager.LogInfo("Clear audio clip cache");
                ClearAudioClipsCache();
            }
            if (e.value.Equals(audioLoadingOptions.Preload_and_Play))
            {
                LogManager.LogInfo("Reloading all announcer audio");
                ReloadAllAnnouncers();
            }
        };

        var audioButtonArray = new ButtonArrayField(
            m_pluginConfigurator.rootPanel,
            "audio_button_array",
            3,
            new float[] { 0.5f, 0.5f },
            new string[] { "Open Audio Folder", "Reload", "Advance" }
        );
        audioButtonArray.OnClickEventHandler(0).onClick += () 
            => AnnouncerManager.ReloadAllAnnouncers();

        audioButtonArray.OnClickEventHandler(1).onClick += () 
            => advancedPanel.OpenPanel();


    }

    private static void CreateAdvancedOptionPanel()
    {
        advancedPanel = new ConfigPanel(m_pluginConfigurator.rootPanel, "Advanced Option", "Advanced_Option");
        advancedPanel.hidden = true;
        new ConfigSpace(advancedPanel, 15f);

        var lowpassToggle = new BoolField(
            advancedPanel,
            "Muffle When Under Water",
            "LowPassFilter_Enabled",
            BepInExConfig.isLowPassFilterEnabled.Value
        );
        lowpassToggle.defaultValue = true;
        lowpassToggle.onValueChange += (e) =>
        {
            BepInExConfig.isLowPassFilterEnabled.Value = e.value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        };

        var ffmpegToggle = new BoolField(
            advancedPanel,
            "FFmpeg Support",
            "FFmpeg_Support",
            BepInExConfig.isFFmpegSupportEnabled.Value
        );
        ffmpegToggle.defaultValue = false;
        ffmpegToggle.onValueChange += (e) =>
        {
            BepInExConfig.isFFmpegSupportEnabled.Value = e.value;
            LogManager.LogInfo($"Switched FFmpeg support : {e.value}");
        };

        var emergencyHeader = new ConfigHeader(advancedPanel, "Emergency");
        emergencyHeader.textColor = m_RedColour;

        var stopAudioSourceButton = new ButtonField(advancedPanel, "Stop All Audio Source", "Stop_All_Audio_Source");
        stopAudioSourceButton.onClick += () 
            => AudioSourceManager.StopAllAudioSource();
    }

    private static void CreateAnnouncerSection()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);
        var announcerHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Announcer Section");
        announcerHeader.textColor = m_OrangeColour;

        logHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "");
        logHeader.tmpAnchor = TMPro.TextAlignmentOptions.TopLeft;
        logHeader.textSize = 12;
        logHeader.textColor = m_CyanColour;
    }

    private static void CreateDelegateTextFromBackEnd()
    {
        AudioLoader.onPluginConfiguratorLogUpdated = log =>
        {
            logHeader.text = log + "\n";
        };
    }

    #region enum
    private enum PlayOptions
    {
        Override = 0,
        Parallel = 1
    }

    private enum audioLoadingOptions
    {
        Load_then_Play = 0,
        Preload_and_Play = 1
    }
    #endregion


    #region function
    private static void ReloadAllAnnouncers()
    {
        logHeader.text = string.Empty;
        AnnouncerManager.ReloadAllAnnouncers();
    }

    private static void ClearAudioClipsCache()
    {
        logHeader.text = string.Empty;
        AnnouncerManager.ClearAudioClipsCache();
    }
    #endregion
}

