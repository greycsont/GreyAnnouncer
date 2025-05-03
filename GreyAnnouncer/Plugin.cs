using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

using rankAnnouncerV2;

/* The StyleHUD.cs in the HarmonyPatches folder is the starting point of the whole sequence of announcer 
   But for the initialize of the program like loading audio or something, you should start from here */
namespace greycsont.GreyAnnouncer;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("ULTRAKILL.exe")]
[BepInDependency(PluginDependencies.PLUGINCONFIGURATOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    private         Harmony         m_harmony;  // patch

    private void Awake()
    {
        Log = base.Logger;
        LoadMainModule();
        LoadOptionalModule();
        PatchHarmony();
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void LoadMainModule()
    {
        InstanceConfig.Initialize(this);
    }

    private void LoadOptionalModule()
    {
        CheckPluginLoaded(PluginDependencies.PLUGINCONFIGURATOR_GUID, "greycsont.GreyAnnouncer.PluginConfiguratorEntry");
        RankAnnouncerV2.Initialize();
    }

    private void PatchHarmony()
    {
        m_harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
        m_harmony.PatchAll();
    }

    private void CheckPluginLoaded(string GUID, string assemblyName)
    {
        if (!Chainloader.PluginInfos.ContainsKey(GUID))
        {
            Plugin.Log.LogWarning($"Plugin {GUID} not loaded, stopping loading {assemblyName}"); 
            return;
        }
        ReflectionManager.LoadByReflection(assemblyName, "Initialize");
    }   
}




/* RankAnnouncer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            AudioLoader.cs for loading/caching/storing/fetching audio
            InstanceConfig.cs for setting
            JsonSetting.cs for setting */
