using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

/* The StyleHUD.cs in the HarmonyPatches folder is the starting point of the whole sequence of announcer 
   But for the initialize of the program like loading audio or something, you should start from here */
namespace greycsont.GreyAnnouncer
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency(PluginDependencies.PLUGINCONFIGURATOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin{
        internal static ManualLogSource Log;
        private Harmony                 harmony;  // patch

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
            JsonManager.   Initialize();
            InstanceConfig.Initialize(this);
            Announcer.     Initialize();
        }
        
        private void LoadOptionalModule()
        {
            CheckPluginLoaded(PluginDependencies.PLUGINCONFIGURATOR_GUID, "greycsont.GreyAnnouncer.IPluginConfigurator");
        }

        private void PatchHarmony()
        {
            harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
            harmony.PatchAll();
        }

        public void CheckPluginLoaded(string GUID, string assemblyName)
        {
            if (!Chainloader.PluginInfos.ContainsKey(GUID)){
                Plugin.Log.LogWarning($"Plugin {GUID} not loaded, stopping loading {assemblyName}"); 
                return;
            }
            ReflectionManager.LoadByReflection(assemblyName);
        }   
    }
}