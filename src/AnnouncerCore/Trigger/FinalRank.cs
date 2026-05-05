using HarmonyLib;
using UnityEngine;
using TMPro;

using GreyAnnouncer.Announcers;

namespace GreyAnnouncer.AnnouncerCore;

[HarmonyPatch(typeof(FinalRank))]
public static class FinalRankPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FinalRank.FlashPanel))]
    public static void FlashPanelPatch(FinalRank __instance)
    {
        var index = __instance.i;

        /*
         * Magic number
         * 6 = time | 10 = kills | 14 = style | 18 = final
         */
        TMP_Text rankText =
            index == 6  ? __instance.timeRank  :
            index == 10 ? __instance.killsRank :
            index == 14 ? __instance.styleRank :
            index == 18 ? __instance.totalRank :
            null;

        if (rankText == null) return;

        var rankIndex = rankText.text switch
        {
            var t when t.Contains(">D<") => 0,
            var t when t.Contains(">C<") => 1,
            var t when t.Contains(">B<") => 2,
            var t when t.Contains(">A<") => 3,
            var t when t.Contains(">S<") => 4,
            var t when t.Contains(">P<") => 5,
            _ => -1
        };
    
            // placeholder
    }
}