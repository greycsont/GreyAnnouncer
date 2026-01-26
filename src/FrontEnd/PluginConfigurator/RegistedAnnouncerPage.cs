using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using PluginConfig.API.Functionals;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.Util;


namespace GreyAnnouncer.FrontEnd;

public class RegistedAnnouncerPage
{
    private PluginConfigurator _pluginConfigurator;

    private string _title;

    private AudioAnnouncer _audioAnnouncer;
    
    private AnnouncerConfigFields _fields = new AnnouncerConfigFields
    {
        CategoryFields = new Dictionary<string, CategoryFields>()
    };
        
    public void Build(string title, AudioAnnouncer audioAnnouncer)
    {
        _pluginConfigurator = PluginConfiguratorEntry.config;
        _title = title;
        _audioAnnouncer = audioAnnouncer;

        ConfigPanel panel = new ConfigPanel(_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(panel, 15f);

        ConfigHeader titleHeader = new ConfigHeader(panel, _title, 30);
        titleHeader.textColor = HeaderColor;

        var announcers = Directory
                             .GetDirectories(AnnouncerIndex.announcersPath)
                             .Select(Path.GetFileName)
                             .ToList();

        var announcerField = new StringListField(
            panel,                     
            "Selected Announcer",             
            "Selected_Announcer",     
            announcers,                
            announcers.FirstOrDefault() ?? "default"
        );

        var openDirectoryButtonField = new ButtonField(
            panel,
            "Open Current Announcer Folder",
            "Open_Current_Announcer_Folder"
        );
        openDirectoryButtonField.onClick += () 
            => _audioAnnouncer.EditExternally();

        // WARNING: This will broken after I create a way to change AnnouncerIndex.announcersPath
        announcerField.onValueChange += e =>
        {
            audioAnnouncer.announcerPath = Path.Combine(AnnouncerIndex.announcersPath, e.value);
        };
        
        new ConfigSpace(panel, 15f);

        ConfigHeader configHeader = new ConfigHeader(panel, "Configuration");

        _fields.RandomizeAudioField = new BoolField(
            panel,
            "Randomize Audio On Play",
            GuidPrefixAdder.AddPrefixToGUID("RandomizeAudioOnPlay", _title),
            _audioAnnouncer.announcerConfig.RandomizeAudioOnPlay
        );
        _fields.RandomizeAudioField.defaultValue = false;
        _fields.RandomizeAudioField.onValueChange += e =>
        {
            _audioAnnouncer.announcerConfig.RandomizeAudioOnPlay = e.value;
        };
        
        foreach (var category in _audioAnnouncer.announcerConfig.CategorySetting)
        {
            string key = category.Key;

            ConfigHeader header = new ConfigHeader(panel, category.Key);
            header.textColor = new Color(0,1,1);

            var fields = new CategoryFields
            {
                Enabled = CreateEnabledField(panel, key, _audioAnnouncer.announcerConfig, true),
                Volume  = CreateVolumeField(panel, key, _audioAnnouncer.announcerConfig, 1f),
                Cooldown = CreateCooldownField(panel, key, _audioAnnouncer.announcerConfig, 3.0f)
            };

            _fields.CategoryFields[key] = fields;
        }
    }

    private BoolField CreateEnabledField(ConfigPanel panel,
                                                string guid,
                                                AnnouncerConfig AnnouncerConfig,
                                                bool defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Enabled");
        var field = new BoolField(panel, "Enabled", fullGuid, AnnouncerConfig.CategorySetting[guid].Enabled);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (AnnouncerConfig.CategorySetting.ContainsKey(guid))
                AnnouncerConfig.CategorySetting[guid].Enabled = e.value;

        };

        return field;
    }


    private FloatField CreateVolumeField(ConfigPanel panel,
                                             string guid,
                                             AnnouncerConfig AnnouncerConfig,
                                             float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "VolumeMultiplier");
        var field = new FloatField(panel, "Volume", fullGuid, AnnouncerConfig.CategorySetting[guid].VolumeMultiplier);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (AnnouncerConfig.CategorySetting.ContainsKey(guid))
                AnnouncerConfig.CategorySetting[guid].VolumeMultiplier = e.value;
        };

        return field;
    }

    private FloatSliderField CreateCooldownField(ConfigPanel panel,
                                                     string guid,
                                                     AnnouncerConfig AnnouncerConfig,
                                                     float defaultValue)
    {
        var fullGuid = GuidPrefixAdder.AddPrefixToGUID(guid, "Cooldown");
        var field = new FloatSliderField(panel, "Cooldown", fullGuid, Tuple.Create(0.2f, 6f), AnnouncerConfig.CategorySetting[guid].Cooldown, 1);
        field.defaultValue = defaultValue;
        field.onValueChange += e =>
        {
            if (AnnouncerConfig.CategorySetting.ContainsKey(guid))
                AnnouncerConfig.CategorySetting[guid].Cooldown = e.newValue;
        };

        return field;
    }

    public void ApplyConfigToUI(AnnouncerConfig config)
    {
        LogHelper.LogDebug($"ApplyConfigToUI called");

        if (_fields.RandomizeAudioField != null)
            _fields.RandomizeAudioField.value = config.RandomizeAudioOnPlay;

        foreach (KeyValuePair<string, CategoryFields> kv in _fields.CategoryFields)
        {
            var category = kv.Key;
            var fields   = kv.Value;

            if (!config.CategorySetting.TryGetValue(category, out var data))
                continue;

            if (fields.Enabled.value != data.Enabled)
                LogHelper.LogDebug($"Category '{category}': Enabled changed from {fields.Enabled.value} -> {data.Enabled}");
            else
                LogHelper.LogDebug($"Category '{category}': Enabled unchanged ({fields.Enabled.value})");

            if (Math.Abs(fields.Volume.value - data.VolumeMultiplier) > 0.0001f)
                LogHelper.LogDebug($"Category '{category}': Volume changed from {fields.Volume.value} -> {data.VolumeMultiplier}");
            else
                LogHelper.LogDebug($"Category '{category}': Volume unchanged ({fields.Volume.value})");

            if (Math.Abs(fields.Cooldown.value - data.Cooldown) > 0.0001f)
                LogHelper.LogDebug($"Category '{category}': Cooldown changed from {fields.Cooldown.value} -> {data.Cooldown}");
            else
                LogHelper.LogDebug($"Category '{category}': Cooldown unchanged ({fields.Cooldown.value})");

            fields.Enabled.value  = data.Enabled;
            fields.Volume.value   = data.VolumeMultiplier;
            fields.Cooldown.value = data.Cooldown;
        }
    }
    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}

public class CategoryFields
{
    public BoolField Enabled;
    public FloatField Volume;
    public FloatSliderField Cooldown;
}

public class AnnouncerConfigFields
{
    public BoolField RandomizeAudioField;
    public Dictionary<string, CategoryFields> CategoryFields;

}