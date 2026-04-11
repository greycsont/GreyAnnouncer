using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace GreyAnnouncer.RankAnnouncer;

/* This patch is used to determine the changes of rankIndex
    The rankIndex is basically the pointer to another arrays, list sth.
    More information in the Announcer.cs 
    StyleHUD.cs -> RankAnnouncer.cs */


[PatchOnEntry]    
[HarmonyPatch(typeof(StyleHUD))]
public static class StyleHUDPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StyleHUD.AscendRank))]  // For non-D ranks
    public static void GetNonDrankAscend(StyleHUD __instance)
    {
        var rank = __instance.rankIndex;
        if (rank is >= 0 and <= 7)
        {
            RankAnnouncer.PlayRankSound(rank);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StyleHUD.ComboStart))] // For D ranks
    public static void GetDrank(StyleHUD __instance)
    {
        var rank = __instance.rankIndex;
        if (rank == 0)
        {
            RankAnnouncer.PlayRankSound(0);
        }
    }
}
