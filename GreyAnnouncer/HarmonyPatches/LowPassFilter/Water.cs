using System.ComponentModel;
using HarmonyLib;


namespace greycsont.GreyAnnouncer;

/* This patch is used to add and remove the LowPassFilter directly copied from ULTRAKILL and trigger by EnterWater() and OutWater() */

[Description("Because it's..... simple so I just put it at here instead of creating a new object file")]
public class Water
{
    public static bool isInWater = false;
    public static void CheckIsInWater()
    {
        if (InstanceConfig.LowPassFilter_Enabled.Value == false)
        {
            AudioSourcePool.Instance.RemoveAudioLowPassFilterFromActiveAudioSource();
            SoloAudioSource.Instance.RemoveAudioLowPassFilter();
        }
        else if (isInWater == true)
        {
            AudioSourcePool.Instance.AddAudioLowPassFilterToActiveAudioSource();
            SoloAudioSource.Instance.AddAudioLowPassFilter();
        }
    }
}


[HarmonyPatch(typeof(UnderwaterController), "EnterWater")]
public static class EnterWater_Patch
{
    public static void Postfix()
    {
        Water.isInWater = true;
        Water.CheckIsInWater();
    }
}
[HarmonyPatch(typeof(UnderwaterController), "OutWater")]
public static class OutWater_Patch
{
    public static void Postfix()
    {
        Water.isInWater = false;
        Water.CheckIsInWater();
    }
}

