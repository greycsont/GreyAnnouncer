using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace greycsont.GreyAnnouncer{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin{
        private Harmony harmony;

        private void Awake()
        {
            Announcer.Initialize();
            harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

    }

   [HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]
    public static class StyleHUDUpdateMeterPatch
    {
        private static bool previousWasZero = true;

        static void Prefix(StyleHUD __instance)
        {
            
        }

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

    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]
    public static class StyleHUDAscendRankPatch{
        static void Postfix(StyleHUD __instance){
            Announcer.PlaySound(__instance.rankIndex);
        }
    }
}