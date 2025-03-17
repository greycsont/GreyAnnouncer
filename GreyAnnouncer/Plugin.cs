using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System;
using System.Reflection;
using HarmonyLib;

namespace greycsont.GreyAnnouncer{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency(PluginDependencies.PLUGINCONFIGURATOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin{
        private Harmony harmony;  // patch
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = base.Logger;
            LoadMainModule();
            LoadOptionalModule();
            PatchHarmony();
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        private void LoadMainModule(){
            InstanceConfig.Initialize(this);
            Announcer.Initialize();
        }
        private void LoadOptionalModule(){
            CheckPluginLoaded(PluginDependencies.PLUGINCONFIGURATOR_GUID, "greycsont.GreyAnnouncer.IPluginConfigurator");
        }

        private void PatchHarmony(){
            harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
            harmony.PatchAll();
        }

        private void CheckPluginLoaded(string GUID, string assemblyName){
            if (!Chainloader.PluginInfos.ContainsKey(GUID)){
                Plugin.Log.LogWarning($"Plugin {GUID} not loaded, stopping loading {assemblyName}"); 
                return;
            }
            try 
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type configuratorType = assembly.GetType(assemblyName);
                MethodInfo initialize = configuratorType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
                initialize?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load module: {ex}");
            }
        }
    }



    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]  // For non-D rank
    public static class StyleHUDAscendRankPatch{
        static void Postfix(StyleHUD __instance){
            Announcer.PlaySound(__instance.rankIndex);
        }
    }

    [HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
    public static class StyleHUDUpdateMeterPatch
    {
        private static bool previousWasZero = true;
        static void Postfix(StyleHUD __instance)
        {
            float currentMeter = GetCurrentMeter(__instance);
            bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0;

            if (previousWasZero && currentIsNonZero)
            {
                Announcer.PlaySound(0);
            }

            previousWasZero = __instance.rankIndex == 0 && currentMeter <= 0;
        }

        private static float GetCurrentMeter(StyleHUD instance)
        {
            return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
        }
    }
}