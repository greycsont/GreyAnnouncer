using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;
using UnityEngine;
using System;


namespace greycsont.GreyAnnouncer;

public static class MainPanelBuilder
{
    private static PluginConfigurator m_pluginConfigurator;
    public  static ConfigHeader        logHeader;

    public static void Build(PluginConfigurator config)
    {
        m_pluginConfigurator = config;

        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        ConfigHeader mainHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Main Settings");
        mainHeader.textColor    = HeaderColor;

        CreateCooldownControls();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
    }

    private static void CreateCooldownControls()
    {
        logHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "");
        logHeader.tmpAnchor = TMPro.TextAlignmentOptions.TopLeft;
        logHeader.textSize  = 12;

        var sharedCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Shared rank cooldown",
            "sharedRankPlayCooldown",
            InstanceConfig.sharedRankPlayCooldown.Value, 0f, 114514f
        );
        sharedCooldown.defaultValue   = InstanceConfig.DEFAULT_SHARED_RANK_COOLDOWN;
        sharedCooldown.onValueChange += e =>
        {
            InstanceConfig.sharedRankPlayCooldown.Value = e.value;
            AnnouncerManager.ResetCooldown();
        };

        var individualCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Individual rank cooldown",
            "individualRankPlaycooldown",
            InstanceConfig.individualRankPlayCooldown.Value, 0f, 1113f
        );
        individualCooldown.defaultValue   = InstanceConfig.DEFAULT_INDIVIDUAL_RANK_COOLDOWN;
        individualCooldown.onValueChange += e =>
        {
            InstanceConfig.individualRankPlayCooldown.Value = e.value;
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
            2
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
//
//
//
//
//
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
            if (e.value == audioLoadingOptions.Load_and_Play)
            {
                AnnouncerManager.ClearAudioClipsCache();
            }
            if (e.value == audioLoadingOptions.Preload_and_Play)
            {
                AnnouncerManager.ReloadAllAnnouncers();
            }
            InstanceConfig.audioLoadingOptions.Value = (int)e.value;
        };

        var audioFolderPath = new StringField(
            m_pluginConfigurator.rootPanel,
            "Audio Folder Path",
            "Audio_Folder_Path",
            InstanceConfig.audioFolderPath.Value
        );
        audioFolderPath.defaultValue   = InstanceConfig.DEFAULT_AUDIO_FOLDER_PATH;
        audioFolderPath.onValueChange += e =>
        {
            InstanceConfig.audioFolderPath.Value = e.value;
            AnnouncerManager.UpdateAllAnnouncerPaths(e.value);
        };

        var audioButtonArray = new ButtonArrayField(
            m_pluginConfigurator.rootPanel,
            "audio_button_array",
            2,
            new float[] { 0.5f, 0.5f },
            new string[] { "Open audio folder", "Reload Audio" }
        );
        audioButtonArray.OnClickEventHandler(0).onClick += () =>
        {
            PathManager.OpenDirectory(InstanceConfig.audioFolderPath.Value);
        };
        audioButtonArray.OnClickEventHandler(1).onClick += () =>
        {
            AnnouncerManager.ReloadAllAnnouncers();
        };

    }

    private static void CreateAdvancedOptionPanel()
    {
        ConfigPanel advancedPanel = new ConfigPanel(m_pluginConfigurator.rootPanel, "Advanced Option", "Advanced_Option");

        ConfigHeader header       = new ConfigHeader(advancedPanel, "Audio Frequency Filter");
        header.textColor          = HeaderColor;

        BoolField lowpassToggle   = new BoolField(
            advancedPanel,
            "Filtering when under water",
            "LowPassFilter_Enabled",
            InstanceConfig.isLowPassFilterEnabled.Value
        );
        lowpassToggle.defaultValue   = true;
        lowpassToggle.onValueChange += (e) =>
        {
            InstanceConfig.isLowPassFilterEnabled.Value = e.value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        };
    }
    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    private enum PlayOptions
    {
        Override = 0,
        Parallel = 1
    }

    private enum audioLoadingOptions
    {
        Load_and_Play = 0,
        Preload_and_Play = 1
    }
}

