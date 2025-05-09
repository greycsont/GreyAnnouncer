using System.Collections.Generic;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel; // for ButtonField only


namespace greycsont.GreyAnnouncer;


[Description("This object is loaded via reflection from Plugin.cs")]
public class PluginConfiguratorEntry
{
    public static PluginConfigurator config
    {
        get => m_config;
        private set => m_config = value;
    }
    private static PluginConfigurator m_config;

    public static void Initialize()
    {
        CreatePluginPages();

        MainPanelBuilder.Build(m_config);
    }

    private static void CreatePluginPages()
    {
        m_config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        m_config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
    }                          
}
