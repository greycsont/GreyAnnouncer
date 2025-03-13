using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Fields;

namespace greycsont.GreyAnnouncer{
    public class IPluginConfigurator{
        private static PluginConfigurator config;
        public static void Initialize(){
            config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
            MainPanel();
        }

        private static void MainPanel(){
            FloatField cooldown = new FloatField(config.rootPanel,"Cooldown timer(in sec)", "cooldown", InstanceConfig.AnnounceCooldown.Value);
            // ConfigFields(panel, displayname, GUID, default value))
            cooldown.minimumValue = 0;
            cooldown.maximumValue = 114514;
            cooldown.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                InstanceConfig.AnnounceCooldown.Value = e.value;
                Announcer.ResetTimerToZero();
            };
            RankEnablePanel();  // You don't have to do this in the MainPanel(). I did this because it makes it more readable by human.
        }

        private static void RankEnablePanel(){
            ConfigPanel rankAnnouncerSettingPanel = new ConfigPanel(config.rootPanel, "Rank filter", "Rank_filter");
            BoolField rankD = BoolFieldFactory(rankAnnouncerSettingPanel, "Destruction", "rank_D", InstanceConfig.RankD_Enabled);
            BoolField rankC = BoolFieldFactory(rankAnnouncerSettingPanel, "Chaotic", "rank_C", InstanceConfig.RankC_Enabled);
            BoolField rankB = BoolFieldFactory(rankAnnouncerSettingPanel, "Brutal", "rank_B", InstanceConfig.RankB_Enabled);
            BoolField rankA = BoolFieldFactory(rankAnnouncerSettingPanel, "Anarchic", "rank_A", InstanceConfig.RankA_Enabled);
            BoolField rankS = BoolFieldFactory(rankAnnouncerSettingPanel, "Supreme", "rank_S", InstanceConfig.RankS_Enabled);
            BoolField rankSS = BoolFieldFactory(rankAnnouncerSettingPanel, "SSadistic", "rank_SS", InstanceConfig.RankSS_Enabled);
            BoolField rankSSS = BoolFieldFactory(rankAnnouncerSettingPanel, "SSShitstorm", "rank_SSS", InstanceConfig.RankSSS_Enabled);
            BoolField rankU = BoolFieldFactory(rankAnnouncerSettingPanel, "ULTRAKILL", "rank_U", InstanceConfig.RankU_Enabled);
        }

        private static BoolField BoolFieldFactory(ConfigPanel parentPanel,string name,string GUID,ConfigEntry<bool> configEntry){
            BoolField boolField = new BoolField(parentPanel, name, GUID, configEntry.Value);
            boolField.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                configEntry.Value = e.value;
            };
            return boolField;
        }
    }
}