using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;


namespace greycsont.GreyAnnouncer;

public static class RegisterAnnouncerPage
{
    private static PluginConfigurator            m_pluginConfigurator;
    private static Dictionary<string, BoolField> m_rankToggleDict;
    private static string                        m_announcerName;
    private static AnnouncerJsonSetting          m_announcerJsonSetting;

    public static void Build(Dictionary<string, BoolField> dict, string announcerName, AnnouncerJsonSetting announcerJsonSetting)
    {
        m_pluginConfigurator   = PluginConfiguratorEntry.greyAnnouncerConfig_PluginConfigurator;
        m_rankToggleDict       = dict;
        m_announcerName        = announcerName;
        m_announcerJsonSetting = announcerJsonSetting;

        ConfigPanel panel    = new ConfigPanel (m_pluginConfigurator.rootPanel, m_announcerName, m_announcerName);
        ConfigHeader header  = new ConfigHeader(panel, m_announcerName);
        header.textColor     = HeaderColor;

        foreach (var entry in InstanceConfig.ConfigEntries)
        {
            if (!entry.Value.section.Equals("Enabled Style")) continue;

            var field = CreateBoolField(
                panel,
                entry.Value.name,
                entry.Key,
                InstanceConfig.RankToggleDict[entry.Key],
                InstanceConfig.DEFAULT_RANK_TOGGLED
            );
            
            m_rankToggleDict.Add(entry.Key, field);
        }
    }

    private static BoolField CreateBoolField(ConfigPanel panel, string label, string guid, ConfigEntry<bool> entry, bool defaultValue, Action<BoolField.BoolValueChangeEvent>[] callbacks = null)
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