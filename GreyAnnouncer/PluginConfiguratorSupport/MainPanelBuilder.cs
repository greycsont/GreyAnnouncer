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
        var sharedCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Shared rank cooldown",
            "sharedRankPlayCooldown",
            InstanceConfig.SharedRankPlayCooldown.Value, 0f, 114514f
        );
        sharedCooldown.defaultValue   = InstanceConfig.DEFAULT_SHARED_RANK_COOLDOWN;
        sharedCooldown.onValueChange += e =>
        {
            InstanceConfig.SharedRankPlayCooldown.Value = e.value;
            AnnouncerManager.ResetCooldown();
        };

        var individualCooldown = new FloatField(
            m_pluginConfigurator.rootPanel,
            "Individual rank cooldown",
            "individualRankPlaycooldown",
            InstanceConfig.IndividualRankPlayCooldown.Value, 0f, 1113f
        );
        individualCooldown.defaultValue   = InstanceConfig.DEFAULT_INDIVIDUAL_RANK_COOLDOWN;
        individualCooldown.onValueChange += e =>
        {
            InstanceConfig.IndividualRankPlayCooldown.Value = e.value;
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
            InstanceConfig.AudioSourceVolume.Value,
            2
        );
        volumeSlider.defaultValue   = InstanceConfig.DEFAULT_AUDIO_SOURCE_VOLUME;
        volumeSlider.onValueChange += e =>
        {
            InstanceConfig.AudioSourceVolume.Value = e.newValue;
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
            (PlayOptions)InstanceConfig.AudioPlayOptions.Value
        );
        playOption.defaultValue = (PlayOptions)InstanceConfig.DEFAULT_AUDIO_PLAY_OPTIONS;
        playOption.onValueChange += e =>
        {
            InstanceConfig.AudioPlayOptions.Value = (int)e.value;
        };

        var audioFolderPath = new StringField(
            m_pluginConfigurator.rootPanel,
            "Audio Folder Path",
            "Audio_Folder_Path",
            InstanceConfig.AudioFolderPath.Value
        );
        audioFolderPath.defaultValue   = InstanceConfig.DEFAULT_AUDIO_FOLDER_PATH;
        audioFolderPath.onValueChange += e =>
        {
            InstanceConfig.AudioFolderPath.Value = e.value;
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
            PathManager.OpenDirectory(InstanceConfig.AudioFolderPath.Value);
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
            InstanceConfig.LowPassFilter_Enabled.Value
        );
        lowpassToggle.defaultValue   = true;
        lowpassToggle.onValueChange += (e) =>
        {
            InstanceConfig.LowPassFilter_Enabled.Value = e.value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        };
    }
    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    private enum PlayOptions
    {
        Override = 0,
        Parallel = 1
    }
}

