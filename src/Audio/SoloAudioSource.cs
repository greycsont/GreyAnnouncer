using UnityEngine;

using greycsont.GreyAnnouncer;

public sealed class SoloAudioSource : MonoBehaviour
{
    private        AudioSource     m_audioSource;
    private static SoloAudioSource m_instance;

    #region Constructor
    public static SoloAudioSource Instance
    {
        get
        {
            if (m_instance == null)
            {
                var obj  = new GameObject("SoloAudioSource");
                DontDestroyOnLoad(obj);
                m_instance = obj.AddComponent<SoloAudioSource>();
            }
            return m_instance;
        }
    }
    #endregion


    #region Public API
    public void PlayOneShot(AudioClip clip, AudioSourceSetting config)
    {
        if (m_audioSource == null)
        {
            m_audioSource = gameObject.AddComponent<AudioSource>();
            m_audioSource = AudioSourceManager.ConfigureAudioSource(m_audioSource, config);
        }

        m_audioSource.clip = clip;
        UnderwaterController_inWater_Instance.CheckIsInWater();
        m_audioSource.Play();
    }

    public void AddAudioLowPassFilter()
    {
        m_audioSource = AudioSourceManager.AddLowPassFilter(m_audioSource);
    }

    public void RemoveAudioLowPassFilter()
    {
        m_audioSource = AudioSourceManager.RemoveLowPassFilter(m_audioSource);
    }

    public void UpdateSoloAudioSourceVolume(float targetVolume, float duration = 0.35f)
    {
        if (m_audioSource != null)
        {                          
            StartCoroutine(AudioSourceManager.FadeVolume(m_audioSource, targetVolume, duration));
        }
    }
    #endregion
}        