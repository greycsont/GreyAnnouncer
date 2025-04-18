using System.ComponentModel;
using HarmonyLib;

/* This patch is used to add and remove the LowPassFilter directly copied from ULTRAKILL and trigger by EnterWater() and OutWater() */
namespace greycsont.GreyAnnouncer
{

    [Description("Because it's..... simple so I just put it at here instead of creating a new object file")]
    public class Water
    {
        public static bool isInWater = false;
        public static void CheckIsInWater(bool isSettingEnabled)
        {
            if (isSettingEnabled == false)
            {
                Announcer.RemoveAudioLowPassFilter();
            }
            else if (isInWater == true)
            {
                Announcer.AddAudioLowPassFilter();
            }
        }
    }


    [HarmonyPatch(typeof(UnderwaterController), "EnterWater")]
    public static class EnterWater_Patch
    {
        public static void Postfix()
        {
            Water.isInWater = true;
            Announcer.AddAudioLowPassFilter();
        }
    }
    [HarmonyPatch(typeof(UnderwaterController), "OutWater")]
    public static class OutWater_Patch
    {
        public static void Postfix()
        {
            Water.isInWater = false;
            Announcer.RemoveAudioLowPassFilter();
        }
    }

}
