using System.ComponentModel;
using HarmonyLib;


namespace greycsont.GreyAnnouncer;

/* This patch is used to add and remove the LowPassFilter directly copied from ULTRAKILL and trigger by EnterWater() and OutWater() */

[Description("Because it's..... related so I just put it at here instead of creating a new object file")]
public class UnderwaterController_inWater_Instance
{
    public static bool isInWater 
    {
        get 
        {
            var controller = global::UnderwaterController.Instance;
            return controller != null && controller.inWater;
        }
    }

    public static void CheckIsInWater()
    {
        if (isInWater == false || InstanceConfig.LowPassFilter_Enabled.Value == false)
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


[Description("Q : Why? A : Prevent when audio is playing and enter/out of the water")]
[HarmonyPatch(typeof(global::UnderwaterController), "EnterWater")]
public static class EnterWater_Patch
{
    public static void Postfix()
    {
        UnderwaterController_inWater_Instance.CheckIsInWater();
    }
}
[HarmonyPatch(typeof(global::UnderwaterController), "OutWater")]
public static class OutWater_Patch
{
    public static void Postfix()
    {
        UnderwaterController_inWater_Instance.CheckIsInWater();
    }
}

