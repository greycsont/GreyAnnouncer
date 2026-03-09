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
        if (panel != __instance.toAppear[__instance.i].transform.parent.GetChild(1).gameObject) return;
        LogHelper.LogDebug($"FinalRank.i(index) : {__instance.i}");
        var index = __instance.i;
        if (index == 0)
        {
            
        }
        if (index == 1)
        {
            
        }
        if (index == 2)
        {
            
        }
        if (index == 3)
        {
            
        }
    }
}