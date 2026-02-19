using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace GreyAnnouncer.RankAnnouncer;

/* This patch is used to determine the changes of rankIndex
    The rankIndex is basically the pointer to another arrays, list sth.
    More information in the Announcer.cs 
    StyleHUD.cs -> RankAnnouncer.cs */
    
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



/*[HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only, left for review silly code, why do you want to patch a Update() function? 
>>>>>>> 145108309970536fe7b8cd4ad469d6e00f945bb7
public static class StyleHUDUpdateMeter_Patch
{
    private static readonly AccessTools.FieldRef<StyleHUD, float> currentMeterRef = AccessTools.FieldRefAccess<StyleHUD, float>("currentMeter");
    private static          bool                                  previousWasZero = true;
    static void Postfix(StyleHUD __instance)
    {
        float currentMeter    = GetCurrentMeter(__instance);
        bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0;

        if (previousWasZero && currentIsNonZero)
        {
            RankAnnouncer.PlaySound(0);
        }

        previousWasZero = __instance.rankIndex == 0 && currentMeter <= 0;
    }

    

    private static float GetCurrentMeter(StyleHUD instance)
    {
        return currentMeterRef(instance);
    }

    private static float GetCurrentMeter(StyleHUD instance)
    {
        return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
    }
}*/

