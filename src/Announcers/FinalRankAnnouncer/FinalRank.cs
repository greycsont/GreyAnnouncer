using HarmonyLib;

namespace GreyAnnouncer.FinalRankAnnouncer;


[HarmonyPatch(typeof(FinalRank))]
public static class FinalRankPatcher
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FinalRank.FlashPanel))]
    public static void FlashPanelPatch()
    {
        LogHelper.LogInfo("FlashPanel Triggerd");
    }
}