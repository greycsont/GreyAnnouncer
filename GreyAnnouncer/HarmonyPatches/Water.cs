using HarmonyLib;

/* This patch is used to determine does the audiosource need to add LowPassFilter or not */
namespace greycsont.GreyAnnouncer{
    [HarmonyPatch(typeof(Water), "AddLowPassFilters")]
    public static class AddLowPassFilters_Patch
    {
        public static void Postfix()
        {

        }
    }
    [HarmonyPatch(typeof(Water), "RemoveLowPassFilters")]
    public static class RemoveLowPassFilters_Patch
    {
        public static void Postfix()
        {

        }
    }
}