using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public sealed class SoloAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioSource _backgroundAudioSource;
    private AudioSource _sfxAudioSource;
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
    public void Play(Sound sound)
    {
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.Stop();
        
        ConfigureAndPlayAudioSource(sound, _audioSource);
    }

    public void PlayOneShot(Sound sound)
    {
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        ConfigureAndPlayAudioSource(sound, _audioSource);

    }

    private void ConfigureAndPlayAudioSource(Sound sound, AudioSource audioSource)
    {
        audioSource.spatialBlend = sound.SpatialBlend;
        audioSource.priority = sound.Priority;
        audioSource.volume = Mathf.Clamp01(BepInExConfig.audioSourceVolume.Value * sound.Volume);
        audioSource.pitch = UnityEngine.Random.Range(sound.Pitch[0], sound.Pitch[1]);
        audioSource.clip = sound.clip;
        audioSource = UnderwaterController_inWater_Instance.GetAudioSourceWithLowPassFilter(_audioSource);
        audioSource.PlayOneShot(sound.clip, sound.Volume);
    }

    private AudioSource GetAudioSource(Sound sound)
    {
        return _sfxAudioSource;
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
            StartCoroutine(AudioSourceManager.FadeVolume(_audioSource, targetVolume, duration));
    }

    public void StopAudioSource()
    {
        _audioSource.Stop();
        Destroy(_audioSource.clip);
    }
    #endregion
}        