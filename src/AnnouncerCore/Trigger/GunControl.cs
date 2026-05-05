using HarmonyLib;

using GreyAnnouncer.Announcers;

namespace GreyAnnouncer.AnnouncerCore;

[HarmonyPatch(typeof(GunControl))]
public static class GunControlPatcher
{
    private static int slotIndex;
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GunControl.SwitchWeapon))]
    public static void SwitchWeaponPrefix(GunControl __instance)
    {
        slotIndex = __instance.currentSlotIndex;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GunControl.SwitchWeapon))]
    public static void SwitchWeaponPostfix(GunControl __instance)
    {
        if (slotIndex != __instance.currentSlotIndex)
        {
            // placeholder
            slotIndex = __instance.currentSlotIndex;
        }
    }
}