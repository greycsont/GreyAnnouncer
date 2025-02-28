using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace greycsont.GreyAnnouncer{

    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin{
        private Harmony harmony;

        private void Awake()
        {
            Announcer.Initialize();
            harmony = new Harmony(PluginInfo.GUID+".harmony");
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.GUID} is loaded!");
        }

    }

    /// <summary>
    /// Patch AscendRank
    /// </summary>
    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]
    public static class StyleHUDAscendRankPatch{
        public static void Postfix(StyleHUD __instance){
            int currentRankIndex = __instance.rankIndex;
            Debug.Log($"currentRankIndex: {currentRankIndex}");
            Announcer.PlaySound(currentRankIndex);
        }
    }
}
