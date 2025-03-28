using System;
using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;  // for ConfigHeader
using PluginConfig.API.Functionals; // for buttonField only

namespace greycsont.GreyAnnouncer{
    public class IPluginConfigurator{
        private static PluginConfigurator config;
        public static void Initialize(){
            config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
            MainPanel();
        }

        private static void MainPanel(){
            ConfigHeader mainHeader = new ConfigHeader(config.rootPanel, "Main Settings");
            mainHeader.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);
            // ConfigFields(panel, displayname, GUID, default value))
            FloatField sharedRankPlayCooldown = new FloatField(
                    config.rootPanel,
                    "Shared rank cooldown", 
                    "sharedRankPlayCooldown", 
                    InstanceConfig.SharedRankPlayCooldown.Value, 0f, 114514f
                    );
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
            RankEnablePanel();  
            AdvancedOptionPanel();
            ButtonField button = new ButtonField(config.rootPanel, "Reload Audio", "reload_audio");
            button.onClick += new ButtonField.OnClick(Announcer.ReloadAudio);
        }

        private static void RankEnablePanel(){
            ConfigPanel rankActivationPanel = new ConfigPanel(config.rootPanel, "Rank Activation", "Rank_Activation");
            ConfigHeader rankActivationHeader = new ConfigHeader(rankActivationPanel, "Rank Activation");
            rankActivationHeader.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);
            BoolField rankD_Enabled = BoolFieldFactory(rankActivationPanel, "Destruction", "rank_D", InstanceConfig.RankD_Enabled);
            BoolField rankC_Enabled = BoolFieldFactory(rankActivationPanel, "Chaotic", "rank_C", InstanceConfig.RankC_Enabled);
            BoolField rankB_Enabled = BoolFieldFactory(rankActivationPanel, "Brutal", "rank_B", InstanceConfig.RankB_Enabled);
            BoolField rankA_Enabled = BoolFieldFactory(rankActivationPanel, "Anarchic", "rank_A", InstanceConfig.RankA_Enabled);
            BoolField rankS_Enabled = BoolFieldFactory(rankActivationPanel, "Supreme", "rank_S", InstanceConfig.RankS_Enabled);
            BoolField rankSS_Enabled = BoolFieldFactory(rankActivationPanel, "SSadistic", "rank_SS", InstanceConfig.RankSS_Enabled);
            BoolField rankSSS_Enabled = BoolFieldFactory(rankActivationPanel, "SSShitstorm", "rank_SSS", InstanceConfig.RankSSS_Enabled);
            BoolField rankU_Enabled = BoolFieldFactory(rankActivationPanel, "ULTRAKILL", "rank_U", InstanceConfig.RankU_Enabled);
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

        private static BoolField BoolFieldFactory(ConfigPanel parentPanel,string name,string GUID,ConfigEntry<bool> configEntry,bool defaultValue = true,params Action<BoolField.BoolValueChangeEvent>[] eventCallbacks)  // 额外事件绑定
        {
            BoolField boolField = new BoolField(parentPanel, name, GUID, configEntry.Value);
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
}