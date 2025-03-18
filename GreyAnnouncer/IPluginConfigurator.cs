using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Fields;
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
            // ConfigFields(panel, displayname, GUID, default value))
            FloatField sharedRankPlayCooldown = new FloatField(
                    config.rootPanel,
                    "Shared rank cooldown", 
                    "sharedRankPlayCooldown", 
                    InstanceConfig.SharedRankPlayCooldown.Value
                    );
            sharedRankPlayCooldown.minimumValue = 0f;
            sharedRankPlayCooldown.maximumValue = 114514f;
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
                    InstanceConfig.IndividualRankPlayCooldown.Value
                    );
            individualRankPlayCooldown.minimumValue = 0f;
            individualRankPlayCooldown.maximumValue = 10f;
            individualRankPlayCooldown.defaultValue = 3f;
            individualRankPlayCooldown.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                InstanceConfig.IndividualRankPlayCooldown.Value = e.value;
                Announcer.ResetTimerToZero();
            };
            
            RankEnablePanel();  // You don't have to do this in the MainPanel(). I did this because it makes it more readable by human.

            ButtonField button = new ButtonField(config.rootPanel, "Reload Audio", "reload_audio");
            button.onClick += new ButtonField.OnClick(Announcer.ReloadAudio);
        }

        private static void RankEnablePanel(){
            ConfigPanel rankActivationPanel = new ConfigPanel(config.rootPanel, "Rank Activation", "Rank_Activation");
            BoolField rankD = BoolFieldFactory(rankActivationPanel, "Destruction", "rank_D", InstanceConfig.RankD_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankC = BoolFieldFactory(rankActivationPanel, "Chaotic", "rank_C", InstanceConfig.RankC_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankB = BoolFieldFactory(rankActivationPanel, "Brutal", "rank_B", InstanceConfig.RankB_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankA = BoolFieldFactory(rankActivationPanel, "Anarchic", "rank_A", InstanceConfig.RankA_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankS = BoolFieldFactory(rankActivationPanel, "Supreme", "rank_S", InstanceConfig.RankS_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankSS = BoolFieldFactory(rankActivationPanel, "SSadistic", "rank_SS", InstanceConfig.RankSS_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankSSS = BoolFieldFactory(rankActivationPanel, "SSShitstorm", "rank_SSS", InstanceConfig.RankSSS_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
            BoolField rankU = BoolFieldFactory(rankActivationPanel, "ULTRAKILL", "rank_U", InstanceConfig.RankU_Enabled, InstanceConfig.DEFAULT_RANK_FILTER_ENABLED);
        }

        private static BoolField BoolFieldFactory(ConfigPanel parentPanel,string name,string GUID,ConfigEntry<bool> configEntry,bool defaultValue){
            BoolField boolField = new BoolField(parentPanel, name, GUID, configEntry.Value);
            boolField.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                configEntry.Value = e.value;
            };
            boolField.defaultValue = defaultValue;
            return boolField;
        }
    }
}