using HarmonyLib;

namespace greycsont.GreyAnnouncer{
    
    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]  // For non-D rank
        public static class StyleHUDAscendRankPatch{
            static void Postfix(StyleHUD __instance){
                Announcer.PlaySound(__instance.rankIndex);
            }
        }

    [HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
    public static class StyleHUDUpdateMeterPatch
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