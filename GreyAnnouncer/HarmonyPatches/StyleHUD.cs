using HarmonyLib;

/* This patch is used to determine the changes of rankIndex
    The rankIndex is basically the pointer to another arrays, list sth.
    More information in the Announcer.cs 
    StyleHUD.cs -> Announcer.cs */
    
namespace greycsont.GreyAnnouncer{
    
    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]  // For non-D ranks
    public static class StyleHUDAscendRank_Patch{
        static void Postfix(StyleHUD __instance){  
            Announcer.PlaySound(__instance.rankIndex);
        }
    }

    [HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
    public static class StyleHUDUpdateMeter_Patch
    {
        private static bool previousWasZero = true;
        static void Postfix(StyleHUD __instance)
        {
            float currentMeter = GetCurrentMeter(__instance);
            bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0;

            if (previousWasZero && currentIsNonZero)
            {
                Announcer.PlaySound(0);
            }

            previousWasZero = __instance.rankIndex == 0 && currentMeter <= 0;
        }

        private static float GetCurrentMeter(StyleHUD instance)
        {
            return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
        }
    }    
}    