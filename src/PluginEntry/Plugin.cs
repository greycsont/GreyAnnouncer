using BepInEx;
using HarmonyLib;
using UnityEngine;
using GreyAnnouncer.Util;
using GreyAnnouncer.Config;
using GreyAnnouncer.FrontEnd;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

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
        gameObject.AddComponent<UIFactory>();
    }

    private void LoadMainModule()
    {
        BepInExConfig.Initialize(this);
    }

    private void LoadOptionalModule()
    {
        LoadPatches();
    }

    private void LoadPatches()
    {
        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        var entryPointMethods = new List<MethodInfo>();

        foreach (var type in typeof(Plugin).Assembly.GetTypes())
        {
            if (type.GetCustomAttribute<PatchOnEntryAttribute>() != null)
            {
                try
                {
                    harmony.PatchAll(type);
                }
                catch (Exception e)
                {
                    LogHelper.LogError($"FUCK PATCHING {type.Name}, {e}");
                    throw;
                }
            }

            if (type.GetCustomAttribute<EntryPointAttribute>() != null)
            {
                var entries = type
                             .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                             .Where(static mi => mi.GetCustomAttribute<EntryPointAttribute>() != null).ToArray();
                switch (entries.Length)
                {
                    case > 1:
                        LogHelper.LogError($"Type {type.FullName} has multiple entry points defined. Only one is allowed.");
                        break;
                    case 0:
                        LogHelper.LogError($"Type {type.FullName} is marked as an entry point but has no methods marked with EntryPointAttribute.");
                        break;
                    case 1 when entries[0].GetParameters().Length != 0:
                        LogHelper.LogError($"Entry point method {type.FullName}.{entries[0].Name} must have no parameters.");
                        break;
                    case 1:
                        entryPointMethods.Add(entries[0]); // idc abt return values
                        break;
                }
            }
        }

        foreach (var method in entryPointMethods)
        {
            LogHelper.LogInfo($"Invoking entry point: {method.DeclaringType?.FullName}.{method.Name}");
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception e)
            {
                LogHelper.LogError(e);
            }
        }
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
