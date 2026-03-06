using HarmonyLib;

namespace GreyAnnouncer.FinalRankAnnouncer;


[HarmonyPatch(typeof(FinalRank))]
public static class FinalRankPatcher
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FinalRank.FlashPanel))]
    public static void FlashPanelPatch(FinalRank __instance)
    {
        LogHelper.LogInfo("FlashPanel Triggerd");
    }
}