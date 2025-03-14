using BepInEx;
using BepInEx.Logging;
using System;
using HarmonyLib;

namespace greycsont.GreyAnnouncer{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin{
        private Harmony harmony;  // patch
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = base.Logger;
            LoadMainMod();
            GetOptionalMod();
            PatchHarmony();
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        private void LoadMainMod(){
            InstanceConfig.Initialize(this);
            Announcer.Initialize();
        }
        private void GetOptionalMod(){
            try{
                IPluginConfigurator.Initialize();
            }catch(Exception ex){
                Log.LogWarning($"skip to load optional mod: {ex}");
            }   
        }

        private void PatchHarmony(){
            harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
            harmony.PatchAll();
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