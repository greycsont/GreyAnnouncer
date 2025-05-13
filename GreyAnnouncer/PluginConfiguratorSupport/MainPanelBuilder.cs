using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;
using UnityEngine;
using System;


namespace greycsont.GreyAnnouncer;

public static class MainPanelBuilder
{
    private static readonly Color              m_greyColour        = new Color(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Color              m_CyanColour        = new Color(0f, 1f, 1f, 1f);
    private static readonly Color              m_OrangeColour      = new Color(1f, 0.6f, 0.2f, 1f);
    private static          PluginConfigurator m_pluginConfigurator;
    public  static          ConfigHeader       logHeader;

    private static          ConfigPanel        advancedPanel;

    public static void Build(PluginConfigurator config)
    {
        m_pluginConfigurator = config;

        CreateMainSettingSectionTitle();
        CreateCooldownControls();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
        CreateAnnouncerSection();
    }

    private static void CreateMainSettingSectionTitle()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        ConfigHeader mainHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Main Settings");
        mainHeader.textColor    = m_CyanColour;

    }

    private static void CreateCooldownControls()
    {
        var sharedCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Shared Cooldown",
            "sharedPlayCooldown",
            InstanceConfig.sharedPlayCooldown.Value, 0f, 114514f
        );
        sharedCooldown.defaultValue   = InstanceConfig.DEFAULT_SHARED_PLAY_COOLDOWN;
        sharedCooldown.onValueChange += e =>
        {
            InstanceConfig.sharedPlayCooldown.Value = e.value;
            AnnouncerManager.ResetCooldown();
        };

        var individualCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Individual Cooldown",
            "individualPlaycooldown",
            InstanceConfig.individualPlayCooldown.Value, 0f, 1113f
        );
        individualCooldown.defaultValue   = InstanceConfig.DEFAULT_INDIVIDUAL_PLAY_COOLDOWN;
        individualCooldown.onValueChange += e =>
        {
            InstanceConfig.individualPlayCooldown.Value = e.value;
            AnnouncerManager.ResetCooldown();
        };
    }

    private static void CreateAudioControls()
    {
        
        var volumeSlider = new FloatSliderField(
            m_pluginConfigurator.rootPanel,
            "Audio Volume",
            "Audio_Volume",
            Tuple.Create(0f, 1f),
            InstanceConfig.audioSourceVolume.Value,
            2   // 2nd decimal
        );
        volumeSlider.defaultValue   = InstanceConfig.DEFAULT_AUDIO_SOURCE_VOLUME;
        volumeSlider.onValueChange += e =>
        {
            InstanceConfig.audioSourceVolume.Value = e.newValue;
            AudioSourcePool.Instance.UpdateAllActiveSourcesVolume(e.newValue);
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(e.newValue);
        };

// It worked, but not working great as there's ton of audio when from low rank directly to the high rank
// May be add a short cooldown as limitation

        var playOption = new EnumField<PlayOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Play Option",
            "Audio_Play_Option",
            (PlayOptions)InstanceConfig.audioPlayOptions.Value
        );
        playOption.defaultValue = (PlayOptions)InstanceConfig.DEFAULT_AUDIO_PLAY_OPTIONS;
        playOption.onValueChange += e =>
        {
            InstanceConfig.audioPlayOptions.Value = (int)e.value;
        };

        var loadingOption = new EnumField<audioLoadingOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Loading Option",
            "Audio_Loading_Option",
            (audioLoadingOptions)InstanceConfig.audioLoadingOptions.Value
        );
        loadingOption.defaultValue = (audioLoadingOptions)InstanceConfig.DEFAULT_AUDIO_LOADING_OPTIONS;
        loadingOption.onValueChange += e =>
        {
            InstanceConfig.audioLoadingOptions.Value = (int)e.value;
            if (e.value.Equals((audioLoadingOptions.Load_then_Play)))
            {
                Plugin.log.LogInfo("Clear audio clip cache");
                ClearAudioClipsCache();
            }
            if (e.value.Equals(audioLoadingOptions.Preload_and_Play))
            {
                Plugin.log.LogInfo("Reloading all announcer audio");
                ReloadAllAnnouncers();
            }
        };

        var audioButtonArray = new ButtonArrayField(
            m_pluginConfigurator.rootPanel,
            "audio_button_array",
            3,
            new float[] { 0.4f, 0.4f, 0.2f },
            new string[] { "Open Audio Folder", "Reload Audio", "Advance" }
            // 2 button
            // width of two buttons ( sum = 1f ) 
            // Two button's text
        );
        audioButtonArray.OnClickEventHandler(0).onClick += () =>
        {
            PathManager.OpenDirectory(InstanceConfig.audioFolderPath.Value);
        };
        audioButtonArray.OnClickEventHandler(1).onClick += () =>
        {
            ReloadAllAnnouncers();
        };
        audioButtonArray.OnClickEventHandler(2).onClick += () =>
        {
            advancedPanel.OpenPanel();
        };

    }

    private static void CreateAdvancedOptionPanel()
    {
        advancedPanel = new ConfigPanel(m_pluginConfigurator.rootPanel, "Advanced Option", "Advanced_Option");
        advancedPanel.hidden = true;
        new ConfigSpace(advancedPanel, 15f);

        var audioFolderPath = new StringField(
            advancedPanel,
            "Audio Folder Path",
            "Audio_Folder_Path",
            InstanceConfig.audioFolderPath.Value
        );
        audioFolderPath.defaultValue   = InstanceConfig.DEFAULT_AUDIO_FOLDER_PATH;
        audioFolderPath.onValueChange += e =>
        {
            InstanceConfig.audioFolderPath.Value = e.value;
            AnnouncerManager.UpdateAllAnnouncerPaths();
        };

        var lowpassToggle   = new BoolField(
            advancedPanel,
            "Muffle When Under Water",
            "LowPassFilter_Enabled",
            InstanceConfig.isLowPassFilterEnabled.Value
        );
        lowpassToggle.defaultValue   = true;
        lowpassToggle.onValueChange += (e) =>
        {
            InstanceConfig.isLowPassFilterEnabled.Value = e.value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        };

        var audioRandomizationToggle = new BoolField(
            advancedPanel, 
            "Audio Randomlization", 
            "Audio_Randomlization", 
            InstanceConfig.isAudioRandomizationEnabled.Value
        );
        audioRandomizationToggle.defaultValue   = false;
        audioRandomizationToggle.onValueChange += (e) =>
        {
            InstanceConfig.isAudioRandomizationEnabled.Value = e.value;
            Plugin.log.LogInfo($"Switch audio randomization : {e.value}");
        }; 
    }

    private static void CreateAnnouncerSection()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);
        var announcerHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Announcer Section");
        announcerHeader.textColor = m_OrangeColour;

        logHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "");
        logHeader.tmpAnchor = TMPro.TextAlignmentOptions.TopLeft;
        logHeader.textSize  = 12;
        logHeader.textColor = m_CyanColour;
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
        AnnouncerManager.ReloadAllAnnouncer();
    }

    private static void ClearAudioClipsCache()
    {
        logHeader.text = string.Empty;
        AnnouncerManager.ClearAudioClipsCache();
    }
    #endregion
}

