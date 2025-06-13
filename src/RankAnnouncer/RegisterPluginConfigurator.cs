using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using UnityEngine;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.PluginConfiguratorGUI;

namespace GreyAnnouncer.RankAnnouncer;

public static class RegisterRankAnnouncerPage
{
    private static PluginConfigurator            _pluginConfigurator;
    private static string                        _title;
    private static AnnouncerJsonSetting          _announcerJsonSetting;
    public static void Build(string title, AnnouncerJsonSetting announcerJsonSetting)
    {
        _pluginConfigurator = PluginConfiguratorEntry.config;
        _title = title;
        _announcerJsonSetting = announcerJsonSetting;

        ConfigPanel panel = new ConfigPanel(_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(panel, 15f);

        ConfigHeader titleHeader = new ConfigHeader(panel, _title);
        titleHeader.textColor = HeaderColor;


        foreach (var category in _announcerJsonSetting.CategoryAudioMap)
        {
            var field = CreateEnabledField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcerJsonSetting,
                true
            );
        }

        ConfigHeader volumeHeader = new ConfigHeader(panel, "Volume Settings");
        volumeHeader.textColor = new UnityEngine.Color(0f, 1f, 1f, 1f);

        foreach (var category in _announcerJsonSetting.CategoryAudioMap)
        {
            var field = CreateVolumeField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcerJsonSetting,
                1
            );
        }

        ConfigHeader pitchHeader = new ConfigHeader(panel, "Pitch Settings");
        pitchHeader.textColor = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);
        
        foreach (var category in _announcerJsonSetting.CategoryAudioMap)
        {
            var field = CreatePitchField(
                panel,
                category.Value.DisplayName,
                category.Key,
                _announcerJsonSetting,
                1
            );
        } 
    }

    public static void UpdateJsonSetting(AnnouncerJsonSetting announcerJsonSetting)
    {
        _announcerJsonSetting = announcerJsonSetting;
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

            RankAnnouncerV2.UpdateJson(_announcerJsonSetting);
            LogManager.LogInfo($"Updated json setting for {_title}");
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

            RankAnnouncerV2.UpdateJson(_announcerJsonSetting);
            LogManager.LogInfo($"Updated json setting for {_title}");
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

            RankAnnouncerV2.UpdateJson(_announcerJsonSetting);
            LogManager.LogInfo($"Updated json setting for {_title}");
        };

        return field;
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}