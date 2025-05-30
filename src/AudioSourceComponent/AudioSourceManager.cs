using UnityEngine;
using System.Collections;
using GreyAnnouncer.AudioSourceComponent;


namespace GreyAnnouncer;

public static class AudioSourceManager
{
    public static AudioSource ConfigureAudioSource(AudioSource audioSource, 
                                                   AudioSourceSetting config)
    {
        audioSource.spatialBlend = config.SpatialBlend;
        audioSource.priority     = config.Priority;
        audioSource.volume       = config.Volume < 1f ? config.Volume : 1f;
        audioSource.pitch        = config.Pitch;
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

    public static IEnumerator FadeVolume(AudioSource audioSource, 
                                         float targetVolume, 
                                         float duration)
    {
        if (audioSource == null) yield break;
        float startVolume = audioSource.volume;
        float timeStep    = duration / 5f;
        float time        = 0f;
        while (time < duration) {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += timeStep;
            yield return new WaitForSeconds(timeStep);
        }
        audioSource.volume = targetVolume;
    }

    public static void StopAllAudioSource()
    {
        SoloAudioSource.Instance.StopAudioSource();
    }

}
