using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using UnityEngine;

using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.PluginConfiguratorGUI;

public static class RegisterRankAnnouncerPagev2
{
    private static PluginConfigurator _pluginConfigurator;
    private static string _title;
    private static AudioAnnouncer _announcer;
    public static void Build(string title, AudioAnnouncer announcer)
    {
        _pluginConfigurator = PluginConfiguratorEntry.config;
        _title = title;
        _announcer = announcer;

        ConfigPanel panel = new ConfigPanel(_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(panel, 15f);

        ConfigHeader titleHeader = new ConfigHeader(panel, _title);
        titleHeader.textColor = HeaderColor;

        var audioPathField = new StringField(
            panel,
            "Audio Path",
            GuidPrefixAdder.AddPrefixToGUID("AudioPath", _title),
            _announcer._announcerConfig.AudioPath
        );
        audioPathField.defaultValue = PathManager.GetCurrentPluginPath();
        audioPathField.onValueChange += e =>
        {
            _announcer._announcerConfig.AudioPath = e.value;
            SomethingAfterUpdateJson();
        };

        var randomizeAudioField = new BoolField(
            panel,
            "Randomize Audio On Play",
            GuidPrefixAdder.AddPrefixToGUID("RandomizeAudioOnPlay", _title),
            _announcer._announcerConfig.RandomizeAudioOnPlay
        );
        randomizeAudioField.defaultValue = false;
        randomizeAudioField.onValueChange += e =>
        {
            _announcer._announcerConfig.RandomizeAudioOnPlay = e.value;
            SomethingAfterUpdateJson();
        };


        foreach (var category in _announcer._announcerConfig.CategoryAudioMap)
        {
            ConfigHeader header = new ConfigHeader(panel, category.Value.DisplayName);
            header.textColor = new Color(0,1,1);

            var Afield = CreateEnabledField(
                panel,
                category.Key,
                _announcer._announcerConfig,
                true
            );

            var Bfield = CreateVolumeField(
                panel,
                category.Key,
                _announcer._announcerConfig,
                1
            );

            var Cfield = CreateCooldownField(
                panel,
                category.Key,
                _announcer._announcerConfig,
                3.0f
            );

            var Dfield = CreatePitchMinField(
                panel,
                category.Key,
                _announcer._announcerConfig,
                1
            );

            var Efield = CreatePitchMaxField(
                panel,
                category.Key,
                _announcer._announcerConfig,
                1
            );
            
        }
    }

    private static BoolField CreateEnabledField(ConfigPanel panel,
                                                string guid,
                                                AnnouncerConfig jsonSetting,
                                                bool defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Enabled");
        var field = new BoolField(panel, "Enabled", fullGuid, jsonSetting.CategoryAudioMap[guid].Enabled);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Enabled = e.value;
            }

            SomethingAfterUpdateJson();
        };

        return field;
    }


    private static FloatField CreateVolumeField(ConfigPanel panel,
                                             string guid,
                                             AnnouncerConfig jsonSetting,
                                             float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "VolumeMultiplier");
        var field = new FloatField(panel, "Volume", fullGuid, jsonSetting.CategoryAudioMap[guid].VolumeMultiplier);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].VolumeMultiplier = e.value;
            }

            SomethingAfterUpdateJson();
        };

        return field;
    }

    private static FloatSliderField CreatePitchMinField(ConfigPanel panel,
                                                     string guid,
                                                     AnnouncerConfig jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "PitchMin");
        var field = new FloatSliderField(panel, "Pitch Min", fullGuid, Tuple.Create(0.2f, 2f), jsonSetting.CategoryAudioMap[guid].Pitch[0], 2);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Pitch[0] = e.newValue;
            }

            SomethingAfterUpdateJson();

        };

        return field;
    }

    private static FloatSliderField CreatePitchMaxField(ConfigPanel panel,
                                                     string guid,
                                                     AnnouncerConfig jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "PitchMax");
        var field = new FloatSliderField(panel, "Pitch Max", fullGuid, Tuple.Create(0.2f, 2f), jsonSetting.CategoryAudioMap[guid].Pitch[1], 2);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Pitch[1] = e.newValue;
            }

            SomethingAfterUpdateJson();

        };

        return field;
    }

    private static FloatSliderField CreateCooldownField(ConfigPanel panel,
                                                     string guid,
                                                     AnnouncerConfig jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Cooldown");
        var field = new FloatSliderField(panel, "Cooldown", fullGuid, Tuple.Create(0.2f, 6f), jsonSetting.CategoryAudioMap[guid].Cooldown, 1);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Cooldown = e.newValue;
            }

            SomethingAfterUpdateJson();

        };

        return field;
    }

    private static void SomethingAfterUpdateJson()
    {
        _announcer.UpdateJsonSetting(_announcer._announcerConfig);
        LogManager.LogInfo($"Updated json setting for {_title}");
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}
