using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using PluginConfig.API.Functionals;

using GreyAnnouncer.AnnouncerCore;
using GreyAnnouncer.Util;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

public class RegistedAnnouncerPage
{
    private PluginConfigurator _pluginConfigurator => PluginConfiguratorEntry.config;

    private string _title;

    private IAnnouncer _announcer;

    private StringListField _announcerField;

    private ConfigPanel _panel;

    private AnnouncerConfigFields _fields = new AnnouncerConfigFields
    {
        CategoryFields = new Dictionary<string, CategoryFields>()
    };

    private ConfigHeader _mismatchHeader;

    public RegistedAnnouncerPage(IAnnouncer announcer)
    {
        Build(announcer);
    }

    public void Build(IAnnouncer announcer)
    {
        _title = announcer.title;
        _announcer = announcer;

        _panel = new ConfigPanel(_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(_panel, 15f);

        ConfigHeader titleHeader = new ConfigHeader(_panel, _title, 30);
        titleHeader.textColor = HeaderColor;

        var announcers = AudioAnnouncer.GetAvailablePacks(_announcer.title);

        _announcerField = new StringListField(
            _panel,                     
            "Selected Announcer",             
            _announcer.title + "_" + "Selected_Announcer",     
            announcers,                
            announcers.FirstOrDefault() ?? "default",
            saveToConfig: false
        );

        _announcerField.value = Path.GetFileName(_announcer.announcerPath);

        _announcerField.onValueChange += e =>
        {
            _announcer.announcerPath = Path.Combine(Setting.announcersPath, _announcer.title, e.value);
        };

        var openDirectoryButtonField = new ButtonField(
            _panel,
            "Open Current Announcer Folder",
            _announcer.title + "_" + "Open_Current_Announcer_Folder"
        );
        openDirectoryButtonField.onClick += ()
            => _announcer.EditExternally();

        _mismatchHeader = new ConfigHeader(_panel, ""){
            textColor = new Color(1f, 0.4f, 0.4f),
            textSize = 14
        };

        if (_announcer.isConfigLoaded)
        {
            BuildConfigSection();
        }
        _announcer.syncUI += ApplyConfigToUI;
    }

    private void BuildConfigSection()
    {
        new ConfigSpace(_panel, 15f);
        new ConfigHeader(_panel, "Configuration");

        _fields.RandomizeAudioField = new BoolField(
            _panel,
            "Randomize Audio On Play",
            _announcer.title + "_" + "RandomizeAudioOnPlay",
            false,
            saveToConfig: false
        );
        _fields.RandomizeAudioField.defaultValue = false;
        _fields.RandomizeAudioField.value = _announcer.announcerConfig.RandomizeAudioOnPlay;
        _fields.RandomizeAudioField.onValueChange += e =>
        {
            _announcer.announcerConfig.RandomizeAudioOnPlay = e.value;
        };

        foreach (var category in _announcer.announcerConfig.CategorySetting)
        {
            string key = category.Key;

            ConfigHeader header = new ConfigHeader(_panel, category.Key);
            header.textColor = new Color(0, 1, 1);

            var fields = new CategoryFields
            {
                Enabled           = CreateEnabledField(_panel, key, true),
                Volume            = CreateVolumeField(_panel, key, 1f),
                Cooldown          = CreateCooldownField(_panel, key, 3.0f),
                ExcludeFromRandom = CreateExcludeFromRandomField(_panel, key, false)
            };

            _fields.CategoryFields[key] = fields;
        }
    }

    private BoolField CreateEnabledField(ConfigPanel panel, string guid, bool defaultValue)
    {
        var fullGuid = _announcer.title + "_" + GuidPrefixAdder.AddPrefixToGUID(guid, "Enabled");
        var field = new BoolField(panel, "Enabled", fullGuid, defaultValue, saveToConfig: false);
        field.defaultValue = defaultValue;
        field.value = _announcer.announcerConfig.CategorySetting[guid].Enabled;
        field.onValueChange += e =>
            _announcer.announcerConfig.CategorySetting[guid].Enabled = e.value;

        return field;
    }

    private FloatField CreateVolumeField(ConfigPanel panel, string guid, float defaultValue)
    {
        var fullGuid = _announcer.title + "_" + GuidPrefixAdder.AddPrefixToGUID(guid, "VolumeMultiplier");
        var field = new FloatField(panel, "Volume", fullGuid, defaultValue, saveToConfig: false);
        field.defaultValue = defaultValue;
        field.value = _announcer.announcerConfig.CategorySetting[guid].VolumeMultiplier;
        field.onValueChange += e =>
            _announcer.announcerConfig.CategorySetting[guid].VolumeMultiplier = e.value;

        return field;
    }

    private FloatSliderField CreateCooldownField(ConfigPanel panel, string guid, float defaultValue)
    {
        var fullGuid = _announcer.title + "_" + GuidPrefixAdder.AddPrefixToGUID(guid, "Cooldown");
        var field = new FloatSliderField(panel, "Cooldown", fullGuid, Tuple.Create(0.2f, 6f), defaultValue, 1, saveToConfig: false);
        field.defaultValue = defaultValue;
        field.value = _announcer.announcerConfig.CategorySetting[guid].Cooldown;
        field.onValueChange += e =>
            _announcer.announcerConfig.CategorySetting[guid].Cooldown = e.newValue;

        return field;
    }

    private BoolField CreateExcludeFromRandomField(ConfigPanel panel, string guid, bool defaultValue)
    {
        var fullGuid = _announcer.title + "_" + GuidPrefixAdder.AddPrefixToGUID(guid, "ExcludeFromRandom");
        var field = new BoolField(panel, "Exclude From Random Selection", fullGuid, defaultValue, saveToConfig: false);
        field.defaultValue = defaultValue;
        field.value = _announcer.announcerConfig.CategorySetting[guid].ExcludeFromRandom;
        field.onValueChange += e =>
            _announcer.announcerConfig.CategorySetting[guid].ExcludeFromRandom = e.value;

        field.hidden = !_announcer.announcerConfig.RandomizeAudioOnPlay;

        return field;
    }

    public void ApplyConfigToUI()
    {
        LogHelper.LogDebug($"ApplyConfigToUI called");

        var currentPack = Path.GetFileName(_announcer.announcerPath);
        _announcerField.value = currentPack;

        if (!_announcer.isConfigLoaded) {
            _mismatchHeader.text = string.IsNullOrEmpty(_announcer.configMismatchInfo)
                ? "Config not loaded."
                : $"Config mismatch — {_announcer.configMismatchInfo}";
            return;
        }
        _mismatchHeader.text = "";

        var config = _announcer.announcerConfig;

        if (_fields.RandomizeAudioField == null)
            BuildConfigSection();

        _fields.RandomizeAudioField.value = config.RandomizeAudioOnPlay;

        foreach (KeyValuePair<string, CategoryFields> kv in _fields.CategoryFields) {
            var category = kv.Key;
            var fields   = kv.Value;

            if (!config.CategorySetting.TryGetValue(category, out var data))
                continue;

            fields.Enabled.value  = data.Enabled;
            fields.Volume.value   = data.VolumeMultiplier;
            fields.Cooldown.value = data.Cooldown;
            fields.ExcludeFromRandom.value = data.ExcludeFromRandom;
            fields.ExcludeFromRandom.hidden = !config.RandomizeAudioOnPlay;
        }
    }
    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}