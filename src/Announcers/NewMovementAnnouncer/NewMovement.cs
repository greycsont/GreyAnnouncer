

using System.Collections.Generic;
using HarmonyLib;



namespace GreyAnnouncer.NewMovementAnnouncer;

[HarmonyPatch(typeof(NewMovement))]
public static class NewMovementPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NewMovement.GetHurt))]
    public static void DeathPatch(NewMovement __instance)
    {
        // This therotically could be equal to 0, but who knows what'll happen        
        if (__instance.hp <= 0)
            NewMovementAnnouncer.PlaySoundViaIndex(0);
    }
}