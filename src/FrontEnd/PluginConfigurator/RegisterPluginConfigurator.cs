using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using UnityEngine;

using GreyAnnouncer.AnnouncerAPI;
using System.Collections.Generic;

namespace GreyAnnouncer.FrontEnd;

public static class RegisterRankAnnouncerPagev2
{
    private static PluginConfigurator _pluginConfigurator;

    private static string _title;

    private static AudioAnnouncer _announcer;

    private static readonly Dictionary<string, CategoryFields> _categoryFields
        = new Dictionary<string, CategoryFields>();
        
    public static void Build(string title, AudioAnnouncer announcer)
    {
        _pluginConfigurator = PluginConfiguratorEntry.config;
        _title = title;
        _announcer = announcer;

        ConfigPanel panel = new ConfigPanel(_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(panel, 15f);

        ConfigHeader titleHeader = new ConfigHeader(panel, _title);
        titleHeader.textColor = HeaderColor;

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
            string key = category.Key;

            ConfigHeader header = new ConfigHeader(panel, category.Key);
            header.textColor = new Color(0,1,1);

            var fields = new CategoryFields
            {
                Enabled = CreateEnabledField(panel, key, _announcer._announcerConfig, true),
                Volume  = CreateVolumeField(panel, key, _announcer._announcerConfig, 1f),
                Cooldown = CreateCooldownField(panel, key, _announcer._announcerConfig, 3.0f)
            };

            _categoryFields[key] = fields;
        }
    }

    public static void ApplyConfigToUI(AnnouncerConfig config)
    {
        foreach (KeyValuePair<string, CategoryFields> kv in _categoryFields)
        {
            var category = kv.Key;
            var fields   = kv.Value;

            if (!config.CategoryAudioMap.TryGetValue(category, out var data))
                continue;

            fields.Enabled.value  = data.Enabled;
            fields.Volume.value   = data.VolumeMultiplier;
            fields.Cooldown.value = data.Cooldown;
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
        _announcer.UpdateAnnouncerConfig(_announcer._announcerConfig);
        LogManager.LogInfo($"Updated announcer config for {_title}");
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}

public class CategoryFields
{
    public BoolField Enabled;
    public FloatField Volume;
    public FloatSliderField Cooldown;
}