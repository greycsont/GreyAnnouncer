using HarmonyLib;

namespace greycsont.GreyAnnouncer{


    
    [HarmonyPatch(typeof(NewMovement), "TrySSJ")]
    public static class TrySSJ_Patch
    {
        public static void Postfix(NewMovement __instance)
        {

        }
    }
}
    