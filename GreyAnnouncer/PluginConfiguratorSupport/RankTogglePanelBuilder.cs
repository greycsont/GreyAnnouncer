using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;


namespace greycsont.GreyAnnouncer;

public static class RankTogglePanelBuilder
{
    private static PluginConfigurator            _config;
    private static Dictionary<string, BoolField> _rankToggleDict;

    public static void Build(PluginConfigurator config, Dictionary<string, BoolField> dict)
    {
        _config             = config;
        _rankToggleDict     = dict;

        ConfigPanel panel   = new ConfigPanel (_config.rootPanel, "Rank Activation", "Rank_Activation");
        ConfigHeader header = new ConfigHeader(panel,             "Rank Activation"                   );
        header.textColor    = HeaderColor;

        foreach (var entry in InstanceConfig.ConfigEntries)
        {
            if (!entry.Value.section.Equals("Enabled Style")) continue;

            var field = CreateBoolField(
                panel, 
                entry.Value.name, 
                entry.Key, 
                InstanceConfig.RankToggleDict[entry.Key]
            );
            
            _rankToggleDict.Add(entry.Key, field);
        }
    }

    private static BoolField CreateBoolField(ConfigPanel panel, string label, string guid, ConfigEntry<bool> entry, bool defaultValue = true, Action<BoolField.BoolValueChangeEvent>[] callbacks = null)
    {
        var fullGuid         = GuidPrefixAdder.AddPrefixToGUID(guid);
        var field            = new BoolField(panel, label, fullGuid, entry.Value);
        field.defaultValue   = defaultValue;
        field.onValueChange += e =>
        {
            entry.Value = e.value;
            if (callbacks != null)
            {
                foreach (var callback in callbacks)
                    callback?.Invoke(e);
            }
        };

        return field;
    }

    private static readonly Color HeaderColor = new Color(0.85f, 0.85f, 0.85f, 1f);
}

