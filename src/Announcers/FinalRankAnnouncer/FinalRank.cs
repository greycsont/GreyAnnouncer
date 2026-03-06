using HarmonyLib;
using UnityEngine;

namespace GreyAnnouncer.FinalRankAnnouncer;


[HarmonyPatch(typeof(FinalRank))]
public static class FinalRankPatcher
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FinalRank.FlashPanel))]
    public static void FlashPanelPatch(ref GameObject panel, FinalRank __instance)
    {
        if (panel == __instance.toAppear[__instance.i].transform.parent.GetChild(1).gameObject)
            LogHelper.LogInfo($"object name = {__instance.toAppear[__instance.i]}");
        LogHelper.LogInfo("FlashPanel Triggerd");
    }
}