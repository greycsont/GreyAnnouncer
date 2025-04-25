using HarmonyLib;


    
namespace greycsont.GreyAnnouncer;

/* This patch is used to determine the changes of rankIndex
    The rankIndex is basically the pointer to another arrays, list sth.
    More information in the Announcer.cs 
    StyleHUD.cs -> Announcer.cs */

[HarmonyPatch(typeof(StyleHUD), "AscendRank")]  // For non-D ranks
public static class StyleHUDAscendRank_Patch
{
    static void Postfix(StyleHUD __instance)
    {  
        var rank = __instance.rankIndex;
        if (rank >= 0 && rank <= 7)
        {
            RankAnnouncer.PlaySound(rank);
        }
    }
}

[HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
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

    /*private static float GetCurrentMeter(StyleHUD instance)
    {
        return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
    }*/
}    
  