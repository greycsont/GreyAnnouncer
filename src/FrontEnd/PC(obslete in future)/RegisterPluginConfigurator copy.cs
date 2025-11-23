using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using UnityEngine;

using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.PluginConfiguratorGUI;

public static class RegisterRankAnnouncerPagev2
{
    private static PluginConfigurator    _pluginConfigurator;
    private static string                _title;
    private static AudioAnnouncer        _announcer;
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
            _announcer._jsonSetting.AudioPath
        );
        audioPathField.defaultValue = PathManager.GetCurrentPluginPath();
        audioPathField.onValueChange += e =>
        {
            _announcer._jsonSetting.AudioPath = e.value;
            SomethingAfterUpdateJson();
        };

        var randomizeAudioField = new BoolField(
            panel,
            "Randomize Audio On Play",
            GuidPrefixAdder.AddPrefixToGUID("RandomizeAudioOnPlay", _title),
            _announcer._jsonSetting.RandomizeAudioOnPlay
        );
        randomizeAudioField.defaultValue = false;
        randomizeAudioField.onValueChange += e =>
        {
            _announcer._jsonSetting.RandomizeAudioOnPlay = e.value;
            SomethingAfterUpdateJson();
        };


        foreach (var category in _announcer._jsonSetting.CategoryAudioMap)
        {
            var field = CreateEnabledField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcer._jsonSetting,
                true
            );
        }

        ConfigHeader volumeHeader = new ConfigHeader(panel, "Volume Settings");
        volumeHeader.textColor = new UnityEngine.Color(0f, 1f, 1f, 1f);

        foreach (var category in _announcer._jsonSetting.CategoryAudioMap)
        {
            var field = CreateVolumeField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcer._jsonSetting,
                1
            );
        }

        ConfigHeader pitchMinHeader = new ConfigHeader(panel, "Pitch Min");
        pitchMinHeader.textColor = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);
        
        foreach (var category in _announcer._jsonSetting.CategoryAudioMap)
        {
            var field = CreatePitchMinField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcer._jsonSetting,
                1
            );
        } 

        ConfigHeader pitchMaxHeader = new ConfigHeader(panel, "Pitch Max");
        pitchMaxHeader.textColor = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);
        
        foreach (var category in _announcer._jsonSetting.CategoryAudioMap)
        {
            var field = CreatePitchMaxField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcer._jsonSetting,
                1
            );
        } 
    }

    private static BoolField CreateEnabledField(ConfigPanel panel,
                                                string label,
                                                string guid,
                                                AnnouncerMapping jsonSetting,
                                                bool defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Enabled");
        var field = new BoolField(panel, label, fullGuid, jsonSetting.CategoryAudioMap[guid].Enabled);
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
                                             string label,
                                             string guid,
                                             AnnouncerMapping jsonSetting,
                                             float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "VolumeMultiplier");
        var field = new FloatField(panel, label, fullGuid, jsonSetting.CategoryAudioMap[guid].VolumeMultiplier);
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
                                                     string label,
                                                     string guid,
                                                     AnnouncerMapping jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "PitchMin");
        var field = new FloatSliderField(panel, label, fullGuid, Tuple.Create(0.2f, 2f), jsonSetting.CategoryAudioMap[guid].Pitch[0], 2);
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
                                                     string label,
                                                     string guid,
                                                     AnnouncerMapping jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "PitchMax");
        var field = new FloatSliderField(panel, label, fullGuid, Tuple.Create(0.2f, 2f), jsonSetting.CategoryAudioMap[guid].Pitch[1], 2);
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
    
    private static void SomethingAfterUpdateJson()
    {
        LogManager.LogInfo($"Updated json setting for {_title}");
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}