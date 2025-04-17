using UnityEngine;
using System.Collections;


namespace greycsont.GreyAnnouncer
{

    public class AudioSourceManager
    {
        public static AudioSource AddLowPassFilter(AudioSource audioSource)
        {
            if (InstanceConfig.LowPassFilter_Enabled.Value == false) return audioSource;
            if (audioSource == null) return null;
            AudioLowPassFilter lowPassFilter = audioSource.gameObject.GetComponent<AudioLowPassFilter>()
                                               ?? audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency   = 1000f;
            lowPassFilter.lowpassResonanceQ = 1f;
            return audioSource;
        }

        public static AudioSource RemoveLowPassFilter(AudioSource audioSource)
        {
            if (audioSource == null) return null;
            AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter != null)
            {
                GameObject.Destroy(lowPassFilter);
            }
            return audioSource;
        }

        public static IEnumerator FadeVolume(AudioSource audioSource, float targetVolume, float duration)
        {
            if (audioSource == null) yield break;
            float startVolume = audioSource.volume;
            float timeStep    = duration / 5f;
            float time        = 0f;
            while (time < duration)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                time += timeStep;
                yield return new WaitForSeconds(timeStep);
            }
            audioSource.volume = targetVolume;
        }
    }
}