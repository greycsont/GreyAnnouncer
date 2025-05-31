using System.ComponentModel;
using HarmonyLib;
using UnityEngine;


namespace GreyAnnouncer.AudioSourceComponent;

/* This patch is used to add and remove the LowPassFilter directly copied from ULTRAKILL and trigger by EnterWater() and OutWater() */

[Description("Because it's..... related so I just put it at here instead of creating a new object file")]
public static class UnderwaterController_inWater_Instance
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
        if (isInWater == false || InstanceConfig.isLowPassFilterEnabled.Value == false)
        {
            RemoveAudioLowPassFilterFromAllAudioSource();
        }
        else if (isInWater == true)
        {
            AddAudioLowPassFilterToAllAudioSource();
        }
    }

    public static AudioSource GetAudioSourceWithLowPassFilter(AudioSource audioSource)
    {
        if (audioSource == null) return null;
        if (isInWater && InstanceConfig.isLowPassFilterEnabled.Value == true)
        {
            audioSource = AudioSourceManager.AddLowPassFilter(audioSource);
        }
        else
        {
            audioSource = AudioSourceManager.RemoveLowPassFilter(audioSource);
        }

        return audioSource;
    }

    private static void RemoveAudioLowPassFilterFromAllAudioSource()
    {
        SoloAudioSource.Instance.RemoveAudioLowPassFilter();
    }

    private static void AddAudioLowPassFilterToAllAudioSource()
    {
        SoloAudioSource.Instance.AddAudioLowPassFilter();
    }
}


[Description("Q : Why? A : Prevent when audio is playing and enter/out of the water")]
[HarmonyPatch(typeof(UnderwaterController), "EnterWater")]
public static class EnterWater_Patch
{
    public static void Postfix()
    {
        UnderwaterController_inWater_Instance.CheckIsInWater();
    }
}
[HarmonyPatch(typeof(UnderwaterController), "OutWater")]
public static class OutWater_Patch
{
    public static void Postfix()
    {
        UnderwaterController_inWater_Instance.CheckIsInWater();
    }
}

