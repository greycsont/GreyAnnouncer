using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using System;
using UnityEngine;


namespace greycsont.GreyAnnouncer;

public static class RegisterAnnouncerPage
{
    private static PluginConfigurator            m_pluginConfigurator;
    private static string                        m_announcerName;
    private static AudioAnnouncer                m_audioAnnouncer;
    public static void Build(string announcerName, AnnouncerJsonSetting announcerJsonSetting, AudioAnnouncer audioAnnouncer)
    {
        m_pluginConfigurator = PluginConfiguratorEntry.config;
        m_announcerName      = announcerName;
        m_audioAnnouncer     = audioAnnouncer;


        ConfigPanel panel   = new ConfigPanel (m_pluginConfigurator.rootPanel, m_announcerName, m_announcerName);
        ConfigHeader header = new ConfigHeader(panel, m_announcerName);
        header.textColor    = HeaderColor;

        foreach (var category in announcerJsonSetting.CategoryAudioMap)
        {
            var field = CreateBoolField(
                panel,
                category.Key,
                category.Key,
                announcerJsonSetting,
                true,
                new Action<BoolField.BoolValueChangeEvent>[] {
                    e => {
                        m_audioAnnouncer.UpdateJson(announcerJsonSetting);
                        Plugin.log.LogInfo($"Updated json setting for {m_announcerName}");
                    }
                }
            );
        }
    }

    private static BoolField CreateBoolField(ConfigPanel panel, string label, string guid, AnnouncerJsonSetting jsonSetting, bool defaultValue, Action<BoolField.BoolValueChangeEvent>[] callbacks = null)
    {
        var fullGuid         = GuidPrefixAdder.AddPrefixToGUID(guid);
        var field            = new BoolField(panel, label, fullGuid, jsonSetting.CategoryAudioMap[guid].Enabled);
        field.defaultValue   = defaultValue;
        field.onValueChange += e =>
        {
            if (jsonSetting.CategoryAudioMap.ContainsKey(guid))
            {
                jsonSetting.CategoryAudioMap[guid].Enabled = e.value;
            }
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