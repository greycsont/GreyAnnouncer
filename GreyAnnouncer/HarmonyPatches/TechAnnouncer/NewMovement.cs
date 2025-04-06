using HarmonyLib;

namespace greycsont.GreyAnnouncer{
    
    [HarmonyPatch(typeof(NewMovement), "TrySSJ")]
    public static class TrySSJ_Patch
    {
        public static void Postfix(NewMovement __instance)
        {
            float frameSinceSlide = GetPrivateFloat(__instance, "frameSinceSlide");
            if ( frameSinceSlide > 0 && frameSinceSlide < GetPrivateFloat(__instance, "ssjMaxFrames") && !__instance.boost){
                Plugin.Log.LogInfo("SSJ Triggered!");
            }
        }

        private static float GetPrivateFloat(NewMovement instance, string variable)
        {
            return Traverse.Create(instance).Field(variable).GetValue<float>();
        }
    }
}
    