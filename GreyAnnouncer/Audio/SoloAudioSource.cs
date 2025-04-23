using UnityEngine;

using greycsont.GreyAnnouncer;

public class SoloAudioSource : MonoBehaviour
{
    private        AudioSource     audioSource;
    private static SoloAudioSource instance;

    public static SoloAudioSource Instance
    {
        get
        {
            if (instance == null)
            {
                var obj  = new GameObject("SoloAudioSource");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<SoloAudioSource>();
            }
            return instance;
        }
    }

    public void PlayOverridable(AudioClip clip, AudioSourceConfiguration config)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource = AudioSourceManager.ConfigureAudioSource(audioSource, config);
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void AddAudioLowPassFilter()
    {
        audioSource = AudioSourceManager.AddLowPassFilter(audioSource);
    }

    public void RemoveAudioLowPassFilter()
    {
        audioSource = AudioSourceManager.RemoveLowPassFilter(audioSource);
    }

    public void UpdateSoloAudioSourceVolume(float targetVolume, float duration = 0.35f)
    {
        if (audioSource != null)
        {                          
            StartCoroutine(AudioSourceManager.FadeVolume(audioSource, targetVolume, duration));
        }
    }
}        