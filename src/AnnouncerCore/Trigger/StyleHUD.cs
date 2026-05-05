using System.Collections.Generic;

using HarmonyLib;

using GreyAnnouncer.Announcers;

namespace GreyAnnouncer.AnnouncerCore;

[PatchOnEntry]
[HarmonyPatch(typeof(StyleHUD))]
public static class StyleHUDPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StyleHUD.AscendRank))]
    public static void GetNonDrankAscend(StyleHUD __instance)
    {
        var rank = __instance.rankIndex;
        if (rank is >= 0 and <= 7)
            RankAnnouncer.PlayRankSound(rank);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StyleHUD.ComboStart))]
    public static void GetDrank(StyleHUD __instance)
    {
        if (__instance.rankIndex == 0)
            RankAnnouncer.PlayRankSound(0);
    }
}
