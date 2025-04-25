using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;


namespace greycsont.GreyAnnouncer;

public static class MainPanelBuilder
{
    private static PluginConfigurator _config;

    public static void Build(PluginConfigurator config)
    {
        _config = config;

        ConfigHeader mainHeader = new ConfigHeader(_config.rootPanel, "Main Settings");
        mainHeader.textColor    = HeaderColor;

        CreateCooldownControls();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
    }

    private static void CreateCooldownControls()
    {
        var sharedCooldown = new FloatField(
            _config.rootPanel,
            "Shared rank cooldown",
            "sharedRankPlayCooldown",
            InstanceConfig.SharedRankPlayCooldown.Value, 0f, 114514f
        );
        sharedCooldown.defaultValue   = InstanceConfig.DEFAULT_SHARED_RANK_COOLDOWN;
        sharedCooldown.onValueChange += e =>
        {
            InstanceConfig.SharedRankPlayCooldown.Value = e.value;
            RankAnnouncer.ResetTimerToZero();
        };

        var individualCooldown = new FloatField(
            _config.rootPanel,
            "Individual rank cooldown",
            "individualRankPlaycooldown",
            InstanceConfig.IndividualRankPlayCooldown.Value, 0f, 1113f
        );
        individualCooldown.defaultValue   = InstanceConfig.DEFAULT_INDIVIDUAL_RANK_COOLDOWN;
        individualCooldown.onValueChange += e =>
        {
            InstanceConfig.IndividualRankPlayCooldown.Value = e.value;
            RankAnnouncer.ResetTimerToZero();
        };
    }

    private static void CreateAudioControls()
    {
        var volumeSlider = new FloatSliderField(
            _config.rootPanel,
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

        var audioFolderPath = new StringField(
            _config.rootPanel,
            "Audio Folder Path",
            "Audio_Folder_Path",
            InstanceConfig.AudioFolderPath.Value
        );
        audioFolderPath.defaultValue = InstanceConfig.DEFAULT_AUDIO_FOLDER_PATH;
        audioFolderPath.onValueChange += e =>
        {
            InstanceConfig.AudioFolderPath.Value = e.value;
            RankAnnouncer.UpdateAudioFolderPath(e.value);
        };

        var openAudioFolder = new ButtonField(
            _config.rootPanel,
            "Open audio folder",
            "open_audio_folder");
        openAudioFolder.onClick += () =>
        {
            PathManager.OpenDirectory(InstanceConfig.AudioFolderPath.Value);
        };


        var reloadButton = new ButtonField(
            _config.rootPanel, 
            "Reload Audio", 
            "reload_audio");
        reloadButton.onClick += () =>
        {
            RankAnnouncer.ReloadAudio();
        };
        
        
    }

    private static void CreateAdvancedOptionPanel()
    {
        ConfigPanel advancedPanel = new ConfigPanel(_config.rootPanel, "Advanced Option", "Advanced_Option");

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
}

