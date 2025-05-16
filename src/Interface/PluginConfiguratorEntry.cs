using PluginConfig.API;
using System.ComponentModel;


namespace GreyAnnouncer.PluginConfiguratorGUI;


[Description("This object is loaded via reflection from Plugin.cs")]
public static class PluginConfiguratorEntry
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
