using System.ComponentModel;

using UnityEngine;
using PluginConfig.API;

using GreyAnnouncer.Util;


namespace GreyAnnouncer.FrontEnd;


[Description("This object is loaded via reflection from Plugin.cs")]
public static partial class PluginConfiguratorEntry
{
    private static readonly Color m_greyColour = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);

    private static readonly Color m_CyanColour = new UnityEngine.Color(0f, 1f, 1f, 1f);

    private static readonly Color m_OrangeColour = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);

    private static readonly Color m_RedColour = new UnityEngine.Color(1f, 0f, 0f, 1f);

    private static readonly Color m_PurpleColour = new UnityEngine.Color(1f, 0f, 1f, 1f);

    public static PluginConfigurator config
    {
        get => field;
        private set => field = value;
    }

    public static void Initialize()
    {
        CreatePluginPages();
        Build();
    }

    private static void CreatePluginPages()
    {
        config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        config.SetIconWithURL(PathHelper.GetCurrentPluginPath("./../../icon.png"));
    }                          
}
