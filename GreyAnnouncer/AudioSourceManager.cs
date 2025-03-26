using UnityEngine;

namespace greycsont.GreyAnnouncer{

    public class AudioSourceManager{
        public static AudioSource AddLowPassFilter(AudioSource audioSource){
            AudioLowPassFilter lowPassFilter = audioSource.gameObject.GetComponent<AudioLowPassFilter>()
                                               ?? audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 1000f;
            lowPassFilter.lowpassResonanceQ = 1f;
            return audioSource;
        }

        public static AudioSource RemoveLowPassFilter(AudioSource audioSource){
            AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter != null){
                Object.Destroy(lowPassFilter);
            }
            return audioSource;
        }
    }
}