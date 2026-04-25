using UnityEngine;
using System.Collections;

using GreyAnnouncer.Config;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioSourceExtension
{
    extension(AudioSource value)
    {
        // The values of lowPassFilter are directly copied from ULTRAKIL
        public AudioSource AddLowPassFilter()
        {
            if (value == null) return null;
            AudioLowPassFilter lowPassFilter = value.gameObject.GetComponent<AudioLowPassFilter>()
                                                ?? value.gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency   = 1000f;
            lowPassFilter.lowpassResonanceQ = 1f;
            return value;
        }

        public AudioSource RemoveLowPassFilter()
        {
            if (value == null)
                return null;
            AudioLowPassFilter lowPassFilter = value.GetComponent<AudioLowPassFilter>(); 
            if (lowPassFilter != null)
                GameObject.Destroy(lowPassFilter);

            return value;
        }

        public IEnumerator FadeVolume(float duration, float? targetVolume = null, float? multiplier = null)
        {
            if (value == null) yield break;

            float startVolume = value.volume;
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
                value.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
                time += timeStep;
                yield return new WaitForSeconds(timeStep);
            }

            value.volume = endVolume;
        }

        public void ConfigureAndPlayAudioSource(Sound sound)
        {
            if (MonoSingleton<NewMovement>.Instance?.dead == true) return;
            
            value.SetSpatialBlend(Mathf.Clamp01(Setting.spatialBlend));
            value.priority = sound.priority;
            value.clip = sound.clip;
            value = UnderwaterController_inWater_Instance.GetAudioSourceWithLowPassFilter(value);
            value.volume = Setting.audioSourceVolume;
            value.PlayOneShot(sound.clip, sound.volume, true);
        }
    }


}