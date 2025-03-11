using PluginConfig.API;
using PluginConfig.API.Fields;

namespace greycsont.GreyAnnouncer{
    public class Interface_PluginConfigurator{
        private static PluginConfigurator config;
        public static void Initialize(Plugin __plugin){
            config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
            FloatField cooldown = new FloatField(config.rootPanel,"Cooldown", "cooldown", 0.75f);
        }
    }
}