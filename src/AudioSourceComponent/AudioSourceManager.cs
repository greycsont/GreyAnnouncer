using UnityEngine;
using System.Collections;
using System;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioSourceManager
{
    public static AudioSource ConfigureAudioSource(AudioSource audioSource, 
                                                   Sound sound)
    {
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.priority     = sound.priority;
        audioSource.volume       = sound.volume < 1f ? sound.volume : 1f;
        audioSource.clip         = sound.clip;
        //audioSource.outputAudioMixerGroup = null;
        return audioSource;
    }

    // The values of lowPassFilter are directly copied from ULTRAKILL
    //
    public static AudioSource AddLowPassFilter(AudioSource audioSource)
    {
        if (audioSource == null) return null;
        AudioLowPassFilter lowPassFilter = audioSource.gameObject.GetComponent<AudioLowPassFilter>()
                                            ?? audioSource.gameObject.AddComponent<AudioLowPassFilter>();
        lowPassFilter.cutoffFrequency   = 1000f;
        lowPassFilter.lowpassResonanceQ = 1f;
        return audioSource;
    }

    public static AudioSource RemoveLowPassFilter(AudioSource audioSource)
    {
        if (audioSource == null)
            return null;
        AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>(); 
        if (lowPassFilter != null)
            GameObject.Destroy(lowPassFilter);

        return audioSource;
    }

    public static IEnumerator FadeVolume(AudioSource audioSource, float duration, float? targetVolume = null, float? multiplier = null)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float endVolume;

        if (targetVolume.HasValue)
            endVolume = Mathf.Clamp01(targetVolume.Value);
        else if (multiplier.HasValue)
            endVolume = Mathf.Clamp01(startVolume * multiplier.Value);
        else
            yield break;

        float timeStep = duration / 5f;
        float time = 0f;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += timeStep;
            yield return new WaitForSeconds(timeStep);
        }

        audioSource.volume = endVolume;
    }

    public static void StopAllAudioSource()
        => SoloAudioSource.Instance.StopAudioSource();

}
