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

        ConfigHeader pitchHeader = new ConfigHeader(panel, "Pitch Settings");
        pitchHeader.textColor = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);
        
        foreach (var category in _announcer._jsonSetting.CategoryAudioMap)
        {
            var field = CreatePitchField(
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
                                                AnnouncerJsonSetting jsonSetting,
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
                                             AnnouncerJsonSetting jsonSetting,
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

    private static FloatSliderField CreatePitchField(ConfigPanel panel,
                                                     string label,
                                                     string guid,
                                                     AnnouncerJsonSetting jsonSetting,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Pitch");
        var field = new FloatSliderField(panel, label, fullGuid, Tuple.Create(0.2f, 2f), jsonSetting.CategoryAudioMap[guid].Pitch, 2);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Pitch = e.newValue;
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