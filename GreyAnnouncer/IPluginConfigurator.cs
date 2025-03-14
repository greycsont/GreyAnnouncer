using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;

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
            cooldown.defaultValue = 0.3f;
            RankEnablePanel();  // You don't have to do this in the MainPanel(). I did this because it makes it more readable by human.
            ButtonField button = new ButtonField(config.rootPanel, "Reload Audio", "reload_audio");
            button.onClick += new ButtonField.OnClick(Announcer.ReloadAudio);
        }

        private static void RankEnablePanel(){
            ConfigPanel rankAnnouncerSettingPanel = new ConfigPanel(config.rootPanel, "Rank filter", "Rank_filter");
            BoolField rankD = BoolFieldFactory(rankAnnouncerSettingPanel, "Destruction", "rank_D", InstanceConfig.RankD_Enabled, true);
            BoolField rankC = BoolFieldFactory(rankAnnouncerSettingPanel, "Chaotic", "rank_C", InstanceConfig.RankC_Enabled, true);
            BoolField rankB = BoolFieldFactory(rankAnnouncerSettingPanel, "Brutal", "rank_B", InstanceConfig.RankB_Enabled, true);
            BoolField rankA = BoolFieldFactory(rankAnnouncerSettingPanel, "Anarchic", "rank_A", InstanceConfig.RankA_Enabled, true);
            BoolField rankS = BoolFieldFactory(rankAnnouncerSettingPanel, "Supreme", "rank_S", InstanceConfig.RankS_Enabled, true);
            BoolField rankSS = BoolFieldFactory(rankAnnouncerSettingPanel, "SSadistic", "rank_SS", InstanceConfig.RankSS_Enabled, true);
            BoolField rankSSS = BoolFieldFactory(rankAnnouncerSettingPanel, "SSShitstorm", "rank_SSS", InstanceConfig.RankSSS_Enabled, true);
            BoolField rankU = BoolFieldFactory(rankAnnouncerSettingPanel, "ULTRAKILL", "rank_U", InstanceConfig.RankU_Enabled, true);
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