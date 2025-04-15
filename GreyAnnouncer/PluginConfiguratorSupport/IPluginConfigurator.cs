using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;  // for ConfigHeader
using PluginConfig.API.Functionals; // for ButtonField only

namespace greycsont.GreyAnnouncer{
    public class IPluginConfigurator{
        
        private static Dictionary<string, BoolField> rankToggleFieldDict = new Dictionary<string, BoolField>();
        private static PluginConfigurator config;
        
        public static void Initialize(){
            config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
            MainPanel();
        }

        private static void MainPanel(){
            ConfigHeader mainHeader = new ConfigHeader(config.rootPanel, "Main Settings");
            
            mainHeader.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);
            
            FloatField sharedRankPlayCooldown = new FloatField(config.rootPanel,
                    "Shared rank cooldown", 
                    "sharedRankPlayCooldown", 
                    InstanceConfig.SharedRankPlayCooldown.Value, 0f, 114514f
                    );  // ConfigFields(panel, displayname, GUID, default value))
            sharedRankPlayCooldown.defaultValue = 0f;
            sharedRankPlayCooldown.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                InstanceConfig.SharedRankPlayCooldown.Value = e.value;
                Announcer.ResetTimerToZero();
            };

            FloatField individualRankPlayCooldown = new FloatField(
                    config.rootPanel,
                    "Individual rank cooldown", 
                    "individualRankPlaycooldown", 
                    InstanceConfig.IndividualRankPlayCooldown.Value, 0f, 1113f
                    );  
            individualRankPlayCooldown.defaultValue = 3f;
            individualRankPlayCooldown.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                InstanceConfig.IndividualRankPlayCooldown.Value = e.value;
                Announcer.ResetTimerToZero();
            };

            FloatSliderField audioSourceVolume = new FloatSliderField(
                    config.rootPanel,
                    "Audio Volume", 
                    "Audio_Volume", 
                    Tuple.Create(0f, 1f), InstanceConfig.AudioSourceVolume.Value, 2
                    );
            audioSourceVolume.defaultValue = 1f;
            audioSourceVolume.onValueChange += (FloatSliderField.FloatSliderValueChangeEvent e) =>
            {
                InstanceConfig.AudioSourceVolume.Value = e.newValue;
                Announcer.UpdateAudioSourceVolume(e.newValue);
            };      

            /* You don't have to do this in the MainPanel(). I did this because it makes it more readable by human. */
            RankTogglePanel();  
            AdvancedOptionPanel();
            ButtonField button = new ButtonField(config.rootPanel, "Reload Audio", "reload_audio");
            button.onClick += new ButtonField.OnClick(Announcer.ReloadAudio);
        }

        private static void RankTogglePanel(){
            ConfigPanel rankActivationPanel = new ConfigPanel(config.rootPanel, "Rank Activation", "Rank_Activation");
            ConfigHeader rankActivationHeader = new ConfigHeader(rankActivationPanel, "Rank Activation");
            rankActivationHeader.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);

            foreach (var entry in InstanceConfig.ConfigEntries)
            {
                if (!entry.Value.section.Equals("Enabled Style")) continue;

                var boolField = BoolFieldFactory(rankActivationPanel, entry.Value.name, entry.Key, InstanceConfig.RankToggleDict[entry.Key]);
                rankToggleFieldDict.Add(entry.Key, boolField);
            }
        }

        private static void AdvancedOptionPanel(){
            ConfigPanel AdvancedOptionPanel = new ConfigPanel(config.rootPanel, "Advanced Option", "Advanced_Option");
            ConfigHeader LowPassFilterHeader = new ConfigHeader(AdvancedOptionPanel, "Audio Frequency Filter"); 
            LowPassFilterHeader.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);
            BoolField LowPassFilter_Enabled = BoolFieldFactory(
                    AdvancedOptionPanel, 
                    "Filtering when under water",
                    "LowPassFilter_Enabled",
                    InstanceConfig.LowPassFilter_Enabled, 
                    true, 
                    (BoolField.BoolValueChangeEvent e) =>
                    {
                        Water.CheckIsInWater(e.value);
                    });    //In Water.cs
        }

        private static BoolField BoolFieldFactory(ConfigPanel parentPanel,string name,string GUID,ConfigEntry<bool> configEntry,bool defaultValue = true,params Action<BoolField.BoolValueChangeEvent>[] eventCallbacks)
        {
            BoolField boolField = new BoolField(parentPanel, name, GuidPrefixAdder.AddPrefixToGUID(GUID), configEntry.Value);
            boolField.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                configEntry.Value = e.value;
                 if (eventCallbacks != null)
                {
                    foreach (var callback in eventCallbacks)
                    {
                        callback?.Invoke(e);
                    }
                }
            };
            boolField.defaultValue = defaultValue;
            return boolField;
        }
    }

    public static class GuidPrefixAdder //this motherfucker should be added to all items, but idk how to decoupe the code
    {
        public static string AddPrefixToGUID(string rankKey)
        {
            // 可加入前缀保持统一命名风格
            return $"GreyAnnouncer_{rankKey.ToUpper()}";
        }

        public static string RemovePrefixFromGUID(string guid)
        {
            // 可选：反向转换，防止 guid 显示太“技术”
            if (guid.StartsWith("InstanceConfig_Rank_"))
            {
                return guid.Substring("InstanceConfig_Rank_".Length);
            }
            return guid;
        }
    }
}