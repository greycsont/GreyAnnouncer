using System.Collections.Generic;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel; // for ButtonField only


namespace greycsont.GreyAnnouncer;


[Description("This object is loaded via reflection from Plugin.cs")]
public class PluginConfiguratorEntry
{
    public static PluginConfigurator greyAnnouncerConfig_PluginConfigurator
    {
        get => _config;
        private set => _config = value;
    }
    private static PluginConfigurator _config;

    public static void Initialize()
    {
        CreatePluginPages();
        
        MainPanelBuilder.Build(_config);
    }

    private static void CreatePluginPages()
    {
        _config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        _config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
    }                          
}
