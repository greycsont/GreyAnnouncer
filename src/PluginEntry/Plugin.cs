using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using GreyAnnouncer.Util;
using GreyAnnouncer.Config;
using GreyAnnouncer.FrontEnd;

/* The StyleHUD.cs in the HarmonyPatches folder is the starting point of the whole sequence of announcer 
   But for the initialize of the program like loading audio or something, you should start from here */
namespace GreyAnnouncer;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("ULTRAKILL.exe")]
[BepInDependency(PluginDependencies.PLUGINCONFIGURATOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        LogHelper.log = base.Logger;
        LogHelper.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        LoadMainModule();
        LoadOptionalModule();
        PatchHarmony();
        gameObject.AddComponent<UIFactory>();
    }

    private void LoadMainModule()
    {
        BepInExConfig.Initialize(this);
    }

    private void LoadOptionalModule()
    {
        CheckPluginLoaded();
        RankAnnouncer.RankAnnouncer.Initialize();
        FinalRankAnnouncer.FinalRankAnnouncer.Initialize();
    }

    private void PatchHarmony()
    {
        new Harmony(PluginInfo.PLUGIN_GUID + ".harmony").PatchAll();
    }

    private void CheckPluginLoaded()
    {
        PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "PluginConfiguratorEntry",
            () => ReflectionManager.LoadByReflection("GreyAnnouncer.FrontEnd.PluginConfiguratorEntry", "Initialize")
        );
    }
}




/* RankAnnouncer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            AudioLoader.cs for loading/caching/storing/fetching audio
            InstanceConfig.cs for setting
            JsonSetting.cs for setting */
