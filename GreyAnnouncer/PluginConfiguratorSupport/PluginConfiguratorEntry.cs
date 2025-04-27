using System.Collections.Generic;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel; // for ButtonField only


namespace greycsont.GreyAnnouncer;


[Description("This object is loaded via reflection from Plugin.cs")]
public class PluginConfiguratorEntry
{

    private static Dictionary<string, BoolField> rankToggleFieldDict = new Dictionary<string, BoolField>();
    private static PluginConfigurator            config;

    public static void Initialize()
    {
        CreatePluginPages();
        
        MainPanelBuilder.      Build(config);
        RankTogglePanelBuilder.Build(config, rankToggleFieldDict);
    }

    private static void CreatePluginPages()
    {
        config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
    }                          
}
