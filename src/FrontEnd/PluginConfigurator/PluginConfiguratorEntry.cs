using PluginConfig.API;
using System.ComponentModel;

using GreyAnnouncer.Util;


namespace GreyAnnouncer.FrontEnd;


[Description("This object is loaded via reflection from Plugin.cs")]
public static class PluginConfiguratorEntry
{
    public static PluginConfigurator config
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
