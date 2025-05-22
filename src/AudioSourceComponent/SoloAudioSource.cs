using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public sealed class SoloAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private static SoloAudioSource _instance;

    #region Constructor
    public static SoloAudioSource Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("SoloAudioSource");
                DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<SoloAudioSource>();
            }
            return _instance;
        }
    }
    #endregion


    #region Public API
    public void PlayOneShot(AudioClip clip, AudioSourceSetting config)
    {
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource = AudioSourceManager.ConfigureAudioSource(_audioSource, config);
        }

        _audioSource.clip = clip;
        UnderwaterController_inWater_Instance.CheckIsInWater();
        _audioSource.Play();
    }

    public void AddAudioLowPassFilter()
    {
        _audioSource = AudioSourceManager.AddLowPassFilter(_audioSource);
    }

    public void RemoveAudioLowPassFilter()
    {
        _audioSource = AudioSourceManager.RemoveLowPassFilter(_audioSource);
    }

    public void UpdateSoloAudioSourceVolume(float targetVolume, float duration = 0.35f)
    {
        if (_audioSource != null)
        {
            StartCoroutine(AudioSourceManager.FadeVolume(_audioSource, targetVolume, duration));
        }
    }

    public void StopAudioSource()
    {
        _audioSource.Stop();
        Destroy(_audioSource.clip);
    }
    #endregion
}        