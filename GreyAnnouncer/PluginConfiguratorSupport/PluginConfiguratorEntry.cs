using System.Collections.Generic;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel; // for ButtonField only


namespace greycsont.GreyAnnouncer;


[Description("This object is loaded via reflection from Plugin.cs")]
public class PluginConfiguratorEntry
{

    private static Dictionary<string, BoolField> m_rankToggleFieldDict = new Dictionary<string, BoolField>();
    public static PluginConfigurator             grey_pluginConfigurator;

    public static void Initialize()
    {
        CreatePluginPages();
        
        MainPanelBuilder.     Build(grey_pluginConfigurator);
        RankTogglePanelBuilder.Build(grey_pluginConfigurator, m_rankToggleFieldDict);
    }

    private static void CreatePluginPages()
    {
        grey_pluginConfigurator = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        grey_pluginConfigurator.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
    }                          
}
