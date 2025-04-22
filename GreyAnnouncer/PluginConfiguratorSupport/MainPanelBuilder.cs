using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;
using greycsont.GreyAnnouncer;
using UnityEngine;
using System;


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
    }

    private static void CreateCooldownControls()
    {
        var sharedCooldown = new FloatField(
            _config.rootPanel,
            "Shared rank cooldown",
            "sharedRankPlayCooldown",
            InstanceConfig.SharedRankPlayCooldown.Value, 0f, 114514f
        );
        sharedCooldown.defaultValue   = 0f;
        sharedCooldown.onValueChange += e =>
        {
            InstanceConfig.SharedRankPlayCooldown.Value = e.value;
            Announcer.ResetTimerToZero();
        };

        var individualCooldown = new FloatField(
            _config.rootPanel,
            "Individual rank cooldown",
            "individualRankPlaycooldown",
            InstanceConfig.IndividualRankPlayCooldown.Value, 0f, 1113f
        );
        individualCooldown.defaultValue   = 3f;
        individualCooldown.onValueChange += e =>
        {
            InstanceConfig.IndividualRankPlayCooldown.Value = e.value;
            Announcer.ResetTimerToZero();
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
        volumeSlider.defaultValue   = 1f;
        volumeSlider.onValueChange += e =>
        {
            InstanceConfig.AudioSourceVolume.Value = e.newValue;
            Announcer.UpdateAudioSourceVolume(e.newValue);
        };

        var reloadButton = new ButtonField(_config.rootPanel, "Reload Audio", "reload_audio");
        reloadButton.onClick += () =>
        {
            Announcer.ReloadAudio();
        };
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}
