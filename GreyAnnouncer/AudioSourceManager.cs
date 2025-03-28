using UnityEngine;

namespace greycsont.GreyAnnouncer{

    public class AudioSourceManager{
        public static AudioSource AddLowPassFilter(AudioSource audioSource){
            if (InstanceConfig.LowPassFilter_Enabled.Value == false) return audioSource;
            if (audioSource == null) return null;
            AudioLowPassFilter lowPassFilter = audioSource.gameObject.GetComponent<AudioLowPassFilter>()
                                               ?? audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 1000f;
            lowPassFilter.lowpassResonanceQ = 1f;
            return audioSource;
        }

        public static AudioSource RemoveLowPassFilter(AudioSource audioSource){
            if (audioSource == null) return null;
            AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter != null){
                Object.Destroy(lowPassFilter);
            }
            return audioSource;
        }
    }
}