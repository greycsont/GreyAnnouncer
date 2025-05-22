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
    public static void Build(string title, AnnouncerJsonSetting announcerJsonSetting)
    {
        _pluginConfigurator = PluginConfiguratorEntry.config;
        _title              = title;

        ConfigPanel panel   = new ConfigPanel (_pluginConfigurator.rootPanel, _title, _title);
        new ConfigSpace(panel, 15f);

        ConfigHeader header = new ConfigHeader(panel, _title);
        header.textColor    = HeaderColor;

        var actionCollection = new Action<BoolField.BoolValueChangeEvent>[]{
            e => {
                RankAnnouncerV2.UpdateJson(announcerJsonSetting);
                LogManager.LogInfo($"Updated json setting for {_title}");
            }
        };


        foreach (var category in announcerJsonSetting.CategoryAudioMap)
        {
            var field = CreateBoolField(
                panel,
                category.Value.DisplayName,
                category.Key,
                announcerJsonSetting,
                true,
                actionCollection
            );
        }
    }

    private static BoolField CreateBoolField(ConfigPanel panel,
                                             string label,
                                             string guid,
                                             AnnouncerJsonSetting jsonSetting,
                                             bool defaultValue,
                                             Action<BoolField.BoolValueChangeEvent>[] callbacks = null)
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